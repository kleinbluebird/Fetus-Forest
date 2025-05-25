using UnityEngine;

public class WaterDepthController : MonoBehaviour
{
    public Transform playerTransform;

    void Update()
    {
        if (playerTransform != null)
        {
            // Set Water_Depth RTPC to player's Y position
            AkSoundEngine.SetRTPCValue("Water_Depth", playerTransform.position.y);
        }
    }
}