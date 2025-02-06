using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MusicPieceUI : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image paper;
    public TextMeshProUGUI title, subtitle;
    public Image checkbox, check;
    public Animator checkAnimator;

    public Sprite sAlert, sDone;
    public Color cUncheckedBox, cCheckedBox, cAlert, cDone;

    public void SetPiece(string title, string subtitle, bool isDone, Color paperColor)
    {
        this.title.text = title;
        this.subtitle.text = subtitle;
        paper.color = paperColor;
        DrawCheckbox(isDone);
    }
    public void MovePos(float x, float y, float dur, Ease ease, bool isOpening)
    {
        rectTransform.DOAnchorPos(new Vector2(x, y), dur).SetEase(ease).OnComplete(() => EnableObjs(isOpening));
    }
    /// <summary>
    /// Called to reset when the folder is first opened and closed
    /// </summary>
    /// <param name="isActive"></param>
    public void ResetToDefault(bool isActive)
    {
        rectTransform.anchoredPosition = Vector2.zero;
        gameObject.SetActive(isActive);
    }
    public void SetPos(float x, float y)
    {
        rectTransform.anchoredPosition = new Vector2(x, y);
    }
    void DrawCheckbox(bool isDone)
    {
        if (isDone)
        {
            checkbox.color = cCheckedBox;
            check.color = cDone;
            check.sprite = sDone;
        }
        else
        {
            checkbox.color = cUncheckedBox;
            check.color = cAlert;
            check.sprite = sAlert;
        }
    }
    void EnableObjs(bool isOpening)
    {
        checkbox.gameObject.SetActive(isOpening);
        if (isOpening)
        {
            checkAnimator.Play("bob");
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
