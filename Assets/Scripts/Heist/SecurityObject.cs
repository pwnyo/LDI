using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityObject : MonoBehaviour
{
    [Header("Main")]
    public bool isOn;
    public bool startsMovingRight;
    public bool singleDirection;
    public int threatAmount;
    protected bool seesPlayer;
    SecurityManager sm;

    [Header("Times")]
    public float delayStart;
    public float delaySeen;
    public float delayReseen;
    public float delaySwap;
    public float delayCycle;
    protected float startTime, currentTime;
    protected float timeSeeingPlayer;

    [Header("Visuals")]
    public Color okColor;
    public Color noColor;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        sm = SecurityManager.Instance;
        startTime = Time.time;
        isOn = false;
    }
    protected virtual void Update()
    {
        if (!PlayerControl.Instance.CanAct())
        {
            return;
        }
        if (currentTime <= startTime + delayStart)
        {
            currentTime += Time.deltaTime;
            return;
        }
        else if (!isOn)
        {
            ToggleOn(true);
        }

    }
    public virtual void ToggleSeePlayer(bool setting)
    {
        seesPlayer = setting;
        if (!seesPlayer)
        {
            timeSeeingPlayer = 0;
            UnalertSecurity();
        }
    }
    protected bool IsAlerted()
    {
        return sm.HasSeen(this);
    }
    protected void AlertSecurity()
    {
        sm.AddSeenBySecurity(this);
    }
    protected void UnalertSecurity()
    {
        sm.RemoveSeenBySecurity(this);
    }
    protected virtual void ToggleOn(bool setting)
    {
        isOn = setting;
    }
}
