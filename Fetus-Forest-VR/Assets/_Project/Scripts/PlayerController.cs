using UnityEngine;
<<<<<<< Updated upstream:Fetus-Forest-VR/Assets/_Project/Scripts/PlayerController.cs
=======
using System.Collections.Generic;
>>>>>>> Stashed changes:Fetus-Forest-VR/Assets/_Project/Scripts/Control/PlayerController.cs
#if UNITY_EDITOR
using AK.Wwise.Editor;
#endif

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

    [Header("Audio Events")]
    [Tooltip("Wwise event name for starting swimming up movement")]
    public string swimUpStartEvent = "Player_SwimUp_Start";
    [Tooltip("Wwise event name for stopping swimming up movement")]
    public string swimUpStopEvent = "Player_SwimUp_Stop";
    [Tooltip("Wwise event name for starting swimming down movement")]
    public string swimDownStartEvent = "Player_SwimDown_Start";
    [Tooltip("Wwise event name for stopping swimming down movement")]
    public string swimDownStopEvent = "Player_SwimDown_Stop";

    private Rigidbody rb;
    private Vector2 lookInput;
    private Vector3? targetPosition;

    // Audio state tracking
    private bool isSwimmingUp = false;
    private bool isSwimmingDown = false;

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
        HandleVerticalMovementAudio();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ClampPositionWithinBounds();
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

        // Handle vertical movement (up/down swimming)
        if (Input.GetKey(KeyCode.E)) inputDirection.y += 1f;
        if (Input.GetKey(KeyCode.Q)) inputDirection.y -= 1f;

        inputDirection.Normalize();
        rb.AddForce(inputDirection * moveSpeed);

        // If click movement target exists, push player towards target
        if (targetPosition.HasValue)
        {
            Vector3 directionToTarget = (targetPosition.Value - transform.position).normalized;
            rb.AddForce(directionToTarget * autoMoveSpeed);

            float distance = Vector3.Distance(transform.position, targetPosition.Value);
            if (distance < 1f)
            {
                targetPosition = null; // Reached target point
            }
        }
    }

    void HandleVerticalMovementAudio()
    {
        bool currentlySwimmingUp = Input.GetKey(KeyCode.E);
        bool currentlySwimmingDown = Input.GetKey(KeyCode.Q);

        // Handle swimming up audio
        if (currentlySwimmingUp && !isSwimmingUp)
        {
            // Started swimming up - play start event
            PlayWwiseEvent(swimUpStartEvent);
            isSwimmingUp = true;
        }
        else if (!currentlySwimmingUp && isSwimmingUp)
        {
            // Stopped swimming up - play stop event
            PlayWwiseEvent(swimUpStopEvent);
            isSwimmingUp = false;
        }

        // Handle swimming down audio
        if (currentlySwimmingDown && !isSwimmingDown)
        {
            // Started swimming down - play start event
            PlayWwiseEvent(swimDownStartEvent);
            isSwimmingDown = true;
        }
        else if (!currentlySwimmingDown && isSwimmingDown)
        {
            // Stopped swimming down - play stop event
            PlayWwiseEvent(swimDownStopEvent);
            isSwimmingDown = false;
        }
    }

    void ClampPositionWithinBounds()
    {
        Vector3 pos = transform.position;

        // Limit player from floating above water surface
        if (pos.y > waterSurfaceY)
        {
            pos.y = waterSurfaceY;
            transform.position = pos;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        // Limit within spherical boundary
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
<<<<<<< Updated upstream:Fetus-Forest-VR/Assets/_Project/Scripts/PlayerController.cs
=======

    void CheckNearbyDroplets()
    {
        if (WaterDropletGridManager.Instance == null) return;

        float triggerHeight = waterSurfaceY - 0.5f; // Player must reach this height to start detecting interactions
        float checkRadius = 5.0f;

        Vector3 playerPos = transform.position;

        List<DropletInteractionController> allDroplets = WaterDropletGridManager.Instance.GetAllDroplets();

        if (playerPos.y >= triggerHeight)
        {
            // Player has reached specified height, detect nearby water droplets
            List<DropletInteractionController> nearby = WaterDropletGridManager.Instance.GetNearbyDroplets(playerPos, checkRadius);
>>>>>>> Stashed changes:Fetus-Forest-VR/Assets/_Project/Scripts/Control/PlayerController.cs

    void PlayWwiseEvent(string eventName)
    {
        // Play Wwise event using official Wwise Unity API
        if (!string.IsNullOrEmpty(eventName))
        {
<<<<<<< Updated upstream:Fetus-Forest-VR/Assets/_Project/Scripts/PlayerController.cs
            AkSoundEngine.PostEvent(eventName, gameObject);
        }
    }
=======
            // Player hasn't reached trigger height, keep all droplets in non-triggered state (but don't force reset)
            foreach (DropletInteractionController droplet in allDroplets)
            {
                droplet.SetPlayerNear(false);
            }
        }
    }

    void PlayWwiseEvent(string eventName)
    {
        // Play Wwise event using official Wwise Unity API
        if (!string.IsNullOrEmpty(eventName))
        {
            AkSoundEngine.PostEvent(eventName, gameObject);
        }
    }
>>>>>>> Stashed changes:Fetus-Forest-VR/Assets/_Project/Scripts/Control/PlayerController.cs
}