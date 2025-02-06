using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject[] credits;
    public GameObject[] menu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowMenu()
    {
        foreach (GameObject go in credits)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in menu)
        {
            go.SetActive(true);
        }
    }
    public void ShowCredits()
    {
        foreach (GameObject go in menu)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in credits)
        {
            go.SetActive(true);
        }
    }
}
