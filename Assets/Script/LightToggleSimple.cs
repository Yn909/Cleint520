using UnityEngine;

public class LightToggleSimple : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 5f;
    // 灯所属的父物体
    public GameObject[] lights;
    public LampDevice lampDevice;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleLight();
        }
    }
    private void Awake()
    {
        playerCamera = Camera.main;
        lampDevice = GetComponent<LampDevice>();
        ignoreLayer = LayerMask.GetMask("IgnoreRaycast");
    }
    int ignoreLayer;
    void ToggleLight()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, ~ignoreLayer))
        {
           
            if (hit.collider.CompareTag("Light"))
            {
                if (hit.collider.name != gameObject.name)
                    return;
                // 以第一个灯当前状态作为开关依据
                bool newState = !lights[0].activeInHierarchy;
              
                foreach (var l in lights)
                {
                    l.SetActive(newState);

                }
                lampDevice.isOn = newState;
                WebSocketManager.Instance.SendDeviceUpdate(lampDevice);


            }
        }
    }
}