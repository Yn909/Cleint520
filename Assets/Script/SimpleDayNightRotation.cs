using UnityEngine;

public class SimpleDayNightRotation : MonoBehaviour
{
    [Header("时间设置")]
    [Range(0f, 24f)]
    public float currentTime = 12f;      // 当前时间
    public bool autoRun = true;          // 是否自动运行
    public float dayDurationInMinutes = 5f;  // 一天现实时间

    [Header("太阳")]
    public Light directionalLight;       // 你的 Directional Light
    public float fixedYRotation = -381.25f;  // 固定Y（你现在用的）
    public float fixedZRotation = 0f;         // 固定Z

    int hour;
    int minute;
    float temperature = 25f;
    float targetTemp;
    float changeInterval = 5f; // 每5秒换一次目标
    float timer = 0f;
    public HomeStatusManager statusManager;

    private void Awake()
    {
        SetNewTarget();
        statusManager = GetComponent<HomeStatusManager>();
    }
    void Update()
    {
        if (directionalLight == null) return;

        // 自动时间流动
        if (autoRun)
        {
            float fullDaySeconds = dayDurationInMinutes * 60f;
            currentTime += (24f / fullDaySeconds) * Time.deltaTime;
            hour = Mathf.FloorToInt(currentTime);
            minute= Mathf.FloorToInt((currentTime - hour) * 60f);
            statusManager.hour = hour;
            statusManager.minute = minute;
            //  平滑过渡（控制变化速度）
            temperature = Mathf.MoveTowards(temperature, targetTemp, Time.deltaTime );

            // 计时
            timer += Time.deltaTime;

            if (timer >= changeInterval)
            {
                SetNewTarget();
                timer = 0f;
            }
            statusManager.currentTemperature = temperature;//温度的变化
            statusManager.SendHomeStatus();
            if (currentTime >= 24f)
                currentTime -= 24f;
        }

        // 只做一件事：旋转太阳
        float xAngle = GetSunXAngleByTime(currentTime);

        directionalLight.transform.rotation =
            Quaternion.Euler(xAngle, fixedYRotation, fixedZRotation);
    }
    void SetNewTarget()
    {
        targetTemp = Random.Range(20f, 25f);
    }
    float GetSunXAngleByTime(float time)
    {
        // 时间 → 角度映射
        // 0点 = -90
        // 6点 = 0
        // 12点 = 90
        // 18点 = 180
        // 24点 = 270
        return (time / 24f) * 360f - 90f;
    }
}