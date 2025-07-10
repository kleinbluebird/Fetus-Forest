using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Control")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 3f;
    public float drag = 2f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Space limitation")]
    public float waterSurfaceY = -1f;
    public Vector3 boundaryCenter = new Vector3(0f, -5f, 0f);
    public float boundaryRadius = 80f;

    [Header("Click Moving")]
    public LayerMask clickLayerMask;
    public float autoMoveSpeed = 3f;

    private Rigidbody rb;
    private Vector2 lookInput;
    private Vector3? targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = drag;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleMouseClickMovement();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ClampPositionWithinBounds();
        CheckNearbyDroplets();
    }

    void HandleLook()
    {
        lookInput.x += Input.GetAxis("Mouse X") * mouseSensitivity;
        lookInput.y -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        lookInput.y = Mathf.Clamp(lookInput.y, -85f, 85f);

        cameraTransform.localRotation = Quaternion.Euler(lookInput.y, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, lookInput.x, 0f);
    }

    void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		//Debug.Log("Input Direction: " + inputDirection); 
        inputDirection = transform.TransformDirection(inputDirection);

        // 上浮/下潜
        if (Input.GetKey(KeyCode.E)) inputDirection.y += 1f;
        if (Input.GetKey(KeyCode.Q)) inputDirection.y -= 1f;

        inputDirection.Normalize();
        rb.AddForce(inputDirection * moveSpeed);

        // 如果点击移动目标存在，则推动玩家朝目标前进
        if (targetPosition.HasValue)
        {
            Vector3 directionToTarget = (targetPosition.Value - transform.position).normalized;
            rb.AddForce(directionToTarget * autoMoveSpeed);

            float distance = Vector3.Distance(transform.position, targetPosition.Value);
            if (distance < 1f)
            {
                targetPosition = null; // 到达目标点
            }
        }
    }

    void ClampPositionWithinBounds()
    {
        Vector3 pos = transform.position;

        // 限制不能上浮到水面上
        if (pos.y > waterSurfaceY)
        {
            pos.y = waterSurfaceY;
            transform.position = pos;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        // 限制在球形边界范围内
        Vector3 offset = pos - boundaryCenter;
        if (offset.magnitude > boundaryRadius)
        {
            Vector3 clampedPos = boundaryCenter + offset.normalized * boundaryRadius;
            transform.position = clampedPos;
            rb.linearVelocity = Vector3.zero;
        }
    }

    void HandleMouseClickMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
			//Debug.Log("Mouse Clicked");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickLayerMask))
            {
                targetPosition = hit.point;
            }
        }
    }
    
    void CheckNearbyDroplets()
    {
        if (WaterDropletGridManager.Instance == null) return;
        
        float triggerHeight = waterSurfaceY - 0.5f; // 玩家必须达到的上浮高度才开始检测交互
        float checkRadius = 5.0f;
        
        Vector3 playerPos = transform.position;
        
        List<DropletInteractionController> allDroplets = WaterDropletGridManager.Instance.GetAllDroplets();
        
        if (playerPos.y >= triggerHeight)
        {
            // 玩家已达到指定高度，检测邻近水滴
            DropletInteractionController focusedDroplet = WaterDropletGridManager.Instance.GetFocusedDroplet(playerPos, checkRadius);

            foreach (DropletInteractionController droplet in allDroplets)
            {
                bool isFocused = droplet == focusedDroplet;
                droplet.SetPlayerNear(isFocused);
            }
        }
        else
        {
            // 玩家未达到触发高度，保持所有水滴未触发状态（但不强制复位）
            foreach (DropletInteractionController droplet in allDroplets)
            {
                droplet.SetPlayerNear(false);
            }
        }
    }

}
