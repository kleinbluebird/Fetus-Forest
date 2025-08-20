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
    
    private void Start()
    {
        // 尝试获取Fog override
        if (globalVolume != null && globalVolume.profile.TryGet(out Fog f))
        {
            fog = f;
            fog.active = true;
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
