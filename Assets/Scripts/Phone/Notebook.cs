using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Notebook : MonoBehaviour
{
    public enum NoteGroup
    {
        HOME,
        MARKET,
        JUNEOUT,
        JUNEIN
    }

    [System.Serializable]
    public class NotebookEntry
    {
        public string name;
        public string text;
        public NoteGroup noteGroup;
        public bool isVisible;
        public bool isComplete;
        public bool isObjective;

        public void SetVisible(bool setting)
        {
            isVisible = setting;
        }
        public void SetComplete()
        {
            isComplete = true;
        }
    }
    public List<NotebookEntry> entries;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [YarnCommand("takenote")]
    public void TakeNote(string[] param)
    {
        foreach(NotebookEntry n in entries)
        {
            if (n.name == param[0])
            {
                n.SetVisible(true);
                Debug.Log(n.name + " is now visible");
                return;
            }
        }
    }
}
