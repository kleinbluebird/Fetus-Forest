using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class SilkUIController : MonoBehaviour
{
	[Header("UI 引用")]
	[SerializeField] private Image silkImage;
	
	[Header("Alpha / 上限与距离映射")]
	[SerializeField] private float initialAlpha = 0f;              // 初始 Alpha 值
	[SerializeField] private float firstActivationAlpha = 0.05f;   // 第一次激活后的最大上限
	[SerializeField] private float pickupIncrement = 0.15f;        // 每次拾取提升的最大上限
	[SerializeField] private float finalAlpha = 1f;                // 最终阶段上限
	[SerializeField] private float alphaLerpSpeed = 8f;            // Alpha 平滑速度

	[Header("距离参考（玩家实时 + 目标固定)")]
	[SerializeField] private Transform playerTransform;
	[SerializeField] private float proximityScale = 1.0f;          // 接近度缩放（越大衰减越快）

	// 每次拾取锁定的目标点位置（由外部在拾取时设置）
	private Vector3 lastPickupTargetPosition;
	private bool hasLockedTarget;
	private float previousDistance = -1f;

	[Header("缩放呼吸（替代Alpha呼吸)")]
	[SerializeField] private bool enableScaleBreath = true;
	[SerializeField] private float scaleMin = 1.0f;
	[SerializeField] private float scaleMax = 1.1f;
	[SerializeField] private float scaleCycleSeconds = 2.0f;
	
	[Header("接近增益设置")]
	[SerializeField] private float gainPerMeter = 0.5f; // 每接近1米带来的Alpha增益系数
	[SerializeField] private float approachEpsilon = 0.02f; // 判定“在接近”的最小距离变化（米）
	
	[Header("调试")]
	[SerializeField] private bool debugAlphaLog = false;
	[SerializeField] private int debugEveryNFrames = 10;
	
	private float currentAlpha;           // 当前 Alpha 值
	private float targetAlpha;            // 由逻辑计算出的当前目标 Alpha（受距离与上限影响）
	private int pickupCount;             // 拾取次数
	private bool isFinalPhase;           // 是否处于最终阶段
	private float maxAlphaCap;           // 当前阶段允许的最大 Alpha 上限
	// 不再使用拾取延时，改为锁定目标位置
	private float achievedAlpha;         // 已达成的最大 Alpha（阶段内单调不降）
	
	// 事件回调
	public System.Action<float> OnAlphaChanged;  // Alpha 值变化时的回调
	
	void Awake()
	{
		if (!silkImage) silkImage = GetComponent<Image>();
		ResetSilkUI();
	}
	
	void OnEnable()
	{
		// 确保初始状态
		ResetSilkUI();
	}
	
	void OnDisable()
	{
		StopAllCoroutines();
	}
	
	void Update()
	{
		// 1) 距离驱动 Alpha 上限映射
		UpdateAlphaByDistance();
		
		// 2) 缩放呼吸
		UpdateScaleBreath();
	}

    // 记录本轮拾取时的目标点位置（由外部在 TrackPoint 选定 next 目标时调用）
    public void SetLockedTargetPosition(Vector3 targetPos)
    {
        lastPickupTargetPosition = targetPos;
        hasLockedTarget = true;
        previousDistance = -1f; // 重置上一帧距离，避免第一帧误判
    }
	
	// 重置 Silk UI 状态
	private void ResetSilkUI()
	{
		currentAlpha = initialAlpha;
		targetAlpha = initialAlpha;
		pickupCount = 0;
		isFinalPhase = false;
		maxAlphaCap = 0f;
		achievedAlpha = currentAlpha;
		previousDistance = -1f;
		
		// 设置初始 Alpha
		SetImageAlpha(currentAlpha);
	}
	
	// 第一次激活 Track Point 时调用
	public void OnFirstActivation()
	{
		if (isFinalPhase) return;
		
		maxAlphaCap = Mathf.Clamp01(firstActivationAlpha);
		achievedAlpha = Mathf.Min(currentAlpha, maxAlphaCap);
	}
	
	// 拾取 Track Point 时调用
	public void OnPickup()
	{
		if (isFinalPhase) return;
		
		pickupCount++;
		maxAlphaCap = Mathf.Clamp01(maxAlphaCap + pickupIncrement);
		// 拾取时不立刻跃迁，仅提升上限，achievedAlpha 保持不变
		
		// 可选：此处可加轻微提示动画，当前先保持逻辑简洁
	}
	
	// 开始最终消失阶段
	public void OnFinalDisappear()
	{
		if (isFinalPhase) return;
		
		isFinalPhase = true;
		maxAlphaCap = Mathf.Clamp01(finalAlpha);
	}
	
	
	// 设置 Image 的 Alpha 值
	private void SetImageAlpha(float alpha)
	{
		if (silkImage)
		{
			Color color = silkImage.color;
			color.a = alpha;
			silkImage.color = color;
			
			// 触发回调
			OnAlphaChanged?.Invoke(alpha);
		}
	}

	// 距离驱动 Alpha（在已达成基础上逐步增加，受上限约束）
	private void UpdateAlphaByDistance()
	{
		if (isFinalPhase)
		{
			// 最终阶段：直接使用上限
			achievedAlpha = Mathf.MoveTowards(achievedAlpha, maxAlphaCap, Time.deltaTime * alphaLerpSpeed);
			currentAlpha = achievedAlpha;
			SetImageAlpha(currentAlpha);
			return;
		}

		float mapped = 0f;
		bool hasDistance = playerTransform != null && hasLockedTarget;
		if (hasDistance)
		{
			Vector3 pA = playerTransform.position;
			Vector3 pB = lastPickupTargetPosition;
			float dist = Vector3.Distance(pA, pB);

			// 仅当在接近（本帧距离比上一帧更小，且变化超过阈值）时才增长
			if (previousDistance >= 0f)
			{
				float delta = previousDistance - dist;
				if (delta > approachEpsilon)
				{
					float gainMeters = delta; // 近了多少米
					float gain = gainMeters * gainPerMeter;
					achievedAlpha = Mathf.Min(maxAlphaCap, achievedAlpha + gain);
				}
			}
			previousDistance = dist;
		}
		else
		{
			// 没有距离源：不改变Alpha，等待绑定后再由距离驱动
			SetImageAlpha(currentAlpha);
			return;
		}

		currentAlpha = Mathf.Lerp(currentAlpha, achievedAlpha, Time.deltaTime * alphaLerpSpeed);
		SetImageAlpha(currentAlpha);
		
		if (debugAlphaLog && (Time.frameCount % Mathf.Max(1, debugEveryNFrames) == 0))
		{
			Debug.Log($"[SilkUI] Alpha: {currentAlpha:F3} | Achieved: {achievedAlpha:F3} | Cap: {maxAlphaCap:F3}");
		}
	}

	// 缩放呼吸（1~1.1 往复）
	private void UpdateScaleBreath()
	{
		if (!enableScaleBreath) return;
		if (!transform) return;
		if (scaleCycleSeconds <= 0.0001f) scaleCycleSeconds = 1.0f;

		float phase = (Time.time % scaleCycleSeconds) / scaleCycleSeconds; // 0..1
		float s = Mathf.Lerp(scaleMin, scaleMax, 0.5f * (1f + Mathf.Sin(phase * Mathf.PI * 2f)));
		transform.localScale = new Vector3(s, s, s);
	}
	
	// 公共方法：获取当前 Alpha 值
	public float GetCurrentAlpha()
	{
		return currentAlpha;
	}
	
	// 公共方法：获取当前拾取次数
	public int GetPickupCount()
	{
		return pickupCount;
	}
	
	// 公共方法：检查是否处于最终阶段
	public bool IsInFinalPhase()
	{
		return isFinalPhase;
	}
	
	// 公共方法：手动设置 Alpha 值（用于调试）
	public void SetAlphaManually(float alpha)
	{
		if (isFinalPhase) return;
		
		currentAlpha = Mathf.Clamp01(alpha);
		SetImageAlpha(currentAlpha);
	}
	
}