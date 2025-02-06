using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvoTracker : MonoBehaviour
{
    static TextConversation[] convos;

    private void Awake()
    {
        convos = GetComponentsInChildren<TextConversation>(true);
        Debug.Log("Convos: " + convos.Length);
    }

    public static TextConversation GetConvo(string id)
    {
        //Debug.Log($"looking for id {id}");
        foreach (TextConversation g in convos)
        {
            if (g.name.Equals(id))
            {
                return g;
            }
        }
        return null;
    }
}
