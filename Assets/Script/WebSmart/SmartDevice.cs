using UnityEngine;
//****************所有设备父类************************
public abstract class SmartDevice : MonoBehaviour
{
    [Header("通用信息")]
    public int deviceId;
    public string deviceName = "设备";
    public string deviceType = "device";
    public string roomName = "未分组";
    public bool isOn = false;

    public virtual void InitDevice(int id, string room)
    {
        deviceId = id;
        roomName = room;
    }

    public virtual void SetPower(bool on)
    {
        isOn = on;
    }

    public virtual void TogglePower()
    {
        SetPower(!isOn);
    }

    /// <summary>
    /// 返回发给前端的 JSON 片段
    /// 子类可以 override 扩展属性
    /// </summary>
    public virtual string ToJson()
    {
        return "{"
               + "\"id\":" + deviceId + ","
               + "\"name\":\"" + deviceName + "\","
               + "\"deviceType\":\"" + deviceType + "\","
               + "\"roomName\":\"" + roomName + "\","
               + "\"isOn\":" + (isOn ? "true" : "false")
               + "}";
    }
}