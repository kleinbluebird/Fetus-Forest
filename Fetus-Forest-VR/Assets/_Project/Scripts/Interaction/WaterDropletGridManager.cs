using System.Collections.Generic;
using UnityEngine;

public class WaterDropletGridManager : MonoBehaviour
{
    public static WaterDropletGridManager Instance;

    private List<DropletInteractionController> droplets = new List<DropletInteractionController>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 注册水滴（在DropletInteractionController的Start()里调用）
    public void RegisterDroplet(DropletInteractionController droplet)
    {
        if (!droplets.Contains(droplet))
            droplets.Add(droplet);
    }

    // 反注册水滴（比如销毁时调用）
    public void UnregisterDroplet(DropletInteractionController droplet)
    {
        if (droplets.Contains(droplet))
            droplets.Remove(droplet);
    }

    // 获取所有水滴列表
    public List<DropletInteractionController> GetAllDroplets()
    {
        return droplets;
    }

    // 根据投影位置，判断玩家是否靠近水滴
    public List<DropletInteractionController> GetNearbyDroplets(Vector3 playerPos, float radius)
    {
        List<DropletInteractionController> nearby = new List<DropletInteractionController>();

        foreach (var droplet in droplets)
        {
            Vector3 dropletPos = droplet.transform.position;

            // 将水滴投影到地面（或玩家所在高度）
            Vector3 dropletProjected = new Vector3(dropletPos.x, playerPos.y, dropletPos.z);

            // 判断玩家是否靠近投影点（水平范围）
            float distance = Vector3.Distance(playerPos, dropletProjected);

            if (distance <= radius)
            {
                nearby.Add(droplet);
                // Debug.Log($"[投影判断] Droplet near: {droplet.name} | 投影点: {dropletProjected} | 距离: {distance:F2}");
            }
        }

        return nearby;
    }

}