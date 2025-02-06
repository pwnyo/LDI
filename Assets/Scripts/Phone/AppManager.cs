using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AppManager : MonoBehaviour
{
    public Notification notif;
    protected bool expanded;
    protected bool isBackable;

    public virtual void Back()
    {
        if (!isBackable)
            return;
    }
    public void SetBackable(bool setting)
    {
        isBackable = setting;
    }
}
