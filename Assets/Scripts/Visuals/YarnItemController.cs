using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Unity;

public class YarnItemController : MonoBehaviour
{
    public static YarnItemController Instance { get; private set; }
    private StringBuilder sb;
    private List<YarnItem> items = new List<YarnItem>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            sb = new StringBuilder();
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    public void Add(YarnItem y)
    {
        items.Add(y);
        sb.Append($"{y.id}|");
    }
    public void Remove(YarnItem y)
    {
        items.Remove(y);
    }
    [YarnCommand("yarnon")]
    public void Show(string itemName)
    {
        Debug.Log("show?");
        foreach (YarnItem y in items)
        {
            if (y != null && y.id == itemName)
            {
                y.Show(true);
                Debug.Log("showing");
            }
        }
    }
    [YarnCommand("yarnoff")]
    public void Hide(string itemName)
    {
        Debug.Log("hide?");
        foreach (YarnItem y in items)
        {
            if (y != null && y.id == itemName)
            {
                y.Show(false);
                Debug.Log("hiding");
            }
        }
    }
    [YarnCommand("yarnanim")]
    public void Animate(string[] param)
    {
        string id = param[0];
        string state = param[1];
        Debug.Log($"animate? {state}");
        foreach (YarnItem y in items)
        {
            if (y != null && y.id == id)
            {
                y.Show(true);
                y.Animate(state);
                Debug.Log("animating");
            }
        }
    }
    [YarnCommand("yarnsprite")]
    public void SetSprite(string[] param)
    {
        string itemName = param[0];
        string spriteName = param[1];
        foreach (YarnItem y in items)
        {
            if (y != null && y.id == itemName)
            {
                y.ShowSprite(spriteName);
                Debug.Log("spriting");
            }
        }
    }
    [YarnCommand("yarnenable")]
    public void EnableSprite(string[] param)
    {
        string itemName = param[0];
        string setting = param[1];
        foreach (YarnItem y in items)
        {
            if (y != null && y.id == itemName)
            {
                y.EnableSprite(setting == "true");
                Debug.Log("spriting");
            }
        }
    }
    [YarnCommand("yarnlight")]
    public void SetLightSwitch(bool on)
    {
        foreach (YarnItem y in items)
        {
            if (y != null && y is YarnLight light)
            {
                light.SetLights(on);
            }
        }
    }
}
