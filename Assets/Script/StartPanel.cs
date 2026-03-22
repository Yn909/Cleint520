using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartPanel : MonoBehaviour
{
    public Button StartBt, QuitBt;
    // Start is called before the first frame update
    void Start()
    {
        StartBt.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(1);
        });
        QuitBt.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

   
}
