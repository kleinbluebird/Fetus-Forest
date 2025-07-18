using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement Control")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 3f;
    public float drag = 2f;

    [Header("Mouse Control")]
    public float lookSmoothSpeed = 10f;
    public float mouseSensitivity = 1.5f;
    public Transform cameraTransform;
    private float pitch; // X轴旋转（上下）
    private float yaw;   // Y轴旋转（左右）
    private Vector2 smoothedLook;

    [Header("Space limitation")]
    public float waterSurfaceY = -1f;
    public Vector3 boundaryCenter = new Vector3(0f, -5f, 0f);
    public float boundaryRadius = 80f;

    [Header("Click Moving")]
    public LayerMask clickLayerMask;
    public float autoMoveSpeed = 3f;
    
    [Header("Rotation Restriction When Near Wall")]
    public float wallDetectDistance = 0.5f;
    public float restrictedYawAngle = 15f; // 摄像头最大左右角度范围
    public LayerMask wallLayerMask;

    private float restrictedYawCenter;     // 当前摄像头被限制旋转时的中心值
    private bool isYawRestricted = false;
    

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool floatUpPressed;
    private bool floatDownPressed;
    private bool clickMoveTriggered;

    private Vector3? targetPosition;
    
    private PlayerInputActions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.FloatUp.performed += ctx => floatUpPressed = true;
        inputActions.Player.FloatUp.canceled += ctx => floatUpPressed = false;

        inputActions.Player.FloatDown.performed += ctx => floatDownPressed = true;
        inputActions.Player.FloatDown.canceled += ctx => floatDownPressed = false;

        inputActions.Player.ClickMove.performed += ctx => clickMoveTriggered = true;
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();
    

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        rb.useGravity = false;
        rb.linearDamping = drag;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        HandleLook();
        CheckCameraRotationRestriction();
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
        // 平滑鼠标输入
        smoothedLook = Vector2.Lerp(smoothedLook, lookInput, lookSmoothSpeed * Time.deltaTime);
        Vector2 mouseDelta = smoothedLook * mouseSensitivity;

        // 更新旋转角度
        yaw += mouseDelta.x;
        pitch -= mouseDelta.y;

        // 限制 pitch（上下视角）
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        // 应用旋转（只操作摄像机，不动角色物体本体）
        cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    
    void HandleMovement()
    {
        // 判断是否有主动输入（键盘或鼠标）
        bool hasMovementInput = moveInput != Vector2.zero || floatUpPressed || floatDownPressed;
        bool hasMouseClick = Mouse.current.leftButton.wasPressedThisFrame;

        // 如果有输入，取消自动移动目标
        if (hasMovementInput || hasMouseClick)
        {
            targetPosition = null;
        }

        // 基于摄像机方向的移动
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDirection = (camForward * moveInput.y + camRight * moveInput.x);

        if (floatUpPressed) inputDirection.y += 1f;
        if (floatDownPressed) inputDirection.y -= 1f;

        inputDirection.Normalize();
        rb.AddForce(inputDirection * moveSpeed);

        // 自动点击移动（如果未被打断）
        if (targetPosition.HasValue)
        {
            Vector3 directionToTarget = (targetPosition.Value - transform.position).normalized;
            rb.AddForce(directionToTarget * autoMoveSpeed);

            float distance = Vector3.Distance(transform.position, targetPosition.Value);
            if (distance < 1f)
            {
                targetPosition = null;
            }
        }
    }
    
    void HandleMouseClickMovement()
    {
        if (clickMoveTriggered)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickLayerMask))
            {
                targetPosition = hit.point;
            }

            clickMoveTriggered = false; // 重置触发状态
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
    
    void CheckCameraRotationRestriction()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 forward = cameraTransform.forward;

        if (Physics.Raycast(origin, forward, out RaycastHit hit, wallDetectDistance, wallLayerMask))
        {
            if (!isYawRestricted)
            {
                // 第一次接近墙体：记录此刻作为 yaw 中心
                restrictedYawCenter = transform.eulerAngles.y;
                isYawRestricted = true;
            }

            // 限制 yaw 范围（左右不能转太多）
            float currentYaw = transform.eulerAngles.y;
            float minYaw = restrictedYawCenter - restrictedYawAngle;
            float maxYaw = restrictedYawCenter + restrictedYawAngle;
            float clampedYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);
            transform.rotation = Quaternion.Euler(0f, clampedYaw, 0f);
        }
        else
        {
            // 离开墙体，恢复自由旋转
            isYawRestricted = false;
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
