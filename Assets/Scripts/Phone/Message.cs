using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Message : Note
{
    public Message()
    {
    }
    public Message(string h, string c, bool v = false)
    {
        header = h;
        content = c;
        isViet = v;
    }
    public bool isViet;
}
