using UnityEngine;
using UnityEngine.VFX;

public class EyeTransformActivator : MonoBehaviour
{
    public VisualEffect vfxGraph;
    public float particleSize = 5.0f;
    public float spawnRate = 3.0f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 需要确保玩家物体有 "Player" 标签
        {
            if (vfxGraph != null)
            {
                vfxGraph.SetBool("EnableTransform", true);
                vfxGraph.SetFloat("MinParticleSize",particleSize);
                vfxGraph.SetFloat("MaxParticleSize",particleSize);
                vfxGraph.SetFloat("SpawnRate", spawnRate);
                
                Debug.Log("Flipbook Player Activated.");
            }
        }
    }
}
