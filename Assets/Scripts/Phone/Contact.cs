using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Contact : MonoBehaviour
{
    public string contactName;
    public string displayName;
    public GameObject convoObj;
    TextConversation convo;
    public TextMeshProUGUI preview;
    public GameObject newMessage;
    public List<Button> buttons;
    bool notified;

    private void Start()
    {
        newMessage.SetActive(false);
        convo = ConvoTracker.GetConvo(contactName);
        
        convoObj = convo.gameObject;

        TextMessage tm = convo.GetBottom();
        if (tm)
        {
            Debug.Log("found a bottom");
            if (tm.message.text.Length > 0)
                preview.text = tm.message.text;
            else
                preview.text = tm.defaultMessage.content;
        }
        buttons = convo.GetButtons();
        if (buttons == null || buttons.Count < 2)
            Debug.LogWarning("buttons not counted correctly");
    }
    public void GetMessage(Message m)
    {
        newMessage.SetActive(true);
        convo.Add(m);
    }
    public void Open()
    {
        Debug.Log($"opening {contactName}");
        newMessage.SetActive(false);
        convoObj.SetActive(true);
    }
    public void Close()
    {
        newMessage.SetActive(false);
        convoObj.SetActive(false);
    }
    public void Notify(Message m)
    {
        Debug.Log($"notifying {contactName}");
        preview.text = m.content;
        convo.Add(m);
        newMessage.SetActive(true);
    }
}
