using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkMessage : MonoBehaviour
{
    public TMP_FontAsset englishFont;
    public TMP_FontAsset vietFont;
    public Color englishFontColor;
    public Color vietFontColor;
    public Color englishBoxColor;
    public Color vietBoxColor;
    public Color eNameColor;
    public Color vNameColor;
    public Sprite eBox, vBox;
    public float eFontSize;
    public float vFontSize;
    public Animator talkAnimator;
    public RectTransform talkContainer;
    public Image talkBox;
    public Image face;
    public TextMeshProUGUI speaker;
    public TextMeshProUGUI speech;
    public FaceManager faceManager;
    bool isViet;
    public string CurrentSpeechText { get; private set; }
    public string CurrentSpeechFull { get; private set; }
    public string CurrentSpeakerName { get; private set; }
    public string CurrentSpeakerFull { get; private set; }
    private string currentExpression;


    private void Awake()
    {
        CurrentSpeechText = "";
        CurrentSpeechFull = "";
        CurrentSpeakerName = "";
        CurrentSpeakerFull = "";
    }

    public void Push(Message m, Sprite s = null)
    {
        Debug.Log($"{m.header}: {m.content}");
        speaker.text = m.header;
        if (m.header.Length > 0)
        {
            if (isViet)
            {
                CurrentSpeakerFull = $"<color=white>{m.header}</color>:";
            }
            else
            {
                CurrentSpeakerFull = $"<color=yellow>{m.header}</color>:";
            }
            CurrentSpeechFull = $"{CurrentSpeakerFull}{m.content}";
            speech.text = CurrentSpeechFull;
        }
        else
        {
            CurrentSpeakerFull = "";
            CurrentSpeechFull = m.content;
            speech.text = CurrentSpeechFull;
        }
        speech.maxVisibleCharacters = m.header.Length;
        if (m.header != CurrentSpeakerName)
            talkAnimator.SetTrigger("ChangeFace");

        CurrentSpeechText = m.content;
        CurrentSpeakerName = m.header;

        SetFace();
        SetViet(isViet);
    }
    public void ShowMore()
    {
        speech.maxVisibleCharacters++;
    }
    public void ShowAll()
    {
        speech.maxVisibleCharacters = speech.text.Length;
    }
    public void ResetFace()
    {
        talkAnimator.ResetTrigger("ChangeFace");
        currentExpression = "";
    }
    public void Align(GameDialogueManager.BoxLocation loc)
    {
        Debug.Log($"aligning to {loc}");
        RectTransform rt = talkContainer;
        switch (loc)
        {
            case (GameDialogueManager.BoxLocation.UPPER):
                rt.localPosition = new Vector3(0, -50, 0);
                Debug.Log(rt.localPosition);
                break;
            case (GameDialogueManager.BoxLocation.UP):
                rt.localPosition = new Vector3(0, -150, 0);
                break;
            case (GameDialogueManager.BoxLocation.DOWN):
                rt.localPosition = new Vector3(0, -615, 0);
                break;
            case (GameDialogueManager.BoxLocation.DOWNER):
                rt.localPosition = new Vector3(0, -725, 0);
                break;
            case (GameDialogueManager.BoxLocation.CENTER):
                rt.localPosition = new Vector3(0, -400, 0);
                break;
        }
    }
    void SetFace()
    {
        face.enabled = true;
        Sprite sp = faceManager.GetFaceFromName(currentExpression, CurrentSpeakerName);
        if (sp != null)
        {
            face.sprite = faceManager.GetFaceFromName(currentExpression, CurrentSpeakerName);
        }
        else
        {
            face.sprite = faceManager.GetDefaultFace(currentExpression);
        }
        if (!faceManager.IsNamedCharacter(CurrentSpeakerName))
        {
            face.enabled = false;
        }
    }
    public void Express(string exp)
    {
        currentExpression = exp;
    }
    public void SetViet(bool setting)
    {
        isViet = setting;
        if (isViet)
        {
            talkBox.sprite = vBox;
            talkBox.color = vietBoxColor;
            speech.font = vietFont;
            speech.color = vietFontColor;
        }
        else
        {
            talkBox.sprite = eBox;
            talkBox.color = englishBoxColor;
            speech.font = englishFont;
            speech.color = englishFontColor;
        }
    }
}
