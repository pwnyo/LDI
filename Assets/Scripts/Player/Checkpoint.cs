using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    SecurityManager sm;

    // Start is called before the first frame update
    void Start()
    {
        sm = FindObjectOfType<SecurityManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        sm.SetSpawnPoint(transform.position);
    }
}
