using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyEvent<T>
{
    private Dictionary<T, System.Action> subscribers;
    public EasyEvent()
    {
        subscribers = new Dictionary<T, System.Action>();
    }
    public void Invoke()
    {
        foreach (KeyValuePair<T, System.Action> kvp in subscribers)
        {
            kvp.Value.Invoke();
        }
    }
    public void Subscribe(T t, System.Action callback)
    {
        if (!subscribers.ContainsKey(t))
            subscribers.Add(t, callback);
    }
    public void Unsubscribe(T t, bool all = false)
    {
        foreach (KeyValuePair<T, System.Action> kvp in subscribers)
        {
            if (kvp.Key.Equals(t))
            {
                subscribers.Remove(t);
                if (!all)
                    return;
            }
        }
    }
    public void Clear()
    {
        subscribers.Clear();
    }
}

