using UnityEngine;

public class LightToggleSimple : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 5f;
    // 灯所属的父物体
    public GameObject[] lights;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleLight();
        }
    }

    void ToggleLight()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
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
            
            }
        }
    }
}