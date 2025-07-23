using System.Collections;
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
    
    private Coroutine delayReturnCoroutine; // 用于控制回升的延时协程
    private VerticalOscillator oscillator;
    
    private Material dropletMaterial;
    private Color initialRimColor = new Color(0.4811321f, 0.4811321f, 0.4811321f, 1f);
    private Color targetRimColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    private Color currentRimColor;
    
    private Material distortionMaterial;
    private float initialStrength = 0.05f;
    private float minStrength = 0.02f;
    
    void Start()
    {
        originalY = transform.localPosition.y;
        currentYValue = originalY;
        oscillator = GetComponent<VerticalOscillator>();

        // 从所有子物体中找到目标材质，并实例化
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true); // 包括不激活的子物体
        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials; // 复制当前材质数组
            
            bool updated = false; // 只在找到材质后才重新赋值 materials
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat == null) continue;
                
                // Babe Inner 材质
                if (dropletMaterial == null && mat.name.Contains("Babe Inner"))
                {
                    Material newMat = Instantiate(mat);
                    materials[i] = newMat;
                    dropletMaterial = newMat;
                    currentRimColor = initialRimColor;
                    dropletMaterial.SetColor("_StylingRimColor", currentRimColor);
                    updated = true;

                    // Debug.Log($"[Init] 成功绑定 Babe Inner 材质: {newMat.name}");
                }
                
                // Distortion 材质
                if (distortionMaterial == null && mat.name.Contains("DistortionWaterURP"))
                {
                    Material newMat = Instantiate(mat);
                    materials[i] = newMat;
                    distortionMaterial = newMat;
                    distortionMaterial.SetFloat("_Strength", initialStrength);
                    updated = true;

                    // Debug.Log($"[Distortion Init] 成功绑定 DistortionWaterURP 材质: {newMat.name}");
                }
            }
            if (updated)
            {
                rend.materials = materials; // 只有在材质被替换后才重新赋值
            }
                
            // 如果都找到就提前跳出
            if (dropletMaterial != null && distortionMaterial != null) break;
        }

        if (dropletMaterial == null)
        {
            Debug.LogWarning($"[Droplet] Could not find “Babe Inner” material on children of {gameObject.name}");
        }
     
        if (distortionMaterial == null)
        {
            Debug.LogWarning($"[Droplet] Could not find “DistortionWaterURP” material on children of {gameObject.name}");
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
        
        // Distortion Strength 插值
        if (distortionMaterial != null)
        {
            // 计算位置插值比例：0（在原始位置）到 1（在sinkTargetY）
            float normalizedY = Mathf.InverseLerp(originalY, sinkTargetY, currentYValue);
    
            // 插值计算 strength
            float currentStrength = Mathf.Lerp(initialStrength, minStrength, normalizedY);
            distortionMaterial.SetFloat("_Strength", currentStrength);
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
            if (near)
            {
                // 玩家靠近，立即取消延时回升
                if (delayReturnCoroutine != null)
                {
                    StopCoroutine(delayReturnCoroutine);
                    delayReturnCoroutine = null;
                }

                if (!isPlayerNear)
                {
                    isPlayerNear = true;
                    isMoving = true;
                    if (oscillator != null) oscillator.enabled = false;
                }
            }
            else
            {
                // 玩家离开，开启延时回升
                if (isPlayerNear && delayReturnCoroutine == null)
                {
                    delayReturnCoroutine = StartCoroutine(DelayReturn());
                }
            }
            
        }
    }
    
    private IEnumerator DelayReturn()
    {
        yield return new WaitForSeconds(3f); // 延迟 3 秒再恢复状态

        isPlayerNear = false;
        isMoving = true;
        delayReturnCoroutine = null;
        
        // Debug.Log($"[Droplet] {gameObject.name} 玩家离开后 xx 秒，开始回升");
    }

    void OnDestroy()
    {
        WaterDropletGridManager.Instance?.UnregisterDroplet(this);
    }
}