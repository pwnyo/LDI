using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnHelper : MonoBehaviour
{
    [YarnCommand("sprite")]
    public void Sprite(string param)
    {
        bool.TryParse(param, out bool setting);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = setting;
        }
    }
    [YarnCommand("enable")]
    public void Enable(string param)
    {
        bool.TryParse(param, out bool setting);
        gameObject.SetActive(setting);
    }
    [YarnCommand("animateb")]
    public void SetAnimateB(string[] param)
    {
        if (param.Length != 2)
        {
            Debug.Log("animateb was not passed 2 parameters!");
            return;
        }
        else
        {
            Animator a = GetComponent<Animator>();
            if (a)
            {
                bool.TryParse(param[1], out bool setting);
                a.SetBool(param[0], setting);
            }
        }
    }
    [YarnCommand("animatet")]
    public void SetAnimateI(string param)
    {
        Animator a = GetComponent<Animator>();
        if (a)
        {
            a.SetTrigger(param);
        }
    }
}
