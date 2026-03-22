using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectHome : MonoBehaviour
{
    public Player player;
    public Eyes eyes;
    public CenterDragObject dragObject;
    public GameObject HomePanel;
    bool iSEnablePanel=false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            iSEnablePanel = !iSEnablePanel;

            HomePanel.SetActive(iSEnablePanel);
            player.enabled = !iSEnablePanel;
            eyes.enabled = !iSEnablePanel;
            dragObject.enabled = !iSEnablePanel;

            Cursor.visible = iSEnablePanel;
            Cursor.lockState = iSEnablePanel ? CursorLockMode.None : CursorLockMode.Locked;
        }
    
    }
}
