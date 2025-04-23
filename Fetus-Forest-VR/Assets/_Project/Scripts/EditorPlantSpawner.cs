#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class EditorPlantSpawner : MonoBehaviour
{
    public GameObject[] plantPrefabs;
    public GameObject landscape;
    public int plantCount = 200;
    public Vector3 areaSize = new Vector3(50, 0, 50);
    public float minScale = 0.8f;
    public float maxScale = 1.5f;
    public float minSpacing = 1.5f;

    private List<Vector3> placedPositions = new List<Vector3>();

    [ContextMenu("Spawn Plants")]
    public void SpawnPlants()
    {
        if (plantPrefabs.Length == 0 || landscape == null)
        {
            Debug.LogWarning("Prefab and Landscape are empty！");
            return;
        }

        // Remove the old ones
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        placedPositions.Clear();

        int attempts = 0;
        int placed = 0;

        while (placed < plantCount && attempts < plantCount * 10)
        {
            attempts++;

            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                10f,
                Random.Range(-areaSize.z / 2, areaSize.z / 2)
            );

            Ray ray = new Ray(randomPos, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
				Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 2f);

                if (hit.collider.gameObject == landscape)
                {
                    // Distance Check
                    bool tooClose = false;
                    foreach (Vector3 pos in placedPositions)
                    {
                        if (Vector3.Distance(pos, hit.point) < minSpacing)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
                    GameObject plant = PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;

                    plant.transform.position = hit.point;
                    plant.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    float scale = Random.Range(minScale, maxScale);
                    plant.transform.localScale = Vector3.one * scale;

                    placedPositions.Add(hit.point);
                    placed++;
                }
				else
			    {
			        Debug.Log("Hit Objects：" + hit.collider.gameObject.name);
			    }
            }
			else
			{
    			Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 2f);
			}
        }

        Debug.Log($"Generate {placed} Plants（Attempt {attempts} times）");
    }
}
#endif
