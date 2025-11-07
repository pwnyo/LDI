using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HeistSwitch : Reactor
{
    [Header("Switch")]
    [SerializeField]
    bool startsOn;
    [SerializeField]
    GameObject lightObj;
    [SerializeField]
    SpriteRenderer switchSr, lightSr;
    [SerializeField]
    Sprite onSprite, offSprite;
    [SerializeField]
    Color onColor, offColor;
    [SerializeField]
    UnityEvent onSwitchOn, onSwitchOff;

    protected override void Start()
    {
        base.Start();
        isOn = startsOn;
        Switch();
    }

    public override void Interact()
    {
        base.Interact();
        Switch();
    }
    void Switch()
    {
        if (isOn)
        {
            onSwitchOn.Invoke();
            switchSr.sprite = onSprite;
            lightSr.color = onColor;
        }
        else
        {
            onSwitchOff.Invoke();
            switchSr.sprite = offSprite;
            lightSr.color = offColor;
        }
        lightObj.SetActive(isOn);
    }
}
