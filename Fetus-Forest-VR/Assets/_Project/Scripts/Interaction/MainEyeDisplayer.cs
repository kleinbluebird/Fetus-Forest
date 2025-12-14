using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using AK.Wwise; // Add Wwise namespace

public class MainEyeDisplayer : MonoBehaviour
{
    public VisualEffect vfxGraph;
    public GameObject eyeObject;
    public string alphaParam = "_Alpha";
    public float showDuration = 2f;
    public float fadeDuration = 10f;
    public SequentialSpriteAnimator spriteAnimator; // 新增动画播放器引用

    [Header("Wwise Audio Settings")]
    [Tooltip("Wwise event name to trigger when switching to 2D animation")]
    public string wwiseEventOnAnimationSwitch = "";

    private Material eyeMaterial;
    private bool hasTriggered = false;

    void Start()
    {
        if (eyeObject != null)
        {
            Renderer rend = eyeObject.GetComponent<Renderer>();
            if (rend != null)
            {
                eyeMaterial = rend.material; // 独立材质
                eyeMaterial.SetFloat(alphaParam, 0f);
            }
        }

        if (spriteAnimator != null)
        {
            spriteAnimator.OnAllAnimationsFinished += HandleAnimationFinished;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FadeInAndPlayAnimation());
        }
    }

    private IEnumerator FadeInAndPlayAnimation()
    {
        float time = 0f;

        // 播放动画
        if (spriteAnimator != null)
        {
            spriteAnimator.Play();
            
            // Trigger Wwise event when switching to 2D animation
            TriggerWwiseEvent(wwiseEventOnAnimationSwitch, "AnimationSwitch");
        }
        
        // 渐显
        while (time < showDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / showDuration);
            eyeMaterial.SetFloat(alphaParam, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        eyeMaterial.SetFloat(alphaParam, 1f);

        // 同时渐隐 VFX
        float vfxTime = 0f;
        while (vfxTime < fadeDuration)
        {
            float vfxAlpha = Mathf.Lerp(1f, 0f, vfxTime / fadeDuration);
            vfxGraph.SetFloat("Alpha", vfxAlpha);
            vfxTime += Time.deltaTime;
            yield return null;
        }
        vfxGraph.SetFloat("Alpha", 0f);
    }

    private void HandleAnimationFinished()
    {
        Debug.Log("MainEyeDisplayer: 所有帧动画播放完成，可以触发后续逻辑");
    }

    /// <summary>
    /// Trigger the specified Wwise audio event using this GameObject as the audio source
    /// </summary>
    /// <param name="eventName">The Wwise event name to trigger</param>
    /// <param name="stageName">The interaction stage name for logging</param>
    private void TriggerWwiseEvent(string eventName, string stageName)
    {
        if (!string.IsNullOrEmpty(eventName))
        {
            // Post Wwise event with this GameObject as the audio source
            AkSoundEngine.PostEvent(eventName, gameObject);
            Debug.Log($"[MainEyeDisplayer] Triggered {stageName} Wwise event: {eventName} on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[MainEyeDisplayer] {stageName} Wwise event name is empty on {gameObject.name}");
        }
    }
}
