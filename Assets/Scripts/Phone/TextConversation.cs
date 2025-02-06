using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextConversation : MonoBehaviour
{
    public Transform parent;
    public TextMessage textPrefab;

    public void Add(Message m)
    {
        TextMessage g = Instantiate(textPrefab, parent);
        g.Push(m);
    }
    public TextMessage GetBottom()
    {
        if (transform.childCount < 2 || transform.GetChild(1).childCount < 1 ||
            transform.GetChild(1).GetChild(0).childCount < transform.childCount) 
            return null;
        Transform grandChild = transform.GetChild(1).GetChild(0);

        return grandChild.GetChild(transform.childCount).GetComponent<TextMessage>();
    }
    public List<Button> GetButtons()
    {
        List<Button> list = new List<Button>();
        foreach (Transform t in this.transform)
        {
            Button b = t.GetComponent<Button>();
            if (b)
                list.Add(b);
        }
        return list;
    }
}
