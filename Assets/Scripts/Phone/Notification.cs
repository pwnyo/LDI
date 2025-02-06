using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    private static readonly float INTIME = 0.25f;
    private static readonly float OUTTIME = 0.5f;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void SetText(string newText)
    {
        text.text = newText;
    }
    public void Show(float holdTime)
    {
        Debug.Log("showing");
        StartCoroutine(ShowAndHide(holdTime));
    }
    public void Show(float holdTime, float intime, float outtime)
    {
        Debug.Log("showing");
        StartCoroutine(ShowAndHide(holdTime));
    }
    IEnumerator ShowAndHide(float holdTime)
    {
        float timer = 0;
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;

        if (anim)
            anim.SetBool("Show", true);

        while (timer < INTIME)
        {
            yield return null;
            timer += Time.deltaTime;
            canvasGroup.alpha = (timer / INTIME);
        }
        canvasGroup.alpha = 1;
        yield return new WaitForSeconds(holdTime);

        timer = 0;
        while (timer < OUTTIME)
        {
            yield return null;
            timer += Time.deltaTime;
            canvasGroup.alpha = 1 - (timer / OUTTIME);
        }

        if (anim)
            anim.SetBool("Show", false);

        canvasGroup.alpha = 0;
        //gameObject.SetActive(false);
    }
}
