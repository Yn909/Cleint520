using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCtr : MonoBehaviour
{
    public Transform door;           // 门
    public float openAngle = 90f;    // 打开角度
    public float speed = 2f;         // 开门速度

    private Quaternion closeRot;
    private Quaternion openRot;

    private bool isOpen = false;

    void Start()
    {
        closeRot = door.rotation;
        openRot = closeRot * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        if (isOpen)
        {
            door.rotation = Quaternion.Lerp(door.rotation, openRot, Time.deltaTime * speed);
        }
        //else
        //{
        //    door.rotation = Quaternion.Lerp(door.rotation, closeRot, Time.deltaTime * speed);
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("AAA");
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    isOpen = false;
        //}
    }
}
