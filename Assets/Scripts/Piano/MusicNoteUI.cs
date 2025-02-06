using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles display for individual music notes on the staff
/// </summary>
public class MusicNoteUI : MonoBehaviour
{
    public Color cActive;
    public Color cSquare, cCross, cCircle, cTriangle;
    public Sprite spSquare, spCross, spCircle, spTriangle;
    public Image button, icon, ledgerLine;
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public RectTransform rect;
    public MusicNoteState state;

    public enum ButtonType
    {
        NONE,
        UP,
        LEFT,
        DOWN,
        RIGHT,
        B_UP,
        B_LEFT,
        B_DOWN,
        B_RIGHT,
        TRIGGER_L,
        TRIGGER_R,
    }

    public enum MusicNoteState
    {
        NONE,
        ACTIVE,
        INACTIVE,
        HIDDEN,
    }

    public void Redraw(DeviceType deviceType = DeviceType.KEYBOARD)
    {
        if (deviceType == DeviceType.KEYBOARD)
        {

        }
        else
        {

        }
    }
    void SetText(string text)
    {

    }
    void SetIcon(ButtonType buttonType)
    {
        if (buttonType == ButtonType.NONE)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        icon.gameObject.SetActive(false);
        SetText("");
        icon.sprite = spCircle;
    }
    void GetIconFromMusicKey(MusicKey key)
    {

    }
    public void SetState(MusicNoteState state)
    {
        this.state = state;
        if (state == MusicNoteState.HIDDEN)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (state == MusicNoteState.ACTIVE)
        {
            button.color = cActive;
            //icon.color
            SetAlpha(1f);
        }
        else if (state == MusicNoteState.INACTIVE)
        {
            button.color = Color.black;
            icon.color = Color.white;
            SetAlpha(0.5f);
        }
    }
    public void SetPositionRaw(float x, float y, int staffKey)
    {
        rect.anchoredPosition = new Vector2(x, y);
    }
    public void SetPosition(float x, float yOffset, float ySpacing, int noteNum)
    {
        //noteNum = MusicPlayer.Instance.GetStaffKey(noteNum);

        rect.anchoredPosition = new Vector2(x, yOffset + ySpacing * noteNum);
    }
    void SetAlpha(float val)
    {
        canvasGroup.alpha = val;
    }
}
