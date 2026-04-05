using UnityEngine;

public class RoomDeviceRegistrar : MonoBehaviour
{
    [Header("房间名")]
    public string roomName = "客厅";

    [Header("该房间下的设备")]
    public SmartDevice[] roomDevices;

    void Start()
    {
        if (DeviceManager.Instance == null) return;

        foreach (var device in roomDevices)
        {
            if (device != null)
            {
                DeviceManager.Instance.RegisterDevice(device, roomName);
            }
        }

        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.SendAllDevices();
        }
    }
}