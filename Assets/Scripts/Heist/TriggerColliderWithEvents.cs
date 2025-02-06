using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerColliderWithEvents : MonoBehaviour
{
    PolygonCollider2D col;
    public UnityEvent enterEvents, exitEvents;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<PolygonCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enterEvents.Invoke();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        exitEvents.Invoke();
    }
}
