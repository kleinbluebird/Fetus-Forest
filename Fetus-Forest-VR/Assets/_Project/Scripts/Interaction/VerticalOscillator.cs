using UnityEngine;

public class VerticalOscillator : MonoBehaviour
{
    public float floatRange = 0.1f; // 上下浮动的范围（总幅度）
    public float speed = 1f;        // 浮动速度

    private float baseY;            // 初始本地Y位置

    void Start()
    {
        // 记录初始局部Y位置
        baseY = transform.localPosition.y;
    }

    void Update()
    {
        // 平滑值在0~1之间变化
        float t = Mathf.PingPong(Time.time * speed, 1f);

        // 计算相对浮动值（0到floatRange之间）
        float offset = Mathf.Lerp(0f, floatRange, t);

        // 应用到局部位置（只修改Y）
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            baseY + offset,
            transform.localPosition.z
        );
    }
}
