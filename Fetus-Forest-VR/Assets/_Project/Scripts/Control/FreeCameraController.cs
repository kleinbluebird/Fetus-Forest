using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public Transform cameraTransform;

    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public float boostMultiplier = 2f;

    private float pitch = 0f;
    private float yaw = 0f;
    
    public Rigidbody rb;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        AvoidCameraClipping(); 

        // ESC 解锁鼠标
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;

        // 水平和前后移动
        float x = Input.GetAxis("Horizontal"); // A / D
        float z = Input.GetAxis("Vertical");   // W / S

        // 垂直方向移动
        float y = 0f;
        if (Input.GetKey(KeyCode.Space)) y += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) y -= 1f;

        Vector3 direction = transform.TransformDirection(new Vector3(x, y, z)).normalized;
        Vector3 targetPosition = rb.position + direction * speed * Time.deltaTime;

        // 碰撞检测
        rb.MovePosition(targetPosition);
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.localRotation = Quaternion.Euler(0, yaw, 0);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
    
    void AvoidCameraClipping()
    {
        Vector3 camPos = cameraTransform.position;
        Vector3 camForward = cameraTransform.forward;

        float checkDistance = 0.3f; // 你可以调整这个数值，避免太贴近物体
        if (Physics.Raycast(camPos, camForward, out RaycastHit hit, checkDistance))
        {
            // 若前方有遮挡物，回推摄像机一点距离
            cameraTransform.position = hit.point - camForward * 0.1f;
        }
    }

}