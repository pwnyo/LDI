using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] o = GameObject.FindGameObjectsWithTag("DialogueRunner");

        if (o.Length > 1)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
