using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextManager : AppManager
{
    [SerializeField]
    private Contact[] contacts;
    [SerializeField]
    private GameObject optionButtonParent;
    public GameObject contactContainer;
    public Animator notificationAnimator;
    public TextMeshProUGUI currentContactName;

    bool isNotifying;
    public GameObject newMessageObj;
    public TextMeshProUGUI newMessageText;
    int newMessages = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ForceOpen(string contactName, Message m)
    {
        Notify(contactName, m);
        Open(contactName);
    }
    public void Open(string contactName)
    {
        foreach (Contact c in contacts)
        {
            if (c.contactName == contactName)
            {
                c.Open();
                currentContactName.text = contactName;
                newMessages = 0;
                expanded = true;
                contactContainer.SetActive(false);

                if (c.buttons != null && c.buttons.Count > 0)
                    GameDialogueManager.Instance.dui.optionButtons = c.buttons;
            }
        }
        optionButtonParent.SetActive(true);
    }
    public void Notify(string contactName, Message m)
    {
        newMessages++;
        newMessageText.text = newMessages.ToString();
        Debug.Log("notifying");
        foreach (Contact c in contacts)
        {
            if (c.contactName == contactName)
            {
                c.transform.SetAsFirstSibling();
                c.Notify(m);
                
                if (PhoneManager.Instance.currentApp != PhoneManager.PhoneApp.TEXTS)
                    notif.Show(1f, .25f, .25f);

                return;
            }
        }
    }
    public void Notify(Message m)
    {
        newMessages++;
        newMessageText.text = newMessages.ToString();
        foreach (Contact c in contacts)
        {
            if (c.contactName == m.header)
            {
                c.transform.SetAsFirstSibling();
                c.Notify(m);
                
                return;
            }
        }
    }
    public override void SelectOption(InputAction.CallbackContext context)
    {
        if (expanded && !GameDialogueManager.Instance.IsWaitingForOptions())
        {
            return;
        }
        if (GameManager.Instance.inConvo ||
            buttons == null || buttons.Count == 0 || buttonIndex < 0 || buttonIndex >= buttons.Count)
        {
            return;
        }
        buttons[buttonIndex].onClick.Invoke();
    }

    public override void Navigate(Vector2 input)
    {
        if (input.y > 0.1)
        {
            CheckOption(buttonIndex - 1);
        }
        else if (input.y < -0.1)
        {
            CheckOption(buttonIndex + 1);
        }
    }
    public override void Focus()
    {
        if (expanded)
        {
            Back();
        }
        base.Focus();
    }
    public override void Back()
    {
        if (GameManager.Instance.inConvo)
        {
            return;
        }
        base.Back();
        if (expanded)
        {
            foreach (Contact c in contacts)
            {
                c.Close();
            }
            currentContactName.text = "";
            contactContainer.SetActive(true);
            optionButtonParent.SetActive(false);
        }
        else
        {
            if (PhoneManager.Instance.currentApp != PhoneManager.PhoneApp.HOME)
                PhoneManager.Instance.OpenApp("home");
        }
        expanded = false;
    }
}
