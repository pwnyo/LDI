using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Yarn.Unity;

public class CameraManager : MonoBehaviour
{
    //First camera is always focused on the player
    [Header("Camera Switch")]
    public CinemachineVirtualCamera[] cams;
    public List<Transform> subjects;
    int subjectIndex = 0;
    public CinemachineBrain brain;
    //TODO: use it or lose it
    public CinemachineBlendDefinition ease, hard, cut;

    //Borrowed from https://github.com/Lumidi/CameraShakeInCinemachine/blob/master/SimpleCameraShakeInCinemachine.cs
    private float shakeDuration;          // Time the Camera Shake effect will last
    private float shakeAmplitude;         // Cinemachine Noise Profile Parameter
    private float shakeFrequency;         // Cinemachine Noise Profile Parameter

    // Cinemachine Shake
    private CinemachineVirtualCamera shakeVcam;
    private CinemachineBasicMultiChannelPerlin shakeVcamNoise;

    // Start is called before the first frame update
    void Start()
    {
        ResetCameras();
    }

    // Update is called once per frame
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            
        }
    }*/
    public void Add(Transform sj)
    {
        subjects.Add(sj);
    }
    void ResetCameras()
    {
        foreach (Cinemachine.CinemachineVirtualCamera c in cams)
        {
            c.gameObject.SetActive(false);
        }
        if (subjectIndex < cams.Length)
        {
            cams[subjectIndex].gameObject.SetActive(true);
            shakeVcam = cams[subjectIndex];
            shakeVcamNoise = shakeVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        if (cams.Length < 2)
        {
            Debug.Log("Only 1 camera");
        }
    }
    [YarnCommand("cameratarget")]
    public void SetCameraFocus(string[] param)
    {
        foreach (Transform t in subjects)
        {
            if (t.name == param[0])
            {
                if (param.Length < 2 || bool.Parse(param[1]))
                {
                    SwapCamera();
                }
                cams[subjectIndex].Follow = t;
                return;
            }
        }
        Debug.Log("Camera target " + param.ToString() + " not found");
    }
    [YarnCommand("cameraswap")]
    public void SwapCamera()
    {
        ResetCameraShake();
        NextCamIndex();
        foreach (Cinemachine.CinemachineVirtualCamera c in cams)
        {
            c.gameObject.SetActive(false);
        }
        cams[subjectIndex].gameObject.SetActive(true);
        shakeVcam = cams[subjectIndex];
        shakeVcamNoise = shakeVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        StartCoroutine(WaitForCamSwap());
    }
    void NextCamIndex()
    {
        subjectIndex++;
        if (subjectIndex >= cams.Length)
        {
            subjectIndex = 0;
        }
    }
    IEnumerator WaitForCamSwap()
    {
        float blendDur = brain.m_DefaultBlend.BlendTime;
        GameManager.Instance.inTransition = true;
        yield return new WaitForSeconds(blendDur);
        GameManager.Instance.inTransition = false;
    }

    [YarnCommand("camerashake")]
    public void Shake(string[] param)
    {
        if (param.Length != 3)
        {
            Debug.Log("Not enough camera shake parameters");
        }
        float.TryParse(param[0], out float dur);
        float.TryParse(param[1], out float amp);
        float.TryParse(param[2], out float freq);
        Shake(dur, amp, freq);
    }
    public void Shake(float dur, float amp, float freq)
    {
        Debug.Log("shaking camera");
        shakeDuration = dur;
        shakeAmplitude = amp;
        shakeFrequency = freq;

        StopCoroutine(ShakeCamera());
        StartCoroutine(ShakeCamera());
    }

    public IEnumerator ShakeCamera()
    {
        shakeVcamNoise.m_AmplitudeGain = shakeAmplitude;
        shakeVcamNoise.m_FrequencyGain = shakeFrequency;
        yield return new WaitForSeconds(shakeDuration);
        ResetCameraShake();
    }
    void ResetCameraShake()
    {
        if (shakeVcamNoise)
        {
            shakeVcamNoise.m_AmplitudeGain = 0f;
        }
    }
}
