using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class EventManager : MonoBehaviour
{
    [YarnCommand("doessay")]
    public void DoEssay()
    {
        GameManager.Instance.doneEssay = true;
    }
    [YarnCommand("doquiz")]
    public void DoQuiz()
    {
        GameManager.Instance.doneQuiz = true;
    }
    [YarnCommand("dotrash")]
    public void DoTrash()
    {
        GameManager.Instance.doneTrash = true;
    }
    [YarnCommand("dorice")]
    public void DoRice()
    {
        GameManager.Instance.doneRice = true;
    }
}
