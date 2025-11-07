using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DreamText : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    TextMeshProUGUI textUI;
    [SerializeField]
    RectTransform rt;
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    Light2D textLight;
    [SerializeField]
    Camera cam;

    [Header("Attributes")]
    [SerializeField]
    string text;
    [SerializeField]
    float distanceToShow, distanceToFull;
    float originalCanvasGroupAlpha, originalLightIntensity;

    [Header("Effects")]
    [SerializeField]
    ParticleSystem ps;
    [SerializeField]
    float shakeDur, shakeIntensity;
    [SerializeField]
    int shakeCount;
    [SerializeField]
    Ease easeType;

    // Start is called before the first frame update
    void Start()
    {
        originalCanvasGroupAlpha = canvasGroup.alpha;
        originalLightIntensity = textLight.intensity;
        SetTransparency(0);
        SetText();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = GetDistanceFromPlayer();
        if (distance < distanceToFull)
        {
            SetTransparency(1);
        }
        else if (distance < distanceToShow)
        {
            SetTransparency(Mathf.Lerp(1, 0, (distance - distanceToFull) / 
                (distanceToShow - distanceToFull)));
        }
        else
        {
            SetTransparency(0);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Shake();
        }
    }
    float GetDistanceFromPlayer()
    {
        //x only
        float currentX = transform.position.x;
        float distance = Mathf.Abs(currentX - PlayerControl.Instance.transform.position.x);
        return distance;
    }
    void SetText()
    {
        textUI.text = text;
    }
    void SetTransparency(float t)
    {
        canvasGroup.alpha = Mathf.Lerp(0, originalCanvasGroupAlpha, t);
        textLight.intensity = Mathf.Lerp(0, originalLightIntensity, t);
    }
    void Shake()
    {
        Tweener tweener = transform.DOShakePosition(shakeDur, shakeIntensity, shakeCount, 90, false, true);
        tweener.SetEase(easeType);
        tweener.Pause();
        tweener.SetAutoKill(true);
    }
}
