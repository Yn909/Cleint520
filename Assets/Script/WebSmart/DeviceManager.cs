using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    public static DeviceManager Instance;

    [Header("所有已注册设备")]
    public List<SmartDevice> devices = new List<SmartDevice>();

    [Header("设备ID起始值")]
    public int nextId = 1001;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 注册设备（静态设备和动态设备都走这里）
    /// </summary>
    public void RegisterDevice(SmartDevice device, string roomName)
    {
        if (device == null) return;

        // 防止重复注册
        if (devices.Contains(device)) return;

        // 自动分配ID
        device.deviceId = nextId++;

        // 更新房间名
        device.roomName = roomName;

        // 加入列表
        devices.Add(device);

        Debug.Log($"注册设备成功: {device.deviceName}, ID={device.deviceId}, 房间={device.roomName}");

        // 同步给前端
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.SendAllDevices();
        }
    }

    /// <summary>
    /// 移除设备
    /// </summary>
    public void RemoveDevice(SmartDevice device)
    {
        if (device == null) return;

        if (devices.Contains(device))
        {
            devices.Remove(device);

            Debug.Log($"移除设备: {device.deviceName}, ID={device.deviceId}");

            // 同步给前端
            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.SendAllDevices();
            }
        }
    }

    /// <summary>
    /// 根据ID获取设备
    /// </summary>
    public SmartDevice GetDeviceById(int id)
    {
        return devices.Find(d => d.deviceId == id);
    }

    /// <summary>
    /// 构建全部设备列表 JSON
    /// 发送格式：
    /// {
    ///   "type":"device_list",
    ///   "devices":[ ... ]
    /// }
    /// </summary>
    public string BuildDeviceListJson()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"type\":\"device_list\",\"devices\":[");

        for (int i = 0; i < devices.Count; i++)
        {
            if (devices[i] != null)
            {
                sb.Append(devices[i].ToJson());

                if (i < devices.Count - 1)
                {
                    sb.Append(",");
                }
            }
        }

        sb.Append("]}");
        return sb.ToString();
    }

    /// <summary>
    /// 构建单个设备更新 JSON
    /// 发送格式：
    /// {
    ///   "type":"device_update",
    ///   "device": { ... }
    /// }
    /// </summary>
    public string BuildSingleDeviceUpdateJson(SmartDevice device)
    {
        if (device == null) return "";

        return "{"
               + "\"type\":\"device_update\","
               + "\"device\":" + device.ToJson()
               + "}";
    }
}