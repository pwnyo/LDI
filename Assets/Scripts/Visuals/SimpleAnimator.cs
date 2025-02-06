using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimator : MonoBehaviour
{
    public Animator anim;
    public SpriteRenderer sr;
    public Transform player;
    public string rIdle, lIdle;

    // Start is called before the first frame update
    void Start()
    {
        anim.Play(rIdle);
    }

    // Update is called once per frame
    void Update()
    {
        bool lookLeft = transform.position.x > player.position.x;
        if (lIdle.Length == 0)
        {
            sr.flipX = lookLeft;
        }
        else
        {
            anim.Play(lookLeft ? lIdle : rIdle);
        }
    }
}
