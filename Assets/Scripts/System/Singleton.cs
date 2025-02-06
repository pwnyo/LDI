using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public string id;
    private static List<Singleton> list = new List<Singleton>();

    private void Awake() { 
        foreach (Singleton s in list)
        {
            if (s.id == this.id)
            {
                Destroy(this.gameObject);
                return;
            }
        }
        list.Add(this);
        DontDestroyOnLoad(this.gameObject);
    }
}
