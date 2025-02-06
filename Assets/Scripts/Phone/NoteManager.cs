using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class NoteManager : AppManager
{
    public NoteDisplay notePrefab;
    public Transform noteParent;
    public Note[] notes;
    private List<NoteDisplay> displays = new List<NoteDisplay>();

    public NoteDisplay expansion;
    public GameObject otherNotes;
    public ToDoManager toDoList;
    bool showingToDo;

    // Start is called before the first frame update
    void Start()
    {
        ShowNote(notes[0], false);
    }

    public override void Back()
    {
        base.Back();
        if (expanded)
        {
            if (!showingToDo)
                UnexpandNote();
            else
                ShowToDo(false);
        }
        else
        {
            if (PhoneManager.Instance.phoneApp != PhoneManager.PhoneApp.HOME)
                PhoneManager.Instance.OpenApp("home");
        }
    }
    public void ShowToDo(bool setting)
    {
        expanded = setting;
        showingToDo = expanded;

        otherNotes.SetActive(!expanded);
        toDoList.gameObject.SetActive(expanded);
        expansion.gameObject.SetActive(false);
    }
    public void ExpandNote(Note n)
    {
        expanded = true;

        otherNotes.SetActive(false);
        expansion.Show(n);
        expansion.gameObject.SetActive(true);
    }
    public void UnexpandNote()
    {
        expanded = false;

        expansion.gameObject.SetActive(false);
        otherNotes.SetActive(true);
    }
    [YarnCommand("shownote")]
    public void ShowNote(string header)
    {
        Debug.Log("trying to show");
        foreach (Note n in notes)
        {
            if (!n.used && header == n.header)
            {
                ShowNote(n);
                return;
            }
        }
    }
    public void ShowNote(Note n, bool useNotif = true)
    {
        int index = FindNoteDisplayIndex(n.header);
        if (index < 0)
        {
            n.used = true;

            NoteDisplay d = Instantiate(notePrefab, noteParent);
            d.Show(n);
            d.gameObject.SetActive(true);

            displays.Add(d);
        }
        else
        {
            displays[index].Replace(n.content);
        }
        
        if (useNotif)
        {
            notif.Show(1.5f);
        }
    }
    [YarnCommand("addtodo")]
    public void AddToDo(string s)
    {
        toDoList.Show(s);
    }
    [YarnCommand("checktodo")]
    public void CheckToDo(string s)
    {
        toDoList.Check(s, true);
    }
    [YarnCommand("unchecktodo")]
    public void UncheckToDo(string s)
    {
        toDoList.Check(s, false);
    }
    int FindNoteDisplayIndex(string header)
    {
        for (int i = 0; i < displays.Count; i++)
        {
            if (header == displays[i].note.header)
            {
                return i;
            }
        }
        return -1;
    }
}
