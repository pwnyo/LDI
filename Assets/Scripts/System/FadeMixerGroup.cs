using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public static class FadeMixerGroup
{
    //TODO: check this out later; added because the audio is too dang loud
    const float baseVolumeScale = 0.7f;
    public static IEnumerator StartFadeMix(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume * baseVolumeScale, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }
    public static IEnumerator StartFade(AudioSource audioSource, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetVolume * baseVolumeScale, currentTime / duration);
            audioSource.volume = newVol;
            yield return null;
        }
        yield break;
    }
}