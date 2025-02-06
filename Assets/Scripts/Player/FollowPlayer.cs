using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public PlayerControl followControl;
    public Transform followObj;
    public float followDistance, speed;
    public SpriteRenderer sr;
    public Animator anim;
    public Sprite defSprite;
    public string walkState, idleState;
    float distance, move;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!followControl.CanAct())
        {
            anim.enabled = false;
            sr.sprite = defSprite;
            return;
        }
        else
        {
            anim.enabled = true;
        }
        
        distance = followObj.position.x - transform.position.x;
        if (Mathf.Abs(distance) > followDistance)
        {
            move = speed * Mathf.Sign(distance);
            anim.Play(walkState);
        }
        else
        {
            move = 0;
            anim.Play(idleState);
        }
        sr.flipX = distance < 0;
    }
    private void FixedUpdate()
    {
        transform.position += new Vector3(move, 0) * Time.deltaTime;
    }
}
