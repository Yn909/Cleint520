using UnityEngine;

public class RoomEnvironment : MonoBehaviour
{
    [Header("房间信息")]
    public string roomName = "客厅";

    [Header("当前环境")]
    public float currentTemperature = 28f;
    public float currentHumidity = 65f;

    [Header("自然回归值")]
    public float naturalTemperature = 30f;
    public float naturalHumidity = 70f;

    [Header("自然回归速度")]
    public float temperatureRecoverSpeed = 0.05f;
    public float humidityRecoverSpeed = 0.05f;

    private void Update()
    {
        // 没有设备干预时，环境慢慢回归自然值
        currentTemperature = Mathf.MoveTowards(
            currentTemperature,
            naturalTemperature,
            temperatureRecoverSpeed * Time.deltaTime
        );

        currentHumidity = Mathf.MoveTowards(
            currentHumidity,
            naturalHumidity,
            humidityRecoverSpeed * Time.deltaTime
        );
    }

    public void Cool(float amountPerSecond)
    {
        currentTemperature -= amountPerSecond * Time.deltaTime;
        currentTemperature = Mathf.Clamp(currentTemperature, 16f, 50f);
    }

    public void Dehumidify(float amountPerSecond)
    {
        currentHumidity -= amountPerSecond * Time.deltaTime;
        currentHumidity = Mathf.Clamp(currentHumidity, 20f, 100f);
    }

    public void Heat(float amountPerSecond)
    {
        currentTemperature += amountPerSecond * Time.deltaTime;
        currentTemperature = Mathf.Clamp(currentTemperature, 16f, 50f);
    }
}