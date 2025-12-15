using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class ConnectionLineController : MonoBehaviour
{
	[Header("VFX 引用")]
	[SerializeField] private VisualEffect vfx;

	[Header("激活策略")]
	[SerializeField] private bool playOnEnable = false;

	[Header("Property 名（需与VFX Graph一致）")]
	[SerializeField] private string playerPosProperty = "PlayerPosition";
	[SerializeField] private string targetPosProperty = "TargetPosition";
	[SerializeField] private string noiseRangeXProperty = "NoiseRangeX";
	[SerializeField] private string lineAlphaProperty = "LineAlpha";

	[Header("Noise X 距离映射")]
	[SerializeField] private bool enableDynamicNoise = true;
	[SerializeField] private float minNoiseRange = -1f; // 近 -> 更弯
	[SerializeField] private float maxNoiseRange = 2f;  // 远 -> 更直
	[SerializeField] private float minDistance = 0.5f;
	[SerializeField] private float maxDistance = 30f;

	[Header("Alpha 衰减（离开越久越淡）")]
	[SerializeField] private bool enableAlphaDecay = true;
	[SerializeField] private float nearThreshold = 1.5f;   // 认为"靠近"的距离
	[SerializeField] private float decaySeconds = 10.0f;   // 离开后多久从 defaultAlpha 衰减到 minAlpha
	[SerializeField] private float minAlpha = 0.15f;       // 不会完全消失
	[SerializeField] private float defaultAlpha = 0.4f;    // 默认Alpha值
	[SerializeField] private float maxAlpha = 1.0f;        // 脉冲时的最大Alpha
	[SerializeField] private float alphaLerpSpeed = 8f;    // 插值平滑

	[Header("脉冲设置（Alpha突增后回归）")]
	[SerializeField] private bool enablePulse = true;
	[SerializeField] private float pulseDuration = 0.6f;   // 脉冲持续时间

	private bool isActive;
	private float awayTimer;     // 离开累计时间
	private float currentAlpha;  // 当前写入的Alpha
	private float pulseTimer;    // 脉冲计时器
	private bool isPulsing;      // 是否正在脉冲
	private bool shouldResetDecay; // 是否需要重置衰减计时器
	private bool isForceHidden;  // 是否被强制隐藏
	private Coroutine fadeCoroutine; // 渐隐/渐显协程

	void Awake()
	{
		if (!vfx) vfx = GetComponent<VisualEffect>();
		if (vfx) vfx.enabled = false;
		isActive = false;
		currentAlpha = defaultAlpha;
	}

	void OnEnable()
	{
		if (playOnEnable) Activate();
		else if (vfx) vfx.enabled = false;
	}

	void OnDisable()
	{
		Deactivate();
	}

	public void Activate()
	{
		if (!vfx) return;
		vfx.enabled = true;
		vfx.Reinit();
		vfx.Play();
		isActive = true;
		awayTimer = 0f;
		currentAlpha = defaultAlpha;
		isForceHidden = false;
		WriteAlpha(currentAlpha);
	}

	public void Deactivate()
	{
		if (!vfx) return;
		vfx.Stop();
		vfx.enabled = false;
		isActive = false;
	}

	// 渐隐/渐显
	public void SetVisible(bool visible)
	{
		if (!vfx) return;
		
		// 停止之前的渐隐/渐显协程
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
		
		if (visible)
		{
			isForceHidden = false;
			vfx.enabled = true;
			vfx.Play();
			fadeCoroutine = StartCoroutine(FadeAlphaTo(defaultAlpha, 0.25f));
		}
		else
		{
			isForceHidden = true;
			fadeCoroutine = StartCoroutine(FadeAlphaTo(0f, 0.25f));
		}
	}

	// 触发脉冲：Alpha从0.4突增到1，然后回落到0.4，并重置衰减计时器
	public void Pulse()
	{
		if (!enablePulse || isForceHidden) return;
		
		isPulsing = true;
		pulseTimer = 0f;
		shouldResetDecay = true;
		Debug.Log("[ConnectionLine] 触发脉冲效果，将重置衰减计时器");
	}

	void LateUpdate()
	{
		if (!isActive || vfx == null || !vfx.enabled || isForceHidden) return;
		
		if (!vfx.HasVector3(playerPosProperty) || !vfx.HasVector3(targetPosProperty)) return;

		Vector3 pA = vfx.GetVector3(playerPosProperty);
		Vector3 pB = vfx.GetVector3(targetPosProperty);
		float dist = Vector3.Distance(pA, pB);

		// 1) 动态Noise X
		if (enableDynamicNoise && vfx.HasFloat(noiseRangeXProperty))
		{
			float t = Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
			float noiseX = Mathf.Lerp(maxNoiseRange, minNoiseRange, t);
			vfx.SetFloat(noiseRangeXProperty, noiseX);
		}

		// 2) Alpha 衰减（离开越久越淡）
		float baseAlpha = defaultAlpha;
		if (enableAlphaDecay)
		{
			// 检查是否需要重置衰减计时器
			if (shouldResetDecay)
			{
				awayTimer = 0f;
				shouldResetDecay = false;
				Debug.Log("[ConnectionLine] 衰减计时器已重置");
			}

			if (dist <= nearThreshold) 
			{
				awayTimer = 0f;
			}
			else 
			{
				awayTimer = Mathf.Min(decaySeconds, awayTimer + Time.deltaTime);
			}

			// 从默认Alpha衰减到最小Alpha
			baseAlpha = Mathf.Lerp(defaultAlpha, minAlpha, decaySeconds > 0f ? (awayTimer / decaySeconds) : 1f);
		}

		// 3) 脉冲效果（Alpha从0.4突增到1，然后回落到0.4）
		float finalAlpha = baseAlpha;
		if (isPulsing)
		{
			pulseTimer += Time.deltaTime;
			float pulseProgress = pulseTimer / pulseDuration;
			
			if (pulseProgress >= 1f)
			{
				// 脉冲结束，强制设置为默认Alpha，然后重新开始衰减逻辑
				isPulsing = false;
				finalAlpha = defaultAlpha;
				awayTimer = 0f;
				Debug.Log("[ConnectionLine] 脉冲结束，Alpha回落到默认值" + defaultAlpha + "，衰减计时器重置");
			}
			else
			{
				// 脉冲进行中：从默认Alpha突增到1，然后平滑回落到默认Alpha
				if (pulseProgress < 0.3f)
				{
					// 前30%时间：快速突增到1
					float riseProgress = pulseProgress / 0.3f;
					finalAlpha = Mathf.Lerp(defaultAlpha, maxAlpha, riseProgress);
				}
				else
				{
					// 后70%时间：从1平滑回落到默认Alpha
					float falloffProgress = (pulseProgress - 0.3f) / 0.7f;
					finalAlpha = Mathf.Lerp(maxAlpha, defaultAlpha, falloffProgress);
				}
			}
		}

		// 应用最终Alpha（只有在非强制隐藏状态下）
		if (!isForceHidden)
		{
			currentAlpha = Mathf.Lerp(currentAlpha, finalAlpha, Time.deltaTime * alphaLerpSpeed);
			WriteAlpha(currentAlpha);
		}
	}

	private void WriteAlpha(float a)
	{
		if (vfx != null && vfx.HasFloat(lineAlphaProperty))
		{
			vfx.SetFloat(lineAlphaProperty, Mathf.Clamp01(a));
		}
	}

	System.Collections.IEnumerator FadeAlphaTo(float target, float seconds)
	{
		float start = currentAlpha;
		float t = 0f;
		
		while (t < seconds)
		{
			t += Time.deltaTime;
			currentAlpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / Mathf.Max(0.0001f, seconds)));
			WriteAlpha(currentAlpha);
			yield return null;
		}
		
		currentAlpha = target;
		WriteAlpha(currentAlpha);
		fadeCoroutine = null;
		
		// 如果目标是0，可以选择完全停止VFX
		if (Mathf.Approximately(target, 0f))
		{
			// 可选：完全停止VFX
			// vfx.Stop();
			// vfx.enabled = false;
		}
	}
}