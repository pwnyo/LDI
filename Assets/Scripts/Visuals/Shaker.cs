using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    [SerializeField]
    float shakeDur, shakeIntensity;
    [SerializeField]
    int shakeCount;
    [SerializeField]
    Ease easeType;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Shake();
        }
    }

    public void Shake()
    {
        Tweener tweener = transform.DOShakePosition(shakeDur, shakeIntensity, shakeCount, 90, false, true);
        tweener.SetEase(easeType);
        //tweener.Pause();
        tweener.SetAutoKill(true);
    }
}
