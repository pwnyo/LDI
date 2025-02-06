using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextMessage : MonoBehaviour
{
    //TODO: Replace the hideous TextLine() function with something like this
    private static string PLAYER_NAME = "Eddy";
    public bool onLeft;
    public Color leftColor; //static?
    public Color rightColor; //static?
    public Animator textAnimator;
    public RectTransform box;
    public RectTransform messageBox;
    public TextMeshProUGUI messengerName;
    public TextMeshProUGUI message;
    public Image boxImage;
    public Message defaultMessage;

    private void Awake()
    {
        if (defaultMessage.content.Length > 0)
            Push(defaultMessage);
    }
    private void Start()
    {
        Align();
    }
    public TextMessage(bool lefty, string name, string content)
    {
        onLeft = lefty;
        if (name.Length == 0)
            messengerName.text = PLAYER_NAME;
        else
            messengerName.text = name;
        message.text = content.TrimStart();
    }
    public void Push(Message m)
    {
        onLeft = m.header.Length != 0 && m.header != PLAYER_NAME;
        if (!onLeft)
            messengerName.text = PLAYER_NAME;
        message.text = m.content.TrimStart();
        Align();
    }
    public void Align(bool left)
    {
        onLeft = left;
        Align();
    }
    public void Align()
    {
        //Debug.Log($"aligning {onLeft}");
        //if left, put stuff over here; if right, put stuff over there
        if (onLeft)
        {
            //container.anchoredPosition = new Vector2(-15, 0);
            //messengerBox.anchoredPosition = new Vector2(-400, 100);
            boxImage.color = leftColor;
            messageBox.pivot = new Vector2(0, 1);
            messageBox.anchoredPosition = new Vector3(-25, -4, 0);

            messengerName.alignment = TextAlignmentOptions.TopLeft;
            message.alignment = TextAlignmentOptions.TopLeft;
            //messageA.GetComponent<Image>().color = leftColor;
        }
        else
        {
            //container.anchoredPosition = new Vector2(15, 0);
            //messengerBox.anchoredPosition = new Vector2(400, 100);
            boxImage.color = rightColor;
            messageBox.pivot = new Vector2(1, 1);
            messageBox.anchoredPosition = new Vector3(25, -4, 0);

            messengerName.alignment = TextAlignmentOptions.TopRight;
            message.alignment = TextAlignmentOptions.TopRight;
        }

        //Resize text box
        if (GameDialogueManager.Instance)
        {
            DebugHelper dh = GameDialogueManager.Instance.debugHelper;
            float messageBoxTargetHeight = message.preferredHeight + 2.5f;
            messageBox.sizeDelta = new Vector3(messageBox.sizeDelta.x, 0);
            box.sizeDelta = new Vector3(box.sizeDelta.x, 0);

            messageBox.DOSizeDelta(new Vector2(Mathf.Min(message.preferredWidth + 4, 50), messageBoxTargetHeight), dh.debugMiscVal).SetEase(dh.debugMiscEase);
            box.DOSizeDelta(new Vector2(box.sizeDelta.x, messageBoxTargetHeight + 4), dh.debugMiscVal).SetEase(dh.debugMiscEase);
        }
        else
        {
            messageBox.sizeDelta = new Vector2(Mathf.Min(message.preferredWidth + 4, 50), message.preferredHeight + 2.5f);
            box.sizeDelta = new Vector2(box.sizeDelta.x, messageBox.sizeDelta.y + 4);
        }

        //Place up or down
        //Play animation to move the other message up, and an animation to pop into place
        //...
    }
}
