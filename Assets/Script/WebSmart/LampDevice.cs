using UnityEngine;

public class LampDevice : SmartDevice
{
    [Header("台灯参数")]
    public Light targetLight1, targetLight2;
    [Range(0, 100)] public int brightness = 50;
    public string colorHex = "#FFD966";

    private void Reset()
    {
        deviceName = "台灯";
        deviceType = "lamp";
    }

    public override void SetPower(bool on)
    {
        base.SetPower(on);
        ApplyLightState();
    }

    public void SetBrightness(int value)
    {
        brightness = Mathf.Clamp(value, 0, 100);
        ApplyLightState();
    }

    public void SetColor(string hex)
    {
        colorHex = hex;
        ApplyLightState();
    }

    protected virtual void ApplyLightState()
    {
        if (targetLight1 == null&& targetLight2==null) return;

        targetLight1.gameObject.SetActive(isOn);
        targetLight2.gameObject.SetActive(isOn);
        if (!ColorUtility.TryParseHtmlString(colorHex, out Color color))
        {
            color = Color.white;
        }

        targetLight1.color = color;
        targetLight2.color = color;
        targetLight1.intensity = Mathf.Lerp(0f, 3f, brightness / 100f);
        targetLight2.intensity= Mathf.Lerp(0f, 3f, brightness / 100f);
    }

    public override string ToJson()
    {
        return "{"
               + "\"id\":" + deviceId + ","
               + "\"name\":\"" + deviceName + "\","
               + "\"deviceType\":\"" + deviceType + "\","
               + "\"roomName\":\"" + roomName + "\","
               + "\"isOn\":" + (isOn ? "true" : "false") + ","
               + "\"brightness\":" + brightness + ","
               + "\"color\":\"" + colorHex + "\""
               + "}";
    }
}