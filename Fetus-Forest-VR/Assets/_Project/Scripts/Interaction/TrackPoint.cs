using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TrackPoint : MonoBehaviour
{
	[Header("输入")] public PlayerInputActions inputActions;

	[Header("位移根与玩家")]
	[SerializeField] private Transform motionRoot;
	[SerializeField] private Transform playerTransform;

	[Header("出现与位移参数")]
	[SerializeField] private float curveMoveDuration = 5.0f;
	[SerializeField] private float arcHeight = 1.0f;
	[SerializeField] private AnimationCurve progressCurve = null;

	[Header("初始出现位置")]
	[SerializeField] private bool useCustomSpawnPosition = false; // 勾选则使用自定义坐标/Transform
	[SerializeField] private Transform customSpawnTransform;

	[Header("目标序列")]
	[SerializeField] private Transform[] targetPositions;
	[SerializeField] private bool loopTargets = false;

	[Header("拾取判定（触发器）")]
	[SerializeField] private LayerMask playerLayer = 1;
	[SerializeField] private SphereCollider pickupCollider; // 勾选 isTrigger
	[SerializeField] private bool useTriggerPickup = true; // 使用触发器进行判定

	[Header("拾取UI（Image）")]
	[SerializeField] private GameObject promptUI; // UI根，包含Image
	[SerializeField] private Image promptImage;
	[SerializeField] private CanvasGroup promptCanvasGroup;
	[SerializeField] private float promptFadeInDuration = 0.3f;
	[SerializeField] private float promptFadeOutDuration = 0.2f;

	[Header("Intro动画等待")]
	[SerializeField] private bool waitIntroThenMove = true; // 激活后是否等待Intro动画结束再移动
	[SerializeField] private int introLayerIndex = 0; // 动画层
	[SerializeField] private float introMinDuration = 0.05f; // 最小等待时间，避免瞬切
	[SerializeField] private float introTimeout = 5f; // 等待超时

	[Header("速度控制")]
	[SerializeField] private bool useDistanceBasedDuration = true; // 按距离换算位移时长
	[SerializeField] private float secondsPerUnit = 0.6f; // 每米所需秒数，值越大越慢
	[SerializeField] private float minCurveDuration = 0.5f; // 最小时长
	[SerializeField] private float durationScale = 1.0f; // 全局时长倍率（>1 更慢）
	

	[Header("最终位置消失动画")]
	[SerializeField] private string intoEyeTriggerParam = "IntoEye"; // 消失动画的触发器参数
	[SerializeField] private bool waitDisappearThenDisable = true; // 是否等待消失动画播放完毕再禁用物体
	[SerializeField] private int disappearLayerIndex = 0; // 消失动画所在的Animator层索引
	[SerializeField] private float disappearMinDuration = 0.05f; // 等待消失动画的最小时间
	[SerializeField] private float disappearTimeout = 5f; // 等待消失动画的最大超时时间

	[SerializeField] private ConnectionLineController connectionLine; // Line Connection 控制器
	
	[Header("Silk UI 控制")]
	[SerializeField] private SilkUIController silkUIController; // Silk UI 控制器引用



	// 私有变量
	private int currentTargetIndex = -1;
	private bool isPickupInProgress = false;
	private bool isPlayerNearby = false;
	private bool isPromptVisible = false;
	private bool canPickup = false; 
	private bool isAtLastPosition = false; // 是否在最后一个目标位置
	private bool isDisappearing = false; // 是否正在播放消失动画
	private bool isMoving = false; // 是否正在移动
	private Animator animator;

	private void Awake()
	{
		if (motionRoot == null) motionRoot = transform;
		inputActions = new PlayerInputActions();
		animator = GetComponent<Animator>();
		if (animator == null) animator = GetComponentInChildren<Animator>();
		if (pickupCollider == null) pickupCollider = GetComponent<SphereCollider>();
	}

	private void OnEnable()
	{
		inputActions.Enable();
		inputActions.Player.PickUp.performed += OnPickupInput;
		
		// 立即设置初始位置
		Vector3 initialSpawnPos;
		if (useCustomSpawnPosition)
		{
			initialSpawnPos = customSpawnTransform.position;
		}
		else
		{
			initialSpawnPos = motionRoot.position; // 保持当前位置
		}
		motionRoot.position = initialSpawnPos;

		canPickup = false; // 初始不允许拾取
		HidePromptUI(); // 初始隐藏UI

		// 初始化并激活连接线（确保TrackPoint已就位）
		if (connectionLine != null)
		{
			connectionLine.Activate();
		}
		
		// 通知 Silk UI 第一次激活
		if (silkUIController != null)
		{
			silkUIController.OnFirstActivation();
		}

		StartCoroutine(SpawnSequence());
	}

	private void OnDisable()
	{
		inputActions.Disable();
		inputActions.Player.PickUp.performed -= OnPickupInput;
		StopAllCoroutines(); // 禁用时停止所有协程
		HidePromptUI(); // 禁用时隐藏UI

		if (connectionLine != null)
		{
			connectionLine.Deactivate();
		}
	}

	private IEnumerator SpawnSequence()
	{
		// 等待Intro动画播放完成
		if (waitIntroThenMove && animator != null)
		{
			yield return WaitForIntroAnimation();
		}

		// 直接从出现位置沿曲线到第一个目标
		Transform firstTarget = GetNextTarget();
		if (firstTarget != null)
		{
			// 通知 Silk UI 本轮的锁定目标位置
			if (silkUIController != null)
			{
				silkUIController.SetLockedTargetPosition(firstTarget.position);
			}

			isMoving = true; // 开始移动
			Debug.Log($"[TrackPoint] 开始移动到第一个目标位置");
			
			float d = Vector3.Distance(motionRoot.position, firstTarget.position);
			float dur = useDistanceBasedDuration ? Mathf.Max(minCurveDuration, d * secondsPerUnit) : curveMoveDuration;
			dur *= Mathf.Max(0.0001f, durationScale);
			yield return MoveAlongArc(motionRoot.position, firstTarget.position, arcHeight, dur);
			currentTargetIndex = 0;
			motionRoot.position = firstTarget.position;
			
			// 检查是否到达最后一个目标位置（如果只有一个目标位置）
			isAtLastPosition = (targetPositions.Length == 1);
			
			isMoving = false; // 移动结束
			Debug.Log($"[TrackPoint] 到达第一个目标位置，移动结束");
		}

		// 首段自动移动完成，允许拾取（除非在最后位置）
		canPickup = !isAtLastPosition;
		
		// 如果在最后位置，记录日志但不做特殊处理
		if (isAtLastPosition)
		{
			Debug.Log($"[TrackPoint] 初始生成时已在最后一个目标位置，拾取功能已禁用");
		}
		
		// 如果玩家在附近且现在可以拾取，显示提示UI
		if (canPickup && isPlayerNearby && !isMoving)
		{
			ShowPromptUI();
		}
	}

	private IEnumerator WaitForIntroAnimation()
	{
		// 最小等待，避免瞬切
		yield return new WaitForSeconds(introMinDuration);

		if (animator != null)
		{
			var clips = animator.GetCurrentAnimatorClipInfo(introLayerIndex);
			if (clips != null && clips.Length > 0 && clips[0].clip != null)
			{
				float speed = Mathf.Abs(animator.speed) < 0.0001f ? 1f : Mathf.Abs(animator.speed);
				float waitSeconds = clips[0].clip.length / speed;
				float remaining = Mathf.Clamp(waitSeconds - introMinDuration, 0f, introTimeout);
				if (remaining > 0f)
				{
					yield return new WaitForSeconds(remaining);
				}
				yield break;
			}
		}
		// 没拿到片段就按超时兜底
		yield return new WaitForSeconds(introTimeout);
	}

	private void OnPickupInput(InputAction.CallbackContext _)
	{
		if (isPickupInProgress) return;
		if (!canPickup) return; // 首段完成后才允许
		if (!isPlayerNearby) return; // 仅当触发器内才允许拾取
		
		// 通知 Silk UI 拾取事件
		if (silkUIController != null)
		{
			silkUIController.OnPickup();
		}

		// 触发连线脉冲效果
		if (connectionLine != null)
		{
			connectionLine.Pulse();
		}


		
		StartCoroutine(NextHop());
	}

	private IEnumerator NextHop()
	{
		isPickupInProgress = true;
		HidePromptUI(); // 拾取后隐藏UI

		Transform next = GetNextTarget();
		if (next != null)
		{
			// 通知 Silk UI 本轮的锁定目标位置
			if (silkUIController != null)
			{
				silkUIController.SetLockedTargetPosition(next.position);
			}

			isMoving = true; // 开始移动
			Debug.Log($"[TrackPoint] 开始移动到下一个目标位置");
			
			float d = Vector3.Distance(motionRoot.position, next.position);
			float dur = useDistanceBasedDuration ? Mathf.Max(minCurveDuration, d * secondsPerUnit) : curveMoveDuration;
			dur *= Mathf.Max(0.0001f, durationScale);
			yield return MoveAlongArc(motionRoot.position, next.position, arcHeight, dur);
			currentTargetIndex = GetClampedNextIndex();
			motionRoot.position = next.position; // 确保精确位置
			
			// 检查是否到达最后一个目标位置
			isAtLastPosition = (currentTargetIndex == targetPositions.Length - 1);
			
			// 如果在最后一个位置，禁用拾取功能
			if (isAtLastPosition)
			{
				canPickup = false;
				HidePromptUI(); // 隐藏提示UI
				Debug.Log($"[TrackPoint] 已到达最后一个目标位置 (索引: {currentTargetIndex})，拾取功能已禁用");
			}
			
			isMoving = false; // 移动结束
			Debug.Log($"[TrackPoint] 到达目标位置，移动结束");
		}
		isPickupInProgress = false;
		// 只有在非最后位置且玩家在附近时才显示UI
		if (!isAtLastPosition && isPlayerNearby && !isMoving) ShowPromptUI();
	}

	private Transform GetNextTarget()
	{
		if (targetPositions == null || targetPositions.Length == 0) return null;
		int nextIndex = GetClampedNextIndex();
		return targetPositions[Mathf.Clamp(nextIndex, 0, targetPositions.Length - 1)];
	}

	private int GetClampedNextIndex()
	{
		if (targetPositions == null || targetPositions.Length == 0) return 0;
		int next = currentTargetIndex + 1;
		if (next >= targetPositions.Length)
		{
			return loopTargets ? 0 : targetPositions.Length - 1;
		}
		return next;
	}

	private IEnumerator MoveAlongArc(Vector3 from, Vector3 to, float height, float duration)
	{
		if (duration <= 0f) duration = 0.01f;
		float elapsed = 0f;
		Vector3 mid = (from + to) * 0.5f + Vector3.up * height;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float p = progressCurve != null ? progressCurve.Evaluate(t) : Mathf.SmoothStep(0f, 1f,t);
			Vector3 a = Vector3.Lerp(from, mid, p);
			Vector3 b = Vector3.Lerp(mid, to, p);
			motionRoot.position = Vector3.Lerp(a, b, p);
			
			// 移动过程中隐藏提示UI（如果正在显示）
			if (isPromptVisible)
			{
				HidePromptUI();
			}
			
			yield return null;
		}
		motionRoot.position = to;
	}

	#region 触发器拾取判定
	private void OnTriggerEnter(Collider other)
	{
		if (!useTriggerPickup) return;
		if (pickupCollider == null || !pickupCollider.enabled) return;
		if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
		if (other.isTrigger && !pickupCollider.isTrigger) return;

		Debug.Log($"[TrackPoint] Player进入触发器范围，当前状态: 最后位置={isAtLastPosition}, 允许拾取={canPickup}, 正在消失={isDisappearing}, 正在移动={isMoving}");
		isPlayerNearby = true;
		
		// 只有在静止状态且允许拾取时才显示提示UI
		if (canPickup && !isMoving)
		{
			ShowPromptUI();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!useTriggerPickup) return;
		if (pickupCollider == null || !pickupCollider.enabled) return;
		if (!other || ((1 << other.gameObject.layer) & playerLayer) == 0) return;
		if (other.isTrigger && !pickupCollider.isTrigger) return;

		Debug.Log($"[TrackPoint] Player离开触发器范围");
		isPlayerNearby = false;
		// 无论是否在移动，都隐藏提示UI
		HidePromptUI();
	}
	#endregion

	#region 提示UI控制
	private void ShowPromptUI()
	{
		if (promptUI == null || isPromptVisible) return;
		promptUI.SetActive(true);
		StartCoroutine(FadeInPrompt());
	}

	private void HidePromptUI()
	{
		if (promptUI == null || !isPromptVisible) return;
		StartCoroutine(FadeOutPrompt());
	}

	private IEnumerator FadeInPrompt()
	{
		if (promptCanvasGroup == null) yield break;
		isPromptVisible = true;
		float t = 0f;
		while (t < promptFadeInDuration)
		{
			t += Time.deltaTime;
			float a = Mathf.Lerp(0f, 1f, t / Mathf.Max(0.0001f, promptFadeInDuration));
			promptCanvasGroup.alpha = a;
			yield return null;
		}
		promptCanvasGroup.alpha = 1f;
	}

	private IEnumerator FadeOutPrompt()
	{
		if (promptCanvasGroup == null) yield break;
		float t = 0f;
		while (t < promptFadeOutDuration)
		{
			t += Time.deltaTime;
			float a = Mathf.Lerp(1f, 0f, t / Mathf.Max(0.0001f, promptFadeOutDuration));
			promptCanvasGroup.alpha = a;
			yield return null;
		}
		promptCanvasGroup.alpha = 0f;
		promptUI.SetActive(false);
		isPromptVisible = false;
	}
	#endregion

	#region 消失动画处理

	private IEnumerator DisappearSequence()
	{
		if (isDisappearing) yield break;
		
		isDisappearing = true;
		Debug.Log($"[TrackPoint] 消失动画序列开始，设置 {intoEyeTriggerParam} = true");

		// 隐藏/停用连接线
		if (connectionLine != null)
		{
			connectionLine.SetVisible(false); // 或 connectionLine.Deactivate();
		}
		
		// 通知 Silk UI 开始最终消失
		if (silkUIController != null)
		{
			silkUIController.OnFinalDisappear();
		}
		
		// 设置IntoEye为true，触发消失动画
		if (animator != null && !string.IsNullOrEmpty(intoEyeTriggerParam))
		{
			animator.SetTrigger(intoEyeTriggerParam);
			
			// 等待一小段时间让转换完成
			yield return new WaitForSeconds(0.1f);
			
			// 等待状态转换完成，检测消失动画是否开始播放
			yield return StartCoroutine(WaitForTransitionToDisappear());
		}

		// 等待消失动画播放完毕
		if (waitDisappearThenDisable && animator != null)
		{
			// 最小等待，避免瞬切
			yield return new WaitForSeconds(disappearMinDuration);

			// 获取当前播放的动画片段
			var clips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);
			if (clips != null && clips.Length > 0 && clips[0].clip != null)
			{
				float clipLength = clips[0].clip.length;
				float speed = Mathf.Abs(animator.speed) < 0.0001f ? 1f : Mathf.Abs(animator.speed);
				float waitSeconds = clipLength / speed;
				
				Debug.Log($"[TrackPoint] 消失动画播放中，长度: {clipLength:F2}秒");
				
				// 等待动画播放完毕，但不超过超时时间
				float remaining = Mathf.Clamp(waitSeconds - disappearMinDuration, 0f, disappearTimeout);
				if (remaining > 0f)
				{
					yield return new WaitForSeconds(remaining);
				}
			}
			else
			{
				// 如果无法获取动画片段信息，使用超时时间作为兜底
				yield return new WaitForSeconds(disappearTimeout);
			}
		}

		Debug.Log($"[TrackPoint] 消失动画播放完毕");
	}

	#region 外部触发接口

	/// <summary>
	/// 外部触发消失动画的公共方法
	/// </summary>
	/// <param name="triggerParamName">触发器参数名</param>
	public void TriggerDisappearAnimation(string triggerParamName = null)
	{
		if (isDisappearing)
		{
			Debug.LogWarning("[TrackPoint] 消失动画已在播放中，忽略重复触发");
			return;
		}
		
		// 如果指定了参数名，临时使用它；否则使用默认的intoEyeTriggerParam
		string paramToUse = !string.IsNullOrEmpty(triggerParamName) ? triggerParamName : intoEyeTriggerParam;
		
		Debug.Log($"[TrackPoint] 外部触发消失动画，参数: {paramToUse}");
		
		// 直接开始消失序列
		StartCoroutine(DisappearSequence());
	}

	#endregion

	private IEnumerator WaitForTransitionToDisappear()
	{
		float startTime = Time.time;
		float timeout = 1.0f; // 1秒超时
		string lastClipName = "";
		
		while (Time.time - startTime < timeout)
		{
			var currentClips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);
			if (currentClips.Length > 0)
			{
				string currentClipName = currentClips[0].clip.name;
				
				// 如果动画片段发生变化，说明转换已完成
				if (currentClipName != lastClipName)
				{
					lastClipName = currentClipName;
					
					// 如果切换到消失动画，转换完成
					if (currentClipName.Contains("Disappear") || currentClipName.Contains("消失"))
					{
						Debug.Log($"[TrackPoint] 状态转换完成，开始播放消失动画");
						yield break;
					}
				}
			}
			
			yield return new WaitForSeconds(0.05f); // 每0.05秒检查一次
		}
		
		Debug.LogWarning($"[TrackPoint] 等待状态转换超时");
	}
	
	#endregion

	#region 调试辅助方法

	[ContextMenu("调试Animator状态")]
	private void DebugAnimatorStates()
	{
		if (animator == null)
		{
			Debug.LogWarning("[TrackPoint] Animator组件不存在");
			return;
		}

		Debug.Log($"[TrackPoint] === Animator调试信息 ===");
		Debug.Log($"[TrackPoint] 总层数: {animator.layerCount}");
		Debug.Log($"[TrackPoint] 当前层索引: {disappearLayerIndex}");
		Debug.Log($"[TrackPoint] IntoEye参数名: {intoEyeTriggerParam}");
		
		// 检查IntoEye参数是否存在
		bool hasIntoEyeParam = false;
		foreach (var param in animator.parameters)
		{
			if (param.name == intoEyeTriggerParam)
			{
				hasIntoEyeParam = true;
				Debug.Log($"[TrackPoint] 找到参数: {param.name} (类型: {param.type})");
				break;
			}
		}
		
		if (!hasIntoEyeParam)
		{
			Debug.LogError($"[TrackPoint] 未找到参数: {intoEyeTriggerParam}");
		}

		// 显示当前状态信息
		var currentState = animator.GetCurrentAnimatorStateInfo(disappearLayerIndex);
		var currentClips = animator.GetCurrentAnimatorClipInfo(disappearLayerIndex);
		
		Debug.Log($"[TrackPoint] 当前动画片段: {(currentClips.Length > 0 ? currentClips[0].clip.name : "无")}");
		Debug.Log($"[TrackPoint] 动画进度: {currentState.normalizedTime:F2}");
		Debug.Log($"[TrackPoint] =========================");
	}

	#endregion
}
