using UnityEngine;

public class AirConditionDevice : SmartDevice
{
    [Header("空调参数")]
    public string mode = "cool"; // cool / dry / heat
    [Range(0, 100)] public int windSpeed = 50;
    [Range(16, 30)] public int targetTemperature = 24;

    [Header("作用房间环境")]
    public RoomEnvironment targetRoomEnvironment;

    [Header("影响强度")]
    public float baseCoolSpeed = 0.15f;
    public float baseDrySpeed = 0.25f;
    public float baseHeatSpeed = 0.15f;

    private void Reset()
    {
        deviceName = "空调";
        deviceType = "aircondition";
    }

    private void Update()
    {
        if (!isOn || targetRoomEnvironment == null) return;

        float windFactor = Mathf.Lerp(0.2f, 2f, windSpeed / 100f);

        switch (mode)
        {
            case "cool":
                if (targetRoomEnvironment.currentTemperature > targetTemperature)
                {
                    targetRoomEnvironment.Cool(baseCoolSpeed * windFactor);
                }
                break;

            case "dry":
                // 除湿主要降湿度，附带轻微降温
                targetRoomEnvironment.Dehumidify(baseDrySpeed * windFactor);

                if (targetRoomEnvironment.currentTemperature > targetTemperature)
                {
                    targetRoomEnvironment.Cool(baseCoolSpeed * 0.35f * windFactor);
                }
                break;

            case "heat":
                if (targetRoomEnvironment.currentTemperature < targetTemperature)
                {
                    targetRoomEnvironment.Heat(baseHeatSpeed * windFactor);
                }
                break;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Check"))
        {
            if(other.gameObject.name=="客厅")
            {
                targetRoomEnvironment = other.gameObject.GetComponent<RoomEnvironment>();
            }
            if (other.gameObject.name == "卧室1")
            {
                targetRoomEnvironment = other.gameObject.GetComponent<RoomEnvironment>();
            }
        }
    }
    public void SetMode(string newMode)
    {
        if (newMode != "cool" && newMode != "dry" && newMode != "heat") return;
        mode = newMode;
    }

    public void SetWindSpeed(int value)
    {
        windSpeed = Mathf.Clamp(value, 0, 100);
    }

    public void SetTargetTemperature(int value)
    {
        targetTemperature = Mathf.Clamp(value, 16, 30);
    }

    public override string ToJson()
    {
        return "{"
               + "\"id\":" + deviceId + ","
               + "\"name\":\"" + deviceName + "\","
               + "\"deviceType\":\"" + deviceType + "\","
               + "\"roomName\":\"" + roomName + "\","
               + "\"isOn\":" + (isOn ? "true" : "false") + ","
               + "\"mode\":\"" + mode + "\","
               + "\"windSpeed\":" + windSpeed + ","
               + "\"targetTemperature\":" + targetTemperature
               + "}";
    }
}