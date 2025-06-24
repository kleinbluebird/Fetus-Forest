using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using SCPE;

public class FogHorizonController : MonoBehaviour
{
    public float transitionHeight = -20f; // Y低于此高度开始渐变
    public float transitionSpeed = 1f;  // 渐变速度

    private Volume volume;
    private Fog fog;

    private Vector2 targetMinMax;
    private Vector2 currentMinMax;
    
    void Start()
    {
        // 获取 Volume 组件
        volume = FindObjectOfType<Volume>();
        if (volume && volume.profile.TryGet(out fog))
        {
            currentMinMax = fog.horizonMinMax.value;
        }
        else
        {
            Debug.LogError("Fog effect not found in Volume");
        }
    }
    
    void Update()
    {
        if (fog == null) return;

        float currentY = transform.position.y;

        // 判断当前高度属于哪种状态，设置目标值
        if (currentY < transitionHeight)
        {
            targetMinMax = new Vector2(1f, 1f);  // 拉近雾
        }
        else
        {
            targetMinMax = new Vector2(0f, 1f);  // 恢复原状
        }

        // 插值平滑过渡
        currentMinMax = Vector2.Lerp(currentMinMax, targetMinMax, Time.deltaTime * transitionSpeed);
        fog.horizonMinMax.value = currentMinMax;
    }
}
