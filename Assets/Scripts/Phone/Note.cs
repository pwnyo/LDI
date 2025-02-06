using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public string header;
    [TextArea(2, 4)]
    public string content;
    public bool used = false;

    public Note()
    {

    }
    public Note(string h, string c)
    {
        header = h;
        content = c;
        used = false;
    }
}
