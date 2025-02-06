using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteSwap))]
public class PivotCamera : SecurityObject
{
    [Header("Unique")]
    public float angleIncrement;
    public SecurityDirection initialDirection;
    Vector3 initialEulerAngles;
    int index;
    bool reversing;

    public SpriteSwap spriteSwap;
    public SpriteRenderer otherSr;
    public Transform toRotate;
    public TriggerColliderWithEvents col;

    public enum SecurityDirection
    {
        L,
        C,
        R,
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        initialEulerAngles = toRotate.eulerAngles;
        index = (int)initialDirection;
        MoveFov();
    }
    protected override void Update()
    {
        base.Update();

        if (!seesPlayer)
        {
            return;
        }
        timeSeeingPlayer += Time.deltaTime;
        if (timeSeeingPlayer > (IsAlerted() ? delayReseen : delaySeen))
        {
            timeSeeingPlayer = 0;
            AlertSecurity();
        }
    }
    protected override void ToggleOn(bool setting)
    {
        base.ToggleOn(setting);
        if (isOn)
        {
            StartCoroutine(Pivot());
        }
        else
        {
            StopAllCoroutines();
        }
    }
    IEnumerator Pivot()
    {
        while (isOn)
        {
            while (!PlayerControl.Instance.CanAct())
            {
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(delaySwap);
            RotateFov();
            yield return new WaitForSeconds(delayCycle);
        }
    }
    void RotateFov()
    {
        if (index == 2)
        {
            reversing = true;
        }
        else if (index == 0)
        {
            reversing = false;
        }
        if (!singleDirection)
        {
            if (reversing)
            {
                index--;
            }
            else
            {
                index++;
            }
        }
        else
        {
            if (startsMovingRight)
            {
                index++;
                if (index > 2)
                {
                    index = 0;
                }
            }
            else
            {
                index--;
                if (index < 0)
                {
                    index = 2;
                }
            }
        }
        MoveFov();
    }
    void MoveFov()
    {
        //Debug.Log(index);
        switch (index)
        {
            case 0:
                toRotate.eulerAngles = initialEulerAngles + new Vector3(0, 0, -angleIncrement);
                break;
            case 1:
                toRotate.eulerAngles = initialEulerAngles;
                break;
            case 2:
                toRotate.eulerAngles = initialEulerAngles + new Vector3(0, 0, angleIncrement);
                break;
        }
        spriteSwap.SwapSprite(index);
    }
    public override void ToggleSeePlayer(bool setting)
    {
        base.ToggleSeePlayer(setting);
        if (seesPlayer)
        {
            otherSr.color = noColor;
        }
        else
        {
            otherSr.color = okColor;
        }
    }
}
