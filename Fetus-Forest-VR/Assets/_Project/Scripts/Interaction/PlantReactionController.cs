using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantReactionController : MonoBehaviour
{
    public Animator animator;
    private List<Material> matchedMaterials = new List<Material>();

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.materials; // 实例化材质，避免影响 prefab
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];

                if (mat.name.Contains("Neuron Inner_Big") || mat.name.Contains("Neuron Inner_Small"))
                {
                    if (!matchedMaterials.Contains(mat))
                    {
                        matchedMaterials.Add(mat);
                    }
                }
            }
        }
        if (matchedMaterials.Count == 0)
        {
            // Debug.LogWarning($"[{name}] 未找到匹配材质 Neuron Inner_Big / Small");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("isNear", true);
            
            foreach (var mat in matchedMaterials)
            {
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

}
