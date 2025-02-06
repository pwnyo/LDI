using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class YarnLight : YarnItem
{
    public Light2D lights;
    public Color onColor, offColor, darkColor;
    public void SetLights(bool on)
    {
        lights.color = on ? onColor : offColor;
    }
}
