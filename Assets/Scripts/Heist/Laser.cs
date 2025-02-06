using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Laser : SecurityObject
{
    [Header("Unique")]
    public bool moves;
    public bool blinks;
    bool moveToward1;
    public float moveSpeed;
    public Vector3 endPoint1;
    public Vector3 endPoint2;
    public float upTime;
    public float downTime;
    public GameObject laser;
    bool isStopped;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (moves && !isStopped)
        {
            if (moveToward1)
            {
                transform.position = Vector3.MoveTowards(transform.position, endPoint1, Time.deltaTime * moveSpeed);
                //transform.position = Vector3.Lerp(endPoint2, endPoint1, Time.deltaTime * moveSpeed);
                if (Vector3.Distance(transform.position, endPoint1) < 0.1f)
                {
                    moveToward1 = !moveToward1;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, endPoint2, Time.deltaTime * moveSpeed);
                if (Vector3.Distance(transform.position, endPoint2) < 0.1f)
                {
                    moveToward1 = !moveToward1;
                }
            }
        }
    }

    protected override void ToggleOn(bool setting)
    {
        base.ToggleOn(setting);
        if (isOn)
        {
            StartCoroutine(BlinkLaser());
        }
        else
        {
            StopAllCoroutines();
        }
    }

    IEnumerator BlinkLaser()
    {
        while (blinks)
        {
            yield return new WaitForSeconds(upTime);
            if (downTime > 0)
            {
                ToggleLaser(false);
                yield return new WaitForSeconds(downTime);
                ToggleLaser(true);
            }
        }
    }
    void ToggleLaser(bool setting)
    {
        laser.SetActive(setting);
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(endPoint1, 0.1f);
        Gizmos.DrawWireSphere(endPoint2, 0.1f);
    }
}
