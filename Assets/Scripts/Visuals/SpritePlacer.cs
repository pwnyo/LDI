using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePlacer : Talker
{
    public string variableToCheck;
    public GameObject[] items;

    protected override void Start()
    {
        foreach(GameObject g in items)
        {
            g.SetActive(false);
        }
    }

    public override void Interact()
    {
        base.Interact();
        int count = PlayerPrefs.GetInt(variableToCheck);

    }
}
