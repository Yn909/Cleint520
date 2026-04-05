using UnityEngine;
using NativeWebSocket;
using System.Text;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance;

    [Header("服务器地址")]
    public string serverUrl = "ws://localhost:3000";

    private WebSocket websocket;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("已连接 Node 服务器");

            SendText("{\"type\":\"unity_register\"}");
            SendAllDevices();

            if (HomeStatusManager.Instance != null)
            {
                HomeStatusManager.Instance.SendHomeStatus();
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log("收到消息: " + msg);

            HandleMessage(msg);
        };

        websocket.OnError += (err) =>
        {
            Debug.LogError("WebSocket错误: " + err);
        };

        websocket.OnClose += (code) =>
        {
            Debug.Log("连接关闭");
        };

        await websocket.Connect();
    }

    // 发送文本
    public async void SendText(string json)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(json);
        }
    }

    // 发全部设备
    public void SendAllDevices()
    {
        if (DeviceManager.Instance == null) return;

        string json = DeviceManager.Instance.BuildDeviceListJson();
        SendText(json);
    }

    // 发单个设备更新
    public void SendDeviceUpdate(SmartDevice device)
    {
        if (device == null) return;

        string json = DeviceManager.Instance.BuildSingleDeviceUpdateJson(device);
        SendText(json);
    }

    //  处理前端控制
    void HandleMessage(string msg)
    {
        if (!msg.Contains("\"type\":\"control_device\"")) return;

        int deviceId = ExtractInt(msg, "deviceId");
        string action = ExtractString(msg, "action");
        string value = ExtractRaw(msg, "value");

        SmartDevice device = DeviceManager.Instance.GetDeviceById(deviceId);

        if (device == null)
        {
            Debug.LogWarning("找不到设备: " + deviceId);
            return;
        }

        ExecuteCommand(device, action, value);

        //  执行完一定要回传
        SendDeviceUpdate(device);
    }

    //  执行设备操作
    void ExecuteCommand(SmartDevice device, string action, string value)
    {
        Debug.Log($"执行命令 -> 设备:{device.deviceName} ID:{device.deviceId} 动作:{action} 值:{value}");

        switch (action)
        {
            case "toggle":
                device.TogglePower();
                return;

            case "set_isOn":
                if (value == "true")
                    device.SetPower(true);
                else if (value == "false")
                    device.SetPower(false);
                return;
        }

        if (device is LampDevice lamp)
        {
            if (action == "set_brightness")
            {
                if (int.TryParse(value, out int b))
                    lamp.SetBrightness(b);
            }
            else if (action == "set_color")
            {
                lamp.SetColor(Trim(value));
            }

            return;
        }

        if (device is AirConditionDevice air)
        {
            if (action == "set_mode")
            {
                air.SetMode(Trim(value));
            }
            else if (action == "set_wind_speed")
            {
                if (int.TryParse(value, out int speed))
                    air.SetWindSpeed(speed);
            }
            else if (action == "set_target_temperature")
            {
                if (int.TryParse(value, out int temp))
                    air.SetTargetTemperature(temp);
            }

            return;
        }

        if (device is TVDevice tv)
        {
            if (action == "set_channel")
            {
                if (int.TryParse(value, out int channel))
                    tv.SetChannel(channel);
            }
            else if (action == "set_volume")
            {
                if (int.TryParse(value, out int volume))
                    tv.SetVolume(volume);
            }

            return;
        }
    }
    //void ExecuteCommand(SmartDevice device, string action, string value)
    //{
    //    switch (action)
    //    {
    //        case "toggle":
    //            device.TogglePower();
    //            return;

    //        case "set_isOn":
    //            if (value == "true")
    //                device.SetPower(true);
    //            else if (value == "false")
    //                device.SetPower(false);
    //            return;
    //    }

    //    if (device is LampDevice lamp)
    //    {
    //        if (action == "set_brightness")
    //        {
    //            if (int.TryParse(value, out int b))
    //                lamp.SetBrightness(b);
    //        }
    //        else if (action == "set_color")
    //        {
    //            lamp.SetColor(Trim(value));
    //        }

    //        return;
    //    }

    //    if (device is AirConditionDevice air)
    //    {
    //        if (action == "set_mode")
    //        {
    //            air.SetMode(Trim(value));
    //        }
    //        else if (action == "set_wind_speed")
    //        {
    //            if (int.TryParse(value, out int speed))
    //                air.SetWindSpeed(speed);
    //        }
    //        else if (action == "set_target_temperature")
    //        {
    //            if (int.TryParse(value, out int temp))
    //                air.SetTargetTemperature(temp);
    //        }

    //        return;
    //    }

    //    if (device is TVDevice tv)
    //    {
    //        if (action == "set_channel")
    //        {
    //            if (int.TryParse(value, out int channel))
    //                tv.SetChannel(channel);
    //        }
    //        else if (action == "set_volume")
    //        {
    //            if (int.TryParse(value, out int volume))
    //                tv.SetVolume(volume);
    //        }

    //        return;
    //    }
    //}
    //void ExecuteCommand(SmartDevice device, string action, string value)
    //{

    //    switch (action)
    //    {
    //        case "toggle":
    //            device.TogglePower();
    //            return;

    //        case "set_isOn":
    //            if (value == "true")
    //                device.SetPower(true);
    //            else if (value == "false")
    //                device.SetPower(false);
    //            return;
    //    }
    //    if (device is LampDevice lamp)
    //    {
    //        if (action == "set_brightness")
    //        {
    //            if (int.TryParse(value, out int v))
    //                lamp.SetBrightness(v);
    //        }
    //        else if (action == "set_color")
    //        {
    //            lamp.SetColor(Trim(value));
    //        }
    //    }

    //    if (device is AirConditionDevice air)
    //    {
    //        if (action == "set_mode")
    //        {
    //            air.SetMode(Trim(value));
    //        }
    //        else if (action == "set_wind_speed")
    //        {
    //            if (int.TryParse(value, out int v))
    //                air.SetWindSpeed(v);
    //        }
    //        else if (action == "set_target_temperature")
    //        {
    //            if (int.TryParse(value, out int v))
    //                air.SetTargetTemperature(v);
    //        }

    //        return;
    //    }

    //    if (device is TVDevice tv)
    //    {
    //        if (action == "set_channel")
    //        {
    //            if (int.TryParse(value, out int v))
    //                tv.SetChannel(v);
    //        }

    //        if (action == "set_volume")
    //        {
    //            if (int.TryParse(value, out int v))
    //                tv.SetVolume(v);
    //        }

    //    }
    //}

    // ------------------------
    // 简易 JSON 解析（够用）
    // ------------------------

    int ExtractInt(string json, string key)
    {
        string pattern = "\"" + key + "\":";
        int start = json.IndexOf(pattern);
        if (start < 0) return 0;

        start += pattern.Length;
        int end = json.IndexOf(",", start);
        if (end < 0) end = json.IndexOf("}", start);

        int.TryParse(json.Substring(start, end - start), out int result);
        return result;
    }

    string ExtractString(string json, string key)
    {
        string pattern = "\"" + key + "\":\"";
        int start = json.IndexOf(pattern);
        if (start < 0) return "";

        start += pattern.Length;
        int end = json.IndexOf("\"", start);

        return json.Substring(start, end - start);
    }

    string ExtractRaw(string json, string key)
    {
        string pattern = "\"" + key + "\":";
        int start = json.IndexOf(pattern);
        if (start < 0) return "";

        start += pattern.Length;

        int end = json.IndexOf(",", start);
        if (end < 0) end = json.IndexOf("}", start);

        return json.Substring(start, end - start).Trim();
    }

    string Trim(string v)
    {
        if (v.StartsWith("\"")) v = v.Substring(1);
        if (v.EndsWith("\"")) v = v.Substring(0, v.Length - 1);
        return v;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
        if(Input.GetKey(KeyCode.LeftShift)&&Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }
}