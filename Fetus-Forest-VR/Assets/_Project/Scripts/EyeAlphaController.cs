using UnityEngine;

public class EyeAlphaController : MonoBehaviour
{
	[Header("Camera Man Transform")]
    public Transform cameraMan;

    [Header("Eye Material Settings")]
    public Renderer eyeRenderer;
    public string alphaProperty = "_Alpha";

    [Header("Y Position to Alpha Mapping")]
    public float fadeStartY = -20f; // 开始渐变的 Y 值
    public float fadeEndY = -30f;   // 渐变结束的 Y 值（达到最大 alpha）

    private Material eyeMaterial;

    void Start()
    {
        // 使用实例化的材质，防止影响原始材质
        eyeMaterial = eyeRenderer.material;
    }

    void Update()
    {
        if (cameraMan == null || eyeMaterial == null) return;

        float yPos = cameraMan.position.y;
        float alpha;

        if (yPos >= fadeStartY)
        {
            alpha = 0f;
        }
        else if (yPos <= fadeEndY)
        {
            alpha = 1f;
        }
        else
        {
            // 进行映射（越低越透明）
            float t = Mathf.InverseLerp(fadeEndY, fadeStartY, yPos); // 这里修正了顺序
            alpha = Mathf.SmoothStep(0f, 1f, 1f - t);
        }

        eyeMaterial.SetFloat(alphaProperty, alpha);
    }
}
