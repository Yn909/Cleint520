using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpLight : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 5f;
    // 灯所属的父物体
    public GameObject[] lights;
    public CeilingLampDevice ceilingLampDevice;
    int ignoreLayer;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleLight();
        }
    }
    private void Awake()
    {
        ceilingLampDevice = GetComponent<CeilingLampDevice>();
        ignoreLayer = LayerMask.GetMask("IgnoreRaycast");
    }
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
                ceilingLampDevice.isOn = newState;
                WebSocketManager.Instance.SendDeviceUpdate(ceilingLampDevice);


            }
        }
    }
}
