using UnityEngine;

public class BubbleEmitterFollow : MonoBehaviour
{
    [Tooltip("Player 根对象")]
    public Transform player;

    public float yDistance = 20f;
    public float xDistance = 25f;
    private Vector3 tunnelStartPoint; // 自动记录玩家进入Trigger的位置
    
    public Animator animator;
    [Tooltip("动画名称，需和 Animator 中的名字一致")]
    public string animationStateName = "BubbleFollow";

    private bool isPlayerInTrigger = false;
    private bool hasTriggered = false;
    private bool animationCompleted = false;
    
    // 方向向量定义（单位向量）
    private Vector3 yDir = Vector3.down;
    private Vector3 xDir = Vector3.right;
    // 是否进入X阶段
    private bool hasEnteredXPhase = false;
    private float xStartOffset = 0f;

    void Start()
    {  
        if (player == null)
            Debug.LogError("请将Player Root赋值给脚本的playerRoot变量！");
    }
    
    void Update()
    {
        if (!isPlayerInTrigger || animationCompleted) return;

        Vector3 displacement = player.position - tunnelStartPoint;

        float yProgress = Vector3.Dot(displacement, yDir);
        yProgress = Mathf.Clamp(yProgress, 0f, yDistance);

        float totalDistance = yDistance + xDistance;
        float totalProgress = yProgress;

        // 如果已经完成 Y 阶段，开始计算 X 阶段
        if (yProgress >= yDistance)
        {
            if (!hasEnteredXPhase)
            {
                // 第一次进入X阶段，记录当前x方向偏移
                xStartOffset = Vector3.Dot(displacement, xDir);
                hasEnteredXPhase = true;
                // Debug.Log($"记录进入X阶段的偏移量: {xStartOffset}");
            }

            float xRaw = Vector3.Dot(displacement, xDir);
            float xProgress = Mathf.Clamp(xRaw - xStartOffset, 0f, xDistance);

            totalProgress = yDistance + xProgress;
        }

        float normalizedTime = Mathf.Clamp01(totalProgress / totalDistance);
        
        // 如果已经到达终点，就停止更新
        if (normalizedTime >= 1.0f)
        {
            normalizedTime = 1.0f;
            animationCompleted = true;
            Debug.Log("VFX 动画播放完成，锁定终帧。");
        }

        animator.Play(animationStateName, 0, normalizedTime);
        animator.speed = 0f;

        // Debug.Log($"当前动画帧: {normalizedTime:F3} | Y进度: {yProgress:F2} | X进度: {totalProgress - yProgress:F2}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // 已经触发过，不再处理
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            isPlayerInTrigger = true;
            tunnelStartPoint = player.position; // 自动记录玩家首次进入位置
            animator.SetTrigger("StartMoving");
            hasEnteredXPhase = false;
            xStartOffset = 0f;

            Debug.Log("玩家进入VFX Trigger，记录起始点：" + tunnelStartPoint);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 保持进入状态，无操作
    }
    
}
