using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public Camera targetCamera; // 如果为空，将使用主摄像机
    public float rotationSpeed = 5f; 

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 由于Quad默认面朝-Z方向，我们希望它“背对摄像机的方向”来进行LookAt
        Vector3 direction = (targetCamera.transform.position - transform.position).normalized;

        // 反向朝向，面朝摄像机
        Quaternion targetRotation = Quaternion.LookRotation(-direction, Vector3.up);

        // 平滑地旋转到目标方向
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
