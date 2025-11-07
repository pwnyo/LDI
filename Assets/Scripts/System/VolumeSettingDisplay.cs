using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingDisplay : MonoBehaviour
{
    [SerializeField]
    string paramName;
    [SerializeField]
    TextMeshProUGUI tmp;
    [SerializeField]
    Button downButton, upButton;

    [SerializeField]
    int currentLevel;

    [SerializeField]
    AudioSource testAudioSource;
    [SerializeField]
    AudioClip testAudioClip;

    private void Start()
    {
        if (PlayerPrefs.HasKey(paramName))
        {
            currentLevel = PlayerPrefs.GetInt(paramName);
        }
        UpdateVolumeDisplay();
    }

    public void UpVolumeLevel()
    {
        currentLevel++;
        UpdateVolumeDisplay();
        PlayTestSound();
    }
    public void DownVolumeLevel()
    {
        currentLevel--;
        UpdateVolumeDisplay();
        PlayTestSound();
    }
    void UpdateVolumeDisplay()
    {
        currentLevel = Mathf.Clamp(currentLevel, 0, 10);
        Debug.Log($"current level: {currentLevel}");
        PlayerPrefs.SetFloat(paramName, currentLevel);

        tmp.text = currentLevel.ToString();
        downButton.interactable = !IsAtLimit(false);
        upButton.interactable = !IsAtLimit(true);

        AudioManager.Instance.AdjustVolumeLevel(paramName, currentLevel);
    }
    bool IsAtLimit(bool upper)
    {
        return upper ? currentLevel == 12 : currentLevel == 0;
    }
    void PlayTestSound()
    {
        if (!testAudioSource || !testAudioClip)
        {
            Debug.LogWarning("Missing audio source or clip!");
            return;
        }
        testAudioSource.PlayOneShot(testAudioClip);
    }
}
