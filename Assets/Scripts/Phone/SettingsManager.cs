using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsManager : AppManager
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void Navigate(Vector2 input)
    {
        if (input.x > 0.1 && buttonIndex % 2 == 0)
        {
            CheckOption(buttonIndex + 1);
        }
        else if (input.x < -0.1 && buttonIndex % 2 != 0)
        {
            CheckOption(buttonIndex - 1);
        }
        if (input.y > 0.1)
        {
            CheckOption(buttonIndex + 2);
        }
        else if (input.y < -0.1)
        {
            CheckOption(buttonIndex - 2);
        }
    }
    public override void Focus()
    {
        base.Focus();
    }
    public override void Back()
    {
        base.Back();
        if (expanded)
        {

        }
        else
        {
            if (PhoneManager.Instance.currentApp != PhoneManager.PhoneApp.HOME)
                PhoneManager.Instance.OpenApp("home");
        }
    }
    void UnexpandSetting()
    {

    }
}
