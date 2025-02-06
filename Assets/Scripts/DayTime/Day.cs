using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Day : MonoBehaviour
{
    [SerializeField]
    private TimedObjects[] _timesOfDay;
    private DialogueRunner dr;
    public Clock.ClockTime dayStart, dayEnd;

    private void Start()
    {
        dr = GameDialogueManager.Instance.dr;
        if (_timesOfDay.Length != 8)
        {
            Debug.LogError("Not enough timed objects!");
        }
        else
        {
            for (int i = 0; i < _timesOfDay.Length; i++)
            {
                if (!_timesOfDay[i])
                {
                    Debug.LogError("Not enough timed objects!");
                    return;
                }
            }
        }

    }
    public void ShowTimeOfDay(GameManager.TimeOfDay tod, bool autoplay = true)
    {
        bool found = false;
        for (int i = (int)tod; i < _timesOfDay.Length; i++)
        {
            if (_timesOfDay[i] != null)
            {
                ShowTimedObject(_timesOfDay[i], autoplay);
                return;
            }
            else
            {
                Debug.Log($"No timed object found at tod {(GameManager.TimeOfDay)i}. Moving to next...");
            }
        }
        if (!found)
            Debug.LogError($"No timed object found for {tod}!");
    }
    void ShowTimedObject(TimedObjects to, bool autoplay = false)
    {
        Debug.Log($"showing timed object for {to.name}");
        if (!dr)
            dr = GameDialogueManager.Instance.dr;
        foreach (TimedObjects t in _timesOfDay)
        {
            if (t == to)
            {
                t.gameObject.SetActive(true);
                if (t.nodeToAutoplay.Length > 0 && autoplay)
                {
                    //Debug.Log(t.nodeToAutoplay);
                    dr.StartDialogue(t.nodeToAutoplay);
                }
            }
            else if (t != null)
                t.gameObject.SetActive(false);
        }
    }
    public void HideAll()
    {
        //Debug.Log("hiding all");
        foreach (TimedObjects t in _timesOfDay)
        {
            t.gameObject.SetActive(false);
        }
    }
    public void PlayPreTransitionNode(GameManager.TimeOfDay tod)
    {
        if (!dr)
            dr = GameDialogueManager.Instance.dr;
        TimedObjects to = null;
        for (int i = (int)tod; i < _timesOfDay.Length; i++)
        {
            if (_timesOfDay[i] != null)
            {
                to = _timesOfDay[i];
                break;
            }
        }
        if (to && to.nodeToAutoplayPreTransition.Length > 0 && dr != null)
        {
            //Debug.Log("pretransition 2");
            Debug.Log(dr.NodeExists(to.nodeToAutoplayPreTransition));
            
            dr.StartDialogue(to.nodeToAutoplayPreTransition);
        }
    }
    public void Hide()
    {
        gameObject.SetActive(true);
    }
    public void Hide(GameManager.TimeOfDay tod)
    {
        //ShowTimeOfDay(tod);
        gameObject.SetActive(true);
    }
}
