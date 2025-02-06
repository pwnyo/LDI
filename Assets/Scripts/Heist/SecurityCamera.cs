using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Settings")]
    public bool isOn;
    public float threatLevel;
    public bool startsMovingRight;
    public bool singleDirection;
    public float courtesyTime;
    public float pivotTime;
    public float delayTime;
    public float waitTime;

    [Header("Visuals")]
    public SpriteSwap spriteSwap;
    public SpriteRenderer otherSr;
    public Transform toRotate;
    public FieldOfView fov;
    public TriggerColliderWithEvents col;
    public Color okColor;
    public Color noColor;

    protected float timeSeen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
