using UnityEngine;

public class DropletInteractionController : MonoBehaviour
{
    private float sinkTargetY = 0f;
    public float sinkSpeed = 0.5f;
    public float returnSpeed = 0.5f;

    private float originalY;
    private float currentYValue; // 独立控制的 Y 值
    private bool isPlayerNear = false;
    private bool isMoving = false; // 是否需要移动
    
    private VerticalOscillator oscillator;
    
    private Material dropletMaterial;
    private Color initialRimColor = new Color(0.4811321f, 0.4811321f, 0.4811321f, 1f);
    private Color targetRimColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    private Color currentRimColor;
    
    void Start()
    {
        originalY = transform.localPosition.y;
        currentYValue = originalY;
        oscillator = GetComponent<VerticalOscillator>();

        // 从所有子物体中找到名字为 "Babe Inner" 的材质，并实例化
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true); // 包括不激活的子物体
        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials; // 复制当前材质数组
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.name.Contains("Babe Inner"))
                {
                    // 实例化材质
                    Material newMat = Instantiate(mat);
                    materials[i] = newMat; // 替换副本数组中对应项
                    rend.materials = materials; // 设置回 renderer 的材质数组

                    dropletMaterial = newMat;
                    currentRimColor = initialRimColor;

                    // 设置颜色参数
                    dropletMaterial.SetColor("_StylingRimColor", currentRimColor);

                    // 打印调试
                    // Debug.Log($"[Droplet] 替换材质成功: {newMat.name}");
                    // Debug.Log($"[Rim Debug] EnableRimStyling: {dropletMaterial.GetFloat("_EnableRimStyling")}");
                    // Debug.Log($"[Rim Debug] StylingRimColor: {dropletMaterial.GetColor("_StylingRimColor")}");
                    break;
                }
            }

            if (dropletMaterial != null) break; // 已经找到并设置，不再继续
        }

        if (dropletMaterial == null)
        {
            Debug.LogWarning($"[Droplet] Could not find 'Babe Inner' material on children of {gameObject.name}");
        }

        
        // 注册到管理器
        if (WaterDropletGridManager.Instance != null)
        {
            WaterDropletGridManager.Instance.RegisterDroplet(this);
        }
        else
        {
            Debug.LogWarning("No WaterDropletGridManager instance found in scene.");
        }
    }

    void Update()
    {
        if (!isMoving) return;
        
        float targetY = isPlayerNear ? sinkTargetY : originalY;
        float speed = isPlayerNear ? sinkSpeed : returnSpeed;
        
        // 插值移动
        currentYValue = Mathf.MoveTowards(currentYValue, targetY, Time.deltaTime * speed);
        transform.localPosition = new Vector3(transform.localPosition.x, currentYValue, transform.localPosition.z);

        // Debug.Log($"[Droplet] isPlayerNear: {isPlayerNear}, currentY: {currentY:F3}, targetY: {targetY:F3}, speed: {speed}");

        // 颜色插值
        Color targetColor = isPlayerNear ? targetRimColor : initialRimColor;
        currentRimColor = Color.Lerp(currentRimColor, targetColor, Time.deltaTime * speed);
        if (dropletMaterial != null)
        {
            dropletMaterial.SetColor("_StylingRimColor", currentRimColor);
            
            Color currentInMaterial = dropletMaterial.GetColor("_StylingRimColor");
            // Debug.Log($"[Material Check] Shader color is now: {currentInMaterial}");
        }
        else
        {
            Debug.LogWarning("Droplet material is null!");
        }

        
        // 如果位置足够接近目标，停止移动
        if (Mathf.Abs(currentYValue - targetY) < 0.001f)
        {
            isMoving = false;

            if (!isPlayerNear && oscillator != null)
            {
                oscillator.enabled = true;
            }
        }
    }

    public void SetPlayerNear(bool near)
    {
        if (isPlayerNear != near)
        {
            isPlayerNear = near;
            // Debug.Log($"[SetPlayerNear] Changing to {(near ? "NEAR" : "FAR")}, current local Y before oscillator = {transform.localPosition.y:F3}");
            isMoving = true;

            if (near && oscillator != null)
            {
                oscillator.enabled = false;
                // Debug.Log($"[Oscillator] Now enabled = {!near}, after enabling, Y = {transform.localPosition.y:F3}");
            }
        }
    }

    void OnDestroy()
    {
        WaterDropletGridManager.Instance?.UnregisterDroplet(this);
    }
}