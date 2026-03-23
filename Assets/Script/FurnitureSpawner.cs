using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    [Header("引用")]
    public Camera playerCamera;                  // 玩家摄像机

    [Header("家具预制体")]
    public GameObject[] furniturePrefabs;        // 按钮按索引调用

    [Header("生成参数")]
    public float spawnDistance = 2.5f;           // 在玩家前方多远生成
    public float surfaceCheckDistance = 5f;      // 向下检测地面的距离
    public float spawnOffsetY = 0.02f;           // 生成时离地微微抬高一点

    [Header("检测层")]
    public LayerMask groundLayer;                // 地面层
    public LayerMask blockingLayer;              // 障碍物层（家具、墙等）

    [Header("检测修正")]
    public float checkPadding = 0.02f;           // XZ方向缩小一点，避免过严
    public float verticalCheckShrink = 0.05f;    // Y方向额外缩一点，减少贴地误判

    [Header("父物体")]
    public Transform initParent;                 // 生成后挂载的父物体，可为空

    /// <summary>
    /// UI按钮调用：根据索引生成家具
    /// </summary>
    public void SpawnFurnitureByIndex(int index)
    {
        if (index < 0 || index >= furniturePrefabs.Length)
        {
            Debug.LogWarning("家具索引越界：" + index);
            return;
        }

        SpawnFurniture(furniturePrefabs[index]);
    }

    /// <summary>
    /// 生成指定家具
    /// </summary>
    public void SpawnFurniture(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("家具预制体为空");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("没有指定玩家摄像机");
            return;
        }

        // 临时创建一个检测物体，只用于计算包围盒和贴地，不作为最终物体
        GameObject tempObj = Instantiate(prefab);
        tempObj.name = prefab.name + "_TempCheck";

        Bounds bounds = GetCombinedBounds(tempObj);

        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning("预制体没有 Renderer，无法计算大小：" + prefab.name);
            Destroy(tempObj);
            return;
        }

        // 计算玩家前方位置
        Vector3 forwardPos = playerCamera.transform.position + playerCamera.transform.forward * spawnDistance;

        // 从这个位置往下找地面
        RaycastHit groundHit;
        Vector3 rayStart = forwardPos + Vector3.up * 2f;

        if (!Physics.Raycast(rayStart, Vector3.down, out groundHit, surfaceCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("前方没有地面，不能生成：" + prefab.name);
            Destroy(tempObj);
            return;
        }

        // 只保留玩家Y轴朝向
        Vector3 euler = playerCamera.transform.eulerAngles;
        Quaternion spawnRot = Quaternion.Euler(0f, euler.y, 0f);

        // 先设置旋转
        tempObj.transform.rotation = spawnRot;

        // 计算模型当前旋转下，真正的底部偏移
        float bottomOffset = GetBottomOffset(tempObj);

        // 最终生成位置：让模型最低点贴到地面上
        Vector3 spawnPos = groundHit.point + Vector3.up * (-bottomOffset + spawnOffsetY);

        tempObj.transform.position = spawnPos;

        // 放到最终位置后，再算一次包围盒
        bounds = GetCombinedBounds(tempObj);

        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents;

        // XZ 略缩一点，避免检测过严
        halfExtents.x = Mathf.Max(0.01f, halfExtents.x - checkPadding);
        halfExtents.z = Mathf.Max(0.01f, halfExtents.z - checkPadding);

        // Y方向多缩一点，减少与地面接触时误判
        halfExtents.y = Mathf.Max(0.01f, halfExtents.y - checkPadding - verticalCheckShrink);

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            spawnRot,
            blockingLayer,
            QueryTriggerInteraction.Ignore
        );

        bool isBlocked = false;

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            // 忽略地面层
            if (IsInLayerMask(hit.gameObject.layer, groundLayer))
                continue;

            // 忽略临时检测物体自己
            if (hit.transform.IsChildOf(tempObj.transform))
                continue;

            Debug.Log("检测到阻挡物: " + hit.name);
            isBlocked = true;
        }

        if (isBlocked)
        {
            Debug.Log("前方位置被占用，不能生成：" + prefab.name);
            Destroy(tempObj);
            return;
        }

        // 正式生成
        GameObject newObj;

        if (initParent != null)
        {
            newObj = Instantiate(prefab, spawnPos, spawnRot, initParent);
        }
        else
        {
            newObj = Instantiate(prefab, spawnPos, spawnRot);
        }

        Destroy(tempObj);
    }

    /// <summary>
    /// 获取整个预制体所有 Renderer 合并后的包围盒
    /// </summary>
    Bounds GetCombinedBounds(GameObject obj)
    {
        Renderer[] renders = obj.GetComponentsInChildren<Renderer>(true);

        if (renders == null || renders.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.zero);
        }

        Bounds bounds = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++)
        {
            bounds.Encapsulate(renders[i].bounds);
        }

        return bounds;
    }

    /// <summary>
    /// 计算物体当前状态下，模型最低点相对于根节点原点的Y偏移
    /// </summary>
    float GetBottomOffset(GameObject obj)
    {
        Renderer[] renders = obj.GetComponentsInChildren<Renderer>(true);

        if (renders == null || renders.Length == 0)
            return 0f;

        float minY = float.MaxValue;

        for (int i = 0; i < renders.Length; i++)
        {
            if (renders[i].bounds.min.y < minY)
            {
                minY = renders[i].bounds.min.y;
            }
        }

        // 返回“最低点 - 根节点Y”
        return minY - obj.transform.position.y;
    }

    /// <summary>
    /// 判断某一层是否在 LayerMask 中
    /// </summary>
    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}