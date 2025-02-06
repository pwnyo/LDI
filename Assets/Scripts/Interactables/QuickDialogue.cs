using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class QuickDialogue : MonoBehaviour
{
    TMP_InputField input;
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<TMP_InputField>();
    }
    public void PlayDialogue(string text)
    {
        DialogueRunner dr = GameDialogueManager.Instance.dr;
        if (dr.NodeExists(text))
        {
            dr.StartDialogue(text);
            input.text = "";
        }
        else
        {
            input.text = "Couldn't find that node!";
            input.Select();
        }
    }
    public void PausePlayer(bool setting)
    {

    }
}
