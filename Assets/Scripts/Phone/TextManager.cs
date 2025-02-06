using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : AppManager
{
    [SerializeField]
    private Contact[] contacts;
    [SerializeField]
    private GameObject buttonParent;
    [SerializeField]
    private Button[] buttons;
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
        buttonParent.SetActive(true);
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
                
                if (PhoneManager.Instance.phoneApp != PhoneManager.PhoneApp.TEXTS)
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
    public override void Back()
    {
        base.Back();
        if (expanded)
        {
            foreach (Contact c in contacts)
            {
                c.Close();
            }
            currentContactName.text = "";
            contactContainer.SetActive(true);
            buttonParent.SetActive(false);
        }
        else
        {
            if (PhoneManager.Instance.phoneApp != PhoneManager.PhoneApp.HOME)
                PhoneManager.Instance.OpenApp("home");
        }
        expanded = false;
    }
}
