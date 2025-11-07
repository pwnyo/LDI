using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class YarnLight : YarnItem
{
    public UnityEngine.Rendering.Universal.Light2D lights;
    public Color onColor, offColor, darkColor;
    public void SetLights(bool on)
    {
        lights.color = on ? onColor : offColor;
    }
}
