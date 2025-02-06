using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform other;
    bool start;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        GetMax(new int[] { 0, 1, 3, 4, 5, 2 });
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            timer += Time.deltaTime;
            if (timer > 5)
            {
                Debug.Break();
            }
        }
    }
    Vector3 AimVector()
    {
        return new Vector3(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), 0);
    }

    bool IsBehindMe(Transform other)
    {
        Vector3 pointer = transform.position - other.position;
        return (Mathf.Acos((transform.position.x - other.position.x) / AimVector().magnitude) >= Mathf.PI / 2);
    }

    int GetMax(int[] arr)
    {
        start = true;
        //Assuming an array of size 3 or more
        int minIndex = 0;
        int maxIndex = arr.Length - 1;
        int cLeft, cRight;

        while (minIndex < maxIndex)
        {
            cLeft = (minIndex + maxIndex) / 2;
            cRight = cLeft + 1;
            if (arr[cLeft] < arr[cRight])
                minIndex = cLeft;
            else if (arr[cLeft] > arr[cRight])
                maxIndex = cRight;
            else
                return -1;
        }
        return maxIndex;
    }
}
