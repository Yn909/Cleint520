using UnityEngine;

[RequireComponent(typeof(SmartDevice))]
public class AutoRegisterDevice : MonoBehaviour
{
    public string roomName = "Î´·Ö×é";
    SmartDevice device;

    void Start()
    {
        device = GetComponent<SmartDevice>();

    
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Check"))
        {
            roomName = other.gameObject.name;
            if (DeviceManager.Instance != null && device != null)
            {
                DeviceManager.Instance.RegisterDevice(device, roomName);
            }
        }
    }
    void OnDestroy()
    {
        SmartDevice device = GetComponent<SmartDevice>();

        if (DeviceManager.Instance != null && device != null)
        {
            DeviceManager.Instance.RemoveDevice(device);
        }
    }
}