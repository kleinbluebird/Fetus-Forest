using UnityEngine;

public class DropletInteractionController : MonoBehaviour
{
    private float sinkTargetY = 0f;
    public float sinkSpeed = 0.5f;
    public float returnSpeed = 0.5f;

    private float originalY;
    private float currentYValue; // Independent Y value control
    private bool isPlayerNear = false;
    private bool isMoving = false; // Whether movement is needed

    private VerticalOscillator oscillator;

    private Material dropletMaterial;
    private Color initialRimColor = new Color(0.4811321f, 0.4811321f, 0.4811321f, 1f);
    private Color targetRimColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    private Color currentRimColor;

    [Header("Wwise Audio Settings")]
    [Tooltip("Wwise event name to trigger when interaction starts")]
    public string wwiseEventStart = "Rope";
    [Tooltip("Wwise event name to trigger when droplet reaches sink position")]
    public string wwiseEventMaintain = "Point";
    [Tooltip("Wwise event name to trigger when droplet starts returning")]
    public string wwiseEventReturn = "Rope";

    private bool hasTriggeredMaintain = false; // Track if maintain event has been triggered

    void Start()
    {
        originalY = transform.localPosition.y;
        currentYValue = originalY;
        oscillator = GetComponent<VerticalOscillator>();

        // Find and instantiate material named "Babe Inner" from all child objects
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true); // Include inactive child objects
        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials; // Copy current material array

            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.name.Contains("Babe Inner"))
                {
                    // Instantiate material
                    Material newMat = Instantiate(mat);
                    materials[i] = newMat; // Replace corresponding item in copy array
                    rend.materials = materials; // Set back to renderer's material array

                    dropletMaterial = newMat;
                    currentRimColor = initialRimColor;

                    // Set color parameters
                    dropletMaterial.SetColor("_StylingRimColor", currentRimColor);

                    // Debug logging
                    // Debug.Log($"[Droplet] Material replacement successful: {newMat.name}");
                    // Debug.Log($"[Rim Debug] EnableRimStyling: {dropletMaterial.GetFloat("_EnableRimStyling")}");
                    // Debug.Log($"[Rim Debug] StylingRimColor: {dropletMaterial.GetColor("_StylingRimColor")}");
                    break;
                }
            }

            if (dropletMaterial != null) break; // Already found and set, stop continuing
        }

        if (dropletMaterial == null)
        {
            Debug.LogWarning($"[Droplet] Could not find 'Babe Inner' material on children of {gameObject.name}");
        }


        // Register to manager
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

        // Interpolated movement
        currentYValue = Mathf.MoveTowards(currentYValue, targetY, Time.deltaTime * speed);
        transform.localPosition = new Vector3(transform.localPosition.x, currentYValue, transform.localPosition.z);

        // Debug.Log($"[Droplet] isPlayerNear: {isPlayerNear}, currentY: {currentY:F3}, targetY: {targetY:F3}, speed: {speed}");

        // Color interpolation
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


        // Stop movement if position is close enough to target
        if (Mathf.Abs(currentYValue - targetY) < 0.001f)
        {
            isMoving = false;

            // Trigger maintain event when reaching sink position (only once)
            if (isPlayerNear && !hasTriggeredMaintain)
            {
                TriggerWwiseEvent(wwiseEventMaintain, "Maintain");
                hasTriggeredMaintain = true;
            }

            if (!isPlayerNear && oscillator != null)
            {
                oscillator.enabled = true;
                hasTriggeredMaintain = false; // Reset for next interaction
            }
        }
    }

    public void SetPlayerNear(bool near)
    {
        if (isPlayerNear != near)
        {
            isPlayerNear = near;
            // Debug.Log($"[SetPlayerNear] Changing to {(near ? "NEAR" : "FAR")}, current local Y before oscillator = {transform.localPosition.y:F3}");
            isMoving = true;

            if (near)
            {
                // Trigger start event when interaction begins (player comes near)
                TriggerWwiseEvent(wwiseEventStart, "Start");

                if (oscillator != null)
                {
                    oscillator.enabled = false;
                    // Debug.Log($"[Oscillator] Now enabled = {!near}, after enabling, Y = {transform.localPosition.y:F3}");
                }
            }
            else
            {
                // Trigger return event when player leaves and droplet starts returning
                TriggerWwiseEvent(wwiseEventReturn, "Return");
            }
        }
    }

    /// <summary>
    /// Trigger the specified Wwise audio event using this GameObject as the audio source
    /// </summary>
    /// <param name="eventName">The Wwise event name to trigger</param>
    /// <param name="stageName">The interaction stage name for logging</param>
    private void TriggerWwiseEvent(string eventName, string stageName)
    {
        if (!string.IsNullOrEmpty(eventName))
        {
            // Post Wwise event with this GameObject as the audio source
            AkSoundEngine.PostEvent(eventName, gameObject);
            Debug.Log($"[Droplet Audio] Triggered {stageName} Wwise event: {eventName} on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Droplet Audio] {stageName} Wwise event name is empty on {gameObject.name}");
        }
    }

    void OnDestroy()
    {
        WaterDropletGridManager.Instance?.UnregisterDroplet(this);
    }
}