using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToDoItem : MonoBehaviour
{
    [SerializeField]
    RectTransform rt;
    [SerializeField]
    Image check;
    [SerializeField]
    TextMeshProUGUI text;
    [TextArea(2, 3)]
    [SerializeField]
    string content;
    [SerializeField]
    bool complete;

    private void Start()
    {
        Redraw();
    }
    public void Check(bool setting)
    {
        complete = setting;
        Redraw();
    }
    public void SetText(string s)
    {
        content = s;
        Redraw();
    }
    public void Redraw()
    {
        check.gameObject.SetActive(complete);
        if (complete)
            text.text = $"<s>{content}</s>";
        else
        {
            text.text = content;
        }
        rt.sizeDelta = new Vector2(100, text.preferredHeight + 2.5f);
    }
}
