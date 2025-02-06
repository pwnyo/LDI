using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class EssayManager : MonoBehaviour
{
    public GameObject essay;
    public TMP_InputField essayText;
    public TextMeshProUGUI essayWordCount;
    public Button[] essayButtons;
    public string essayNode;

    private void Start()
    {
        SceneManager.sceneUnloaded += EssayDown;
    }

    [YarnCommand("essayup")]
    public void EssayUp()
    {
        EssayGrader.isUp = true;
        GameManager.Instance.inEssay = true;

        essay.SetActive(true);
        foreach (Button b in essayButtons)
        {
            b.interactable = true;
        }
        if (EssayGrader.essay != null)
        {
            essayText.text = EssayGrader.essay;
        }
    }
    void EssayDown(Scene scene)
    {
        EssayGrader.SaveEssay(essayText.text);
        //essay.SetActive(false);
        GameManager.Instance.inEssay = false;
        EssayGrader.isUp = false;
    }
    [YarnCommand("essaydown")]
    public void EssayDown()
    {
        EssayGrader.SaveEssay(essayText.text);
        essay.SetActive(false);

        GameManager.Instance.inEssay = false;
        EssayGrader.isUp = false;
    }
    public void EssayGrade()
    {
        string s = essayText.text;
        EssayGrader.GradeEssay(s);

        essayText.interactable = false;
        foreach (Button b in essayButtons)
        {
            b.interactable = false;
        }

        GameDialogueManager.Instance.dr.StartDialogue(essayNode);
    }
    [YarnCommand("essayfeedback")]
    public void EssayFeedback()
    {
        essayText.interactable = true;
        foreach (Button b in essayButtons)
        {
            b.interactable = true;
        }
    }
    public void UpdateWordCount()
    {
        essayWordCount.text = $"{essayText.text.Length}/600";
    }
}
