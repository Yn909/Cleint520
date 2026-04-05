using UnityEngine;

public class SensorDevice : SmartDevice
{
    [Header("绑定房间环境")]
    public RoomEnvironment targetRoomEnvironment;

    [Header("当前读数")]
    public float temperature = 24.7f;
    public float humidity = 51.1f;

    [Header("同步设置")]
    public float syncInterval = 1f;
    private float syncTimer = 0f;

    private void Reset()
    {
        deviceName = "温湿度传感器";
        deviceType = "sensor";
    }

    private void Update()
    {
        if (targetRoomEnvironment != null)
        {
            temperature = targetRoomEnvironment.currentTemperature;
            humidity = targetRoomEnvironment.currentHumidity;
        }

        syncTimer += Time.deltaTime;
        if (syncTimer >= syncInterval)
        {
            syncTimer = 0f;

            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.SendDeviceUpdate(this);
            }
        }
    }

    public override string ToJson()
    {
        return "{"
               + "\"id\":" + deviceId + ","
               + "\"name\":\"" + deviceName + "\","
               + "\"deviceType\":\"" + deviceType + "\","
               + "\"roomName\":\"" + roomName + "\","
               + "\"isOn\":" + (isOn ? "true" : "false") + ","
               + "\"temperature\":" + temperature.ToString("F1") + ","
               + "\"humidity\":" + humidity.ToString("F1")
               + "}";
    }
}