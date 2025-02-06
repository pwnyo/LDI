using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoteDisplay : MonoBehaviour
{
    public TextMeshProUGUI header, content;
    public Image box;
    public Note note;
    private NoteManager nm;

    private void Start()
    {
        nm = GetComponentInParent<NoteManager>();
    }

    public void Show(Note n)
    {
        note = n;
        header.text = n.header;
        content.text = n.content;
    }
    public void Replace(string s)
    {
        Debug.Log("replacing");
        content.text = s;
    }
    public void Recolor(Color c)
    {
        box.color = c;
    }
    public void Expand()
    {
        if (nm)
        {
            nm.ExpandNote(note);
        }
    }
}
