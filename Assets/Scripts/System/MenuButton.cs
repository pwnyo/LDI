using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene()
    {
        if (sceneToLoad.Equals("exit"))
        {
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
