using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerConstraintController : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;
    public PlayerController playerController;
    public CinemachineCamera playerVCam;

    [Header("Constraint Target")]
    public Vector3 targetPosition = new Vector3(31.5f, -55f, 12f);
    public float moveDuration = 3f;  // 移动时长

    private bool isConstraining = false;

    public void StartConstraint()
    {
        if (!isConstraining)
            StartCoroutine(ConstraintRoutine());
    }

    private IEnumerator ConstraintRoutine()
    {
        isConstraining = true;

        if (playerController != null)
            playerController.enabled = false;
        
        if (playerVCam != null)
            playerVCam.enabled = false;

        Vector3 startPos = playerRoot.position;

        float elapsed = 0f;

        // 在 moveDuration 时间内逐渐过渡
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float tMove = Mathf.Clamp01(elapsed / moveDuration);
           
            if (playerRoot != null)
                playerRoot.position = Vector3.Lerp(startPos, targetPosition, tMove);

            yield return null;
        }

        // 确保最终位置/旋转一致
        if (playerRoot != null)
            playerRoot.position = targetPosition;

        Debug.Log("[Constraint] 玩家位置与视角已被固定");

        isConstraining = false;
    }
    
}
