using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CenterDragObject : MonoBehaviour
{
    [Header("射线检测")]
    public float rayDistance = 5f;                 // 屏幕中心射线最大检测距离
    public LayerMask movableLayer;                 // 可抓取物体所在层

    [Header("拖动物体参数")]
    public float holdDistance = 2.5f;              // 当前抓取距离
    public float moveSpeed = 12f;                  // 物体跟随速度

    [Header("滚轮控制距离")]
    public float scrollSpeed = 2f;                 // 滚轮调节抓取距离的速度
    public float minHoldDistance = 1f;             // 最小抓取距离
    public float maxHoldDistance = 6f;             // 最大抓取距离

    [Header("防穿模检测")]
    public LayerMask obstacleLayer;                // 障碍物层（墙、地面、家具等）
    public float wallCheckRadius = 0.2f;           // 基础检测半径
    public float wallOffset = 0.1f;                // 与障碍物保持的安全距离
    public float objectRadiusScale = 0.2f;         // 根据物体大小动态放大检测半径的比例

    [Header("无刚体物体防穿模")]
    public float boxCastSkin = 0.02f;              // BoxCast 的安全边距
    public float minMoveCheckDistance = 0.001f;    // 小于这个距离就不做额外检测

    [Header("抓取时刚体设置")]
    public bool disableGravityWhileHolding = true; // 抓取时是否关闭重力
    public bool zeroVelocityOnPick = true;         // 抓取瞬间是否清空速度
    public bool zeroVelocityOnRelease = true;      // 释放时是否清空速度

    [Header("抓取旋转")]
    public bool allowRotateWhileHolding = true;    // 抓取时是否允许旋转
    public float rotateSpeed = 120f;               // Q/E 旋转速度（度/秒）

    private Transform currentObj;                  // 当前抓住的物体
    private Rigidbody currentRb;                   // 当前物体的刚体（可能为空）
    private Camera cam;                            // 当前摄像机

    private int originalLayer;                     // 原始层
    private Transform holdPoint;                   // 镜头前的目标抓取点



    // 物体根节点到“视觉中心”的偏移
    // 后面移动的是根节点，但我们希望对准的是视觉中心
    private Vector3 centerOffsetWorld;

    // 保存刚体抓取前原本的状态
    private RigidbodyConstraints originalConstraints;
    private bool originalUseGravity;

    void Awake()
    {
        cam = GetComponent<Camera>();

        GameObject point = new GameObject("HoldPoint");
        holdPoint = point.transform;
        holdPoint.SetParent(transform);
        holdPoint.localPosition = new Vector3(0f, 0f, holdDistance);
        holdPoint.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (currentObj != null)
        {
            UpdateHoldDistanceByScroll();
            UpdateHeldObjectRotation(); // 新增：抓取时按 Q / E 旋转
        }

        if (holdPoint != null)
        {
            float safeDistance = holdDistance;

            if (currentObj != null)
            {
                safeDistance = GetSafeHoldDistance();
            }

            holdPoint.localPosition = new Vector3(0f, 0f, safeDistance);
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPickObject();
        }

        if (Input.GetMouseButton(0) && currentObj != null)
        {
            MoveHeldObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }
    }

    /// <summary>
    /// 尝试从屏幕中心抓取物体
    /// </summary>
    void TryPickObject()
    {
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, movableLayer))
        {
            currentObj = hit.transform;
            currentRb = hit.rigidbody;

            // 如果有刚体，优先移动刚体所在物体
            if (currentRb != null)
            {
                currentObj = currentRb.transform;
            }

            if (currentObj == null) return;

            // 保存原层
            originalLayer = currentObj.gameObject.layer;

            // 抓起来后切到 Ignore Raycast，避免挡住准星射线
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreRaycastLayer != -1)
            {
                SetLayerRecursively(currentObj.gameObject, ignoreRaycastLayer);
            }

            // 如果有刚体，先处理刚体状态
            if (currentRb != null)
            {
                // 记录原始状态
                originalConstraints = currentRb.constraints;
                originalUseGravity = currentRb.useGravity;

                if (disableGravityWhileHolding)
                {
                    currentRb.useGravity = false;
                }

                if (zeroVelocityOnPick)
                {
                    currentRb.velocity = Vector3.zero;
                    currentRb.angularVelocity = Vector3.zero;
                }

                // 保持你原本思路：抓取时直接锁住旋转
                currentRb.constraints = originalConstraints
                                        | RigidbodyConstraints.FreezeRotationX
                                        | RigidbodyConstraints.FreezeRotationY
                                        | RigidbodyConstraints.FreezeRotationZ;
            }

            // 计算“视觉中心”相对于根节点的位置偏移
            Vector3 visualCenter = GetVisualCenter(currentObj);
            centerOffsetWorld = visualCenter - currentObj.position;
        }
    }

    /// <summary>
    /// 让抓住的物体持续跟随到镜头前的安全位置
    /// </summary>
    void MoveHeldObject()
    {
        if (holdPoint == null || currentObj == null) return;

        // 目标：让物体视觉中心到达 holdPoint
        Vector3 targetCenterPos = holdPoint.position;

        // 反推出根节点应该去的位置
        Vector3 targetRootPos = targetCenterPos - centerOffsetWorld;

        if (currentRb != null)
        {
            Vector3 dir = targetRootPos - currentRb.position;
            currentRb.velocity = dir * moveSpeed;
        }
        else
        {
            Vector3 safeTargetPos = GetSafeTargetPositionForNonRigidbody(currentObj, targetRootPos);
            currentObj.position = Vector3.Lerp(currentObj.position, safeTargetPos, Time.deltaTime * moveSpeed);
        }
    }

    /// <summary>
    /// 抓取时按 Q / E 旋转物体
    /// 不解除刚体约束，直接手动设置 rotation
    /// </summary>
    void UpdateHeldObjectRotation()
    {
        if (!allowRotateWhileHolding || currentObj == null) return;

        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateInput -= 1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotateInput += 1f;
        }

        if (Mathf.Abs(rotateInput) < 0.001f) return;

        float angle = rotateInput * rotateSpeed * Time.deltaTime;
        RotateHeldObject(angle);
    }

    /// <summary>
    /// 只绕世界 Y 轴旋转，适合家具
    /// 即使刚体冻结了旋转约束，也可以手动设置 rotation
    /// </summary>
    void RotateHeldObject(float angle)
    {
        if (currentObj == null) return;

        Quaternion deltaRotation = Quaternion.AngleAxis(angle, Vector3.up);

        if (currentRb != null)
        {
            currentRb.rotation = deltaRotation * currentRb.rotation;
            currentRb.angularVelocity = Vector3.zero;
        }
        else
        {
            currentObj.rotation = deltaRotation * currentObj.rotation;
        }

        // 旋转后重新计算中心偏移，避免跟随位置飘
        Vector3 visualCenter = GetVisualCenter(currentObj);
        centerOffsetWorld = visualCenter - currentObj.position;
    }

    /// <summary>
    /// 释放当前抓住的物体
    /// </summary>
    public void ReleaseObject()
    {
        if (currentObj != null)
        {
            SetLayerRecursively(currentObj.gameObject, originalLayer);
        }

        if (currentRb != null)
        {
            // 恢复抓取前的刚体设置
            currentRb.useGravity = originalUseGravity;
            currentRb.constraints = originalConstraints;

            if (zeroVelocityOnRelease)
            {
                currentRb.velocity = Vector3.zero;
                currentRb.angularVelocity = Vector3.zero;
            }

            currentRb = null;
        }

        currentObj = null;
        centerOffsetWorld = Vector3.zero;
    }

    /// <summary>
    /// 鼠标滚轮控制抓取距离
    /// 向上滚：更远
    /// 向下滚：更近
    /// </summary>
    void UpdateHoldDistanceByScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            holdDistance += scroll * scrollSpeed;
            holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
        }
    }

    /// <summary>
    /// 计算当前真正安全的抓取距离
    /// 前方如果有障碍物，就自动把抓取点往回缩
    /// </summary>
    float GetSafeHoldDistance()
    {
        if (cam == null) return holdDistance;

        Vector3 origin = cam.transform.position;
        Vector3 dir = cam.transform.forward;

        RaycastHit hit;

        // 动态计算检测半径
        float radius = wallCheckRadius;

        if (currentObj != null)
        {
            float objectRadius = GetObjectApproxRadius(currentObj);
            radius = Mathf.Max(wallCheckRadius, objectRadius * objectRadiusScale);
        }

        // 用 SphereCast 比单纯 Raycast 更适合防穿模
        if (Physics.SphereCast(origin, radius, dir, out hit, holdDistance, obstacleLayer))
        {
            float safeDistance = hit.distance - wallOffset;

            // 限制不要太近
            return Mathf.Clamp(safeDistance, minHoldDistance, holdDistance);
        }

        return holdDistance;
    }

    /// <summary>
    /// 无刚体物体专用：用 BoxCast 检查从当前位置到目标位置的移动路径是否会撞到障碍物
    /// 如果会，就把目标位置截停在碰撞前
    /// </summary>
    Vector3 GetSafeTargetPositionForNonRigidbody(Transform target, Vector3 desiredTargetPos)
    {
        Bounds bounds = GetCombinedBounds(target);

        if (bounds.size == Vector3.zero)
        {
            return desiredTargetPos;
        }

        Vector3 currentPos = target.position;
        Vector3 move = desiredTargetPos - currentPos;
        float moveDistance = move.magnitude;

        if (moveDistance < minMoveCheckDistance)
        {
            return desiredTargetPos;
        }

        Vector3 direction = move.normalized;

        // 当前包围盒中心相对于根节点的位置偏移
        Vector3 boundsCenterOffset = bounds.center - currentPos;

        // 计算当前和目标时，包围盒中心的位置
        Vector3 castStartCenter = currentPos + boundsCenterOffset;
        Vector3 halfExtents = bounds.extents;

        // 收一点安全边距，避免边缘贴得太死
        halfExtents -= Vector3.one * boxCastSkin;

        // 防止收缩后出现负数
        halfExtents.x = Mathf.Max(0.001f, halfExtents.x);
        halfExtents.y = Mathf.Max(0.001f, halfExtents.y);
        halfExtents.z = Mathf.Max(0.001f, halfExtents.z);

        RaycastHit hit;

        // 用物体当前旋转作为 BoxCast 朝向
        Quaternion orientation = target.rotation;

        if (Physics.BoxCast(castStartCenter, halfExtents, direction, out hit, orientation, moveDistance, obstacleLayer))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - wallOffset);

            // 根节点安全目标位置 = 当前位置 + 安全移动距离
            return currentPos + direction * safeDistance;
        }

        return desiredTargetPos;
    }

    /// <summary>
    /// 获取物体的大致半径，用于辅助 SphereCast 防穿模
    /// </summary>
    float GetObjectApproxRadius(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return wallCheckRadius;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.extents.magnitude;
    }

    /// <summary>
    /// 获取物体视觉中心
    /// 使用所有 Renderer 的包围盒中心，而不是 transform.position
    /// </summary>
    Vector3 GetVisualCenter(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return target.position;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center;
    }

    /// <summary>
    /// 获取整个物体所有 Renderer 合并后的包围盒
    /// </summary>
    Bounds GetCombinedBounds(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return new Bounds(target.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    /// <summary>
    /// 递归设置层
    /// 抓起物体后把它改到 Ignore Raycast，避免它自己挡住中心射线
    /// </summary>
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void OnDrawGizmos()
    {
        // 画出中心射线
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * rayDistance;
        Gizmos.DrawLine(start, end);

        // 画出抓取目标点
        Gizmos.color = Color.green;
        Vector3 holdPos = transform.position + transform.forward * holdDistance;
        Gizmos.DrawSphere(holdPos, 0.05f);

        // 画基础防穿模检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(holdPos, wallCheckRadius);
    }
}