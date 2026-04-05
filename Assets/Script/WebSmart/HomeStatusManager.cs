using UnityEngine;

public class HomeStatusManager : MonoBehaviour
{
    public static HomeStatusManager Instance;

    [Range(0, 23)] public int hour = 20;
    [Range(0, 59)] public int minute = 3;
    public float currentTemperature = 24.7f;

    public float syncInterval = 1f;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= syncInterval)
        {
            timer = 0f;
            SendHomeStatus();
        }
    }

    public string GetTimeString()
    {
        return hour.ToString("00") + ":" + minute.ToString("00");
    }

    public string BuildHomeStatusJson()
    {
        return "{"
               + "\"type\":\"home_status\","
               + "\"currentTime\":\"" + GetTimeString() + "\","
               + "\"currentTemperature\":" + currentTemperature.ToString("F1")
               + "}";
    }

    public void SendHomeStatus()
    {
        if (WebSocketManager.Instance != null)

        {
            WebSocketManager.Instance.SendText(BuildHomeStatusJson());
        }
    }
}