using UnityEngine;

public class TVDevice : SmartDevice
{
    [Header("电视参数")]
    public int channel = 1;
    [Range(0, 100)] public int volume = 30;

    public TVVideoSwitcher TV;

    private void Reset()
    {
        deviceName = "电视";
        deviceType = "tv";
    }
    public override void SetPower(bool on)
    {
        base.SetPower(on);
        ApplyTVState();
    }
    protected virtual void ApplyTVState()
    {
        if (TV == null) return;
        if (isOn == true)
            TV.PlayCurrentVideo();
        if (isOn == false)
            TV.StopVideo();
    }
    public void SetChannel(int value)
    {
        channel = Mathf.Max(1, value);
        TV.PlayNextVideo(channel);
        isOn = true;
    }
    float v;
    public void SetVolume(int value)
    {
        volume = Mathf.Clamp(value, 0, 100);
        v = volume / 100f;
        TV.videoPlayer.SetDirectAudioVolume(0, v);
       
    }

    public override string ToJson()
    {
        return "{"
               + "\"id\":" + deviceId + ","
               + "\"name\":\"" + deviceName + "\","
               + "\"deviceType\":\"" + deviceType + "\","
               + "\"roomName\":\"" + roomName + "\","
               + "\"isOn\":" + (isOn ? "true" : "false") + ","
               + "\"channel\":" + channel + ","
               + "\"volume\":" + volume
               + "}";
    }
}