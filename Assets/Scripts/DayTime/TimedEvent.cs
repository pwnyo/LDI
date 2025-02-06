using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEvent : MonoBehaviour
{
    public string dialogueNode;
    [Range(0, 4)]
    public int day;
    public bool allDays;
    public GameManager.TimeOfDay tod;
    public Clock.ClockTime triggerTime;

    public bool IsTime(Clock.ClockTime current)
    {
        return triggerTime == current;
    }
    public bool IsDay(int day, GameManager.TimeOfDay tod)
    {
        if (tod != this.tod)
        {
            return false;
        }
        return allDays || day == this.day;
    }
    public void TriggerEvent()
    {
        Debug.Log($"{dialogueNode} met condition!");
        GameDialogueManager.Instance.dr.StartDialogue(dialogueNode);
    }
}
