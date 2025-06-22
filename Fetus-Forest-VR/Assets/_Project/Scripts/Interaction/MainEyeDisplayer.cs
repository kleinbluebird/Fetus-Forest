using UnityEngine;
using UnityEngine.VFX;

public class MainEyeDisplayer : MonoBehaviour
{
    public VisualEffect vfxGraph;
    public GameObject eyeObject;
    public string alphaParam = "_Alpha"; 
    public float showDuration = 2f;
    public float fadeDuration = 10f;
    
    private Material eyeMaterial;
    private bool hasTriggered = false;
    
    void Start()
    {
        if (eyeObject != null)
        {
            Renderer rend = eyeObject.GetComponent<Renderer>();
            if (rend != null)
            {
                // 使用实例材质，避免影响共享材质
                eyeMaterial = rend.material;
                eyeMaterial.SetFloat(alphaParam, 0f);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FadeIn());
        }
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        float time = 0f;

        while (time < showDuration || time < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / showDuration);
            eyeMaterial.SetFloat(alphaParam, alpha);
            
            float vfxAlpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            vfxGraph.SetFloat("Alpha", vfxAlpha);
            
            time += Time.deltaTime;
            yield return null;
        }

        // 确保最终值是 1
        eyeMaterial.SetFloat(alphaParam, 1f);
        vfxGraph.SetFloat("Alpha", 0f);
    }
}
