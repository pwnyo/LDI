using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class AppManager : MonoBehaviour
{
    public RectTransform rt;
    public Button defaultButton;
    public Notification notif;
    protected bool expanded;
    protected bool isBackable;
    [SerializeField]
    protected List<Button> buttons;
    protected int buttonIndex;

    public virtual void Focus()
    {
        if (defaultButton)
        {
            defaultButton.Select();
            buttonIndex = 0;
        }
    }
    public virtual void Back()
    {
        //TODO: Should focus the button you selected and then backed from
        if (!isBackable)
            return;
    }
    public virtual void Back(InputAction.CallbackContext context)
    {

    }
    public void SetBackable(bool setting)
    {
        isBackable = setting;
    }
    public virtual void Navigate(Vector2 input)
    {

    }
    public virtual void SelectOption(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.inConvo ||
            buttons == null || buttons.Count == 0 || buttonIndex < 0 || buttonIndex >= buttons.Count)
        {
            return;
        }
        buttons[buttonIndex].onClick.Invoke();
    }
    protected virtual void CheckOption(int index)
    {
        Debug.Log($"current index is {index}");
        if (index >= 0 && index < buttons.Count)
        {
            buttonIndex = index;
            buttons[index].Select();
            Debug.Log($"selecting option {buttons[index].name}");
        }
    }
}
