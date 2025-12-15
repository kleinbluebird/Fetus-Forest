using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class MainEyeDisplayer : MonoBehaviour
{
    public VisualEffect vfxGraph;
    public GameObject eyeObject;
    public string alphaParam = "_Alpha";
    public float showDuration = 2f;
    public float fadeDuration = 10f;
    public SequentialSpriteAnimator spriteAnimator; // 新增动画播放器引用

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
}
