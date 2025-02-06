using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class UIManager : MonoBehaviour
{
    //Can be used to handle hiding UI as needed.

    public GameObject trashDisplay;
    public TextMeshProUGUI trashCounter;
    int trashCount;

    public Animator missionAnimator;
    public TextMeshProUGUI missionText;
    string[] successTexts =
    {
        "[Home] Took out the trash.",
        "[Home] Made rice.",
        "[Home] Finished take-home quiz.",
        "[Home] Finished take-home essay.",
        "[Chinatown] Got condoms.",
        "[Chinatown] Snuck home unnoticed.",
        "[June's House] Got camera layout.",
        "[June's House] Found infiltration route.",
    };

    // Start is called before the first frame update
    void Start()
    {
        if (trashDisplay != null)
        {
            Debug.Log("trying to find trash display");
            UpdateTrash();
        }
    }

    [YarnCommand("showsuccess")]
    public void ShowSuccess(string param)
    {
        int.TryParse(param, out int index);
        missionText.text = successTexts[index];
        missionAnimator.SetTrigger("Start");
        //missionAnimator.ResetTrigger("Start");
        Debug.Log("trying to show message " + successTexts[index]);
    }

    void UpdateTrash()
    {
        Debug.Log(GameManager.Instance.GetValue("$trashcollected"));
        int count = (int)GameManager.Instance.GetValue("$trashcollected").AsNumber;
        Debug.Log("Trash collected so far: " + count);
        if (count == 5)
        {
            trashCounter.fontStyle = FontStyles.Bold;
            trashCounter.fontSize += 5;
        }
        else if (count > 0)
        {
            trashDisplay.SetActive(true);
        }
        trashCounter.text = "Trash collected: " + count + "/5";
    }
    [YarnCommand("addtrash")]
    public void AddTrash()
    {
        GameManager.Instance.trashCollected++;
        UpdateTrash();
    }
    [YarnCommand("finishtrash")]
    public void FinishTrash()
    {
        trashDisplay.SetActive(false);
    }
}
