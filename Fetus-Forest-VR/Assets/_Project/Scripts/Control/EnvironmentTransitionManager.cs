using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using SCPE;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnvironmentTransitionManager : MonoBehaviour
{
    [Header("Fog Settings")]
    public Volume globalVolume; // 挂在 Global Volume
    private Fog fog;
    public float fogStartFrom = 30f;
    public float fogStartTo = 70f;
    public float fogTransitionDuration = 3f;

    [Header("Background Object")]
    public Transform cylinderBG;
    public Vector3 cylinderStart = new Vector3(53f, -55.08f, 6.41f);
    public Vector3 cylinderEnd = new Vector3(130f, -55.08f, 6.41f);
    public float cylinderTransitionDuration = 5f;

    [Header("Camera Settings")]
    public CinemachineCamera mainVCam; 
    public CinemachineCamera trackVCam;
    // public CinemachineSplineDolly dolly; // TrackVCam 的 Dolly 组件
    // public float dollyDuration = 5f;       // normalized 从 0→1 持续时长
    // public float dollyDelay = 2f;          // clip4 播放后延迟触发
    public float fovStart = 80f;
    public float fovEnd = 25f;
    public float fovDuration = 5f;
    public float fovDelay = 2f;

    [Header("Silk UI Linkage")]
    public Image silkUIImage;                 // Silk UI 的 Image（用于Alpha）
    public RectTransform silkUITransform;     // Silk UI 的 RectTransform（用于缩放/旋转）
    public float silkTargetScale = 1.5f;      // FOV缩放期间目标缩放
    public float silkRotationZDelta = -10f;    // FOV缩放期间Z轴增加角度

    [Header("Player & Invisible Plane")]
    public Transform playerTransform;          // 玩家实时位置
    public Transform invisiblePlane;           // 仅使用其Y坐标
    public float planeInfluenceRange = 1f;     // 与平面相距多少米内开始强影响
    public float approachLerpSpeed = 2f;       // 接近时雾值逼近速度

    [Header("Post & Fog Parameters")]
    public float initialPostExposure = 0.64f;
    public float pickupPostExposure = 0.48f;
    public float initialFogDistance = -50f;        // 初始“Distance Density”对应的近距离（用Fog.fogStartDistance表示）
    public float minApproachFogDistance = -200f;   // 玩家接近Invisible Plane时单向靠近的极限
    public float pickupFogDistance = 30f;          // 拾取后快速散开的目标
    public float pickupTransitionDuration = 0.6f;  // 拾取后过渡时长

    [Header("Animated Eye Settings")]
    // public Transform animatedEye;          // Animated_Eye GameObject
    // private Vector3 eyeOffset;             // 初始相对偏移
    
    public Renderer animatedEyeRenderer;     // Animated_Eye 的 Renderer
    private Material animatedEyeMat;
    private string alphaProperty = "_Alpha";  // Shader 中的透明度属性名
    public float eyeAlphaStart = 1f;
    public float eyeAlphaEnd = 0.5f;
    
    [Header("Thorns Animation")]
    public Animator thornsAnimator;   // Animator 组件
    
    [Header("Scene Transition")]
    public Image fadeImage;    
    public string nextSceneName;
    public float whiteFadeDuration = 1.5f;
    
    private ColorAdjustments colorAdjustments;
    private float bestVerticalDistance = float.MaxValue; // 历史最小 |player.y - plane.y|
    private bool pickupTransitionTriggered;

    private void Start()
    {
        // 尝试获取Fog override
        if (globalVolume != null && globalVolume.profile.TryGet(out Fog f))
        {
            fog = f;
            fog.active = true;
        }
        // 尝试获取 Color Adjustments
        if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments ca))
        {
            colorAdjustments = ca;
        }
        
        // if (animatedEye != null && trackVCam != null)
        // {
        //     // 记录初始相对偏移
        //     eyeOffset = animatedEye.position - trackVCam.transform.position;
        // }
        
        // 获取 Animated_Eye 的材质实例
        if (animatedEyeRenderer != null)
        {
            animatedEyeMat = animatedEyeRenderer.material;
        }

        // 初始化后期与雾参数
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(initialPostExposure);
        }
        if (fog != null)
        {
            fog.fogStartDistance.Override(initialFogDistance);
        }
    }

    public void TriggerEnvironmentTransition()
    {
        Debug.Log("[EnvironmentTransition] 启动环境过渡效果");
        StartCoroutine(DollyAndRotateRoutine());
    }

    private IEnumerator DollyAndRotateRoutine()
    {
        // 延迟触发
        // yield return new WaitForSeconds(dollyDelay);
        yield return new WaitForSeconds(fovDelay);
        
        
        StartCoroutine(CylinderTransition());
        StartCoroutine(FogTransition());

        // 启动 Animator 动画
        if (thornsAnimator != null)
        {
            thornsAnimator.SetBool("ThornsRotate", true);
        }
        
        float elapsed = 0f;
        // 记录Silk UI初始状态
        float silkStartAlpha = 0f;
        float silkStartScale = 1f;
        float silkStartRotZ = 0f;
        if (silkUIImage != null)
        {
            Color c = silkUIImage.color;
            silkStartAlpha = c.a;
        }
        if (silkUITransform != null)
        {
            silkStartScale = silkUITransform.localScale.x;
            silkStartRotZ = silkUITransform.localEulerAngles.z;
        }
        // float startNormalized = dolly.CameraPosition;
        // float targetNormalized = 0.43f;

        // while (elapsed < dollyDuration)
        // {
        //     elapsed += Time.deltaTime;
        //     float t = Mathf.Clamp01(elapsed / dollyDuration);
        //
        //     // Dolly 移动
        //     if (dolly != null)
        //         dolly.CameraPosition = Mathf.Lerp(startNormalized, targetNormalized, t);
        //     
        //     // Eye 跟随
        //     if (animatedEye != null && trackVCam != null)
        //         animatedEye.position = trackVCam.transform.position + eyeOffset;
        //
        //     yield return null;
        // }
        //
        // // 确保 Dolly 完成
        // if (dolly != null)
        //     dolly.CameraPosition = targetNormalized;
        
        while (elapsed < fovDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fovDuration);

            // 相机 FOV 渐变
            if (trackVCam != null)
            {
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                trackVCam.Lens.FieldOfView = Mathf.Lerp(fovStart, fovEnd, smoothT);
            }

            // Animated Eye Alpha 渐变
            if (animatedEyeMat != null && animatedEyeMat.HasProperty(alphaProperty))
            {
                float alpha = Mathf.Lerp(eyeAlphaStart, eyeAlphaEnd, t);
                animatedEyeMat.SetFloat(alphaProperty, alpha);
            }

            // Silk UI 渐变（Alpha → 1，Scale → silkTargetScale，RotationZ 增加 silkRotationZDelta）
            {
                float smoothT2 = Mathf.SmoothStep(0f, 1f, t);
                if (silkUIImage != null)
                {
                    Color c = silkUIImage.color;
                    c.a = Mathf.Lerp(silkStartAlpha, 1f, smoothT2);
                    silkUIImage.color = c;
                }
                if (silkUITransform != null)
                {
                    float s = Mathf.Lerp(silkStartScale, silkTargetScale, smoothT2);
                    silkUITransform.localScale = new Vector3(s, s, s);
                    Vector3 euler = silkUITransform.localEulerAngles;
                    euler.z = Mathf.LerpAngle(silkStartRotZ, silkStartRotZ + silkRotationZDelta, smoothT2);
                    silkUITransform.localEulerAngles = euler;
                }
            }

            yield return null;
        }

        // 确保最终值
        if (trackVCam != null)
            trackVCam.Lens.FieldOfView = fovEnd;

        if (animatedEyeMat != null && animatedEyeMat.HasProperty(alphaProperty))
            animatedEyeMat.SetFloat(alphaProperty, eyeAlphaEnd);
        

        // 触发白场过渡到新场景
        StartCoroutine(FadeToWhiteAndLoadScene());
    }

    private void Update()
    {
        // 初始阶段：根据玩家与 Invisible Plane 的纵向接近，单向地让雾的 StartDistance 靠近 minApproachFogDistance（不可逆）
        if (fog != null && playerTransform != null && invisiblePlane != null && !pickupTransitionTriggered)
        {
            float dy = Mathf.Abs(playerTransform.position.y - invisiblePlane.position.y);
            if (dy < bestVerticalDistance) bestVerticalDistance = dy; // 记录历史最小值
            float t = Mathf.Clamp01(1f - (bestVerticalDistance / Mathf.Max(0.0001f, planeInfluenceRange)));
            float targetFog = Mathf.Lerp(initialFogDistance, minApproachFogDistance, t);
            float current = fog.fogStartDistance.value;
            float next = Mathf.Lerp(current, targetFog, Time.deltaTime * approachLerpSpeed);
            fog.fogStartDistance.Override(next);
        }
    }

    // 外部触发：玩家拾取第一个 Target 后调用
    public void OnPlayerPickedTarget()
    {
        if (!pickupTransitionTriggered)
        {
            StartCoroutine(QuickPostAndFogAfterPickup());
        }
    }

    private IEnumerator QuickPostAndFogAfterPickup()
    {
        pickupTransitionTriggered = true;
        float elapsed = 0f;
        float startPost = colorAdjustments != null ? colorAdjustments.postExposure.value : 0f;
        float startFog = fog != null ? fog.fogStartDistance.value : 0f;
        while (elapsed < pickupTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pickupTransitionDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.Override(Mathf.Lerp(startPost, pickupPostExposure, smoothT));
            }
            if (fog != null)
            {
                fog.fogStartDistance.Override(Mathf.Lerp(startFog, pickupFogDistance, smoothT));
            }
            yield return null;
        }
        if (colorAdjustments != null) colorAdjustments.postExposure.Override(pickupPostExposure);
        if (fog != null) fog.fogStartDistance.Override(pickupFogDistance);

        // 拾取后雾气消散完成，同步禁用 Invisible Plane，允许玩家穿过
        if (invisiblePlane != null)
        {
            var col = invisiblePlane.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            invisiblePlane.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator CylinderTransition()
    {
        if (cylinderBG == null) yield break;

        Vector3 startPos = cylinderStart;
        Vector3 endPos = cylinderEnd;

        float elapsed = 0f;
        while (elapsed < cylinderTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cylinderTransitionDuration);
            cylinderBG.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
    
    private IEnumerator FogTransition()
    {
        if (fog == null) yield break;

        float elapsed = 0f;
        while (elapsed < fogTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fogTransitionDuration);
            fog.fogStartDistance.Override(Mathf.Lerp(fogStartFrom, fogStartTo, t));
            yield return null;
        }
    }
    
    private IEnumerator FadeToWhiteAndLoadScene()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("[EnvironmentTransition] 缺少 fadeImage 引用，无法执行白场过渡！");
            yield break;
        }

        Debug.Log("[EnvironmentTransition] 白场过渡开始");
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float elapsed = 0f;
        while (elapsed < whiteFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / whiteFadeDuration);
            color.a = t;
            fadeImage.color = color;
            yield return null;
        }

        // 确保最终全白
        color.a = 1f;
        fadeImage.color = color;

        // 延迟一帧，保证画面完全白后再切换场景
        yield return null;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
