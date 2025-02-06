using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToDoManager : MonoBehaviour
{
    [SerializeField]
    ToDoItem[] items;

    public void Show(string itemName)
    {
        foreach (ToDoItem item in items)
        {
            if (item.name.ToLower() == itemName)
            {
                item.gameObject.SetActive(true);
                return;
            }
        }
        Debug.LogWarning($"No item with name {itemName} found");
    }
    public void Check(string itemName, bool setting = true)
    {
        foreach (ToDoItem item in items)
        {
            if (item.name.ToLower() == itemName)
            {
                item.Check(setting);
                return;
            }
        }
        Debug.LogWarning($"No item with name {itemName} found");
    }
}
