using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Yarn.Unity;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioTrack
    {
        public string clipName;
        public AudioClip audioClip;
    }
    public static AudioManager Instance { get; private set; }
    public string startingClip; //if none, do nothing. if same as before, do nothing. otherwise, fade between the songs
    public float fadeInDuration;
    public float fadeInVolume;
    public float fadeOutDuration;

    public AudioTrack[] musicTracks;
    public AudioTrack[] sfxTracks;
    public AudioMixer audioMixer;
    public AudioMixerGroup audioMixerGroup;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    bool isFading;
    bool shouldAutoplay;

    private string prevClipName;
    private float prevClipTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
            Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        /*
        if (prevClipName == musicSource.clip.name)
        {
            musicSource.PlayDelayed(prevClipTime);
        }*/
    }
    public void EndClip()
    {
        prevClipName = musicSource.clip.name;
        prevClipTime = musicSource.time;
    }
    /*
    private void Awake()
    {
        Debug.Log("am");
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("destroying");
            return;
        }

        Debug.Log("not destroying");
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }*/
    public string GetTrackForScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return GetTrackForScene(sceneName);
    }
    public string GetTrackForScene(string sceneName)
    {
        switch (sceneName)
        {
            case ("EddyRoom"):
            case ("Bathroom"):
            case ("HomeUpper"):
            case ("HomeLower"):
                return "hideout";
            case ("HomeOutside"):
            case ("ChinatownOutside"):
                return "v4";
            case ("JuneOutside"):
            case ("JuneInside"):
            case ("JuneRoom"):
            default:
                return "";
        }
    }
    AudioClip FindMusicByString(string name)
    {
        foreach (AudioTrack a in musicTracks)
        {
            if (name == a.clipName)
            {
                return a.audioClip;
            }
        }
        return null;
    }
    AudioClip FindSfxByString(string name)
    {
        foreach (AudioTrack a in sfxTracks)
        {
            if (name == a.clipName)
            {
                return a.audioClip;
            }
        }
        return null;
    }
    [YarnCommand("sfxplay")]
    public void PlaySfx(string name)
    {
        Debug.Log("trying to play sfx");
        AudioClip a = FindSfxByString(name);
        if (a)
        {
            sfxSource.PlayOneShot(a);
        }
        else
        {

        }
    }
    [YarnCommand("musicplay")]
    public void PlayMusic(string name)
    {
        Debug.Log("trying to play music");
        AudioClip a = FindMusicByString(name);
        if (a)
        {
            musicSource.clip = a;
            musicSource.Play();
        }
        else
        {

        }
    }
    [YarnCommand("musicstop")]
    public void StopMusic()
    {
        musicSource.Stop();
    }
    [YarnCommand("musicin")]
    public void FadeInMusic(string[] param)
    {
        string name = param[0];
        float.TryParse(param[1], out float volume);
        float.TryParse(param[2], out float duration);
        if (!(volume >= 0 || volume <= 1) || duration < 0)
        {
            Debug.Log("Parameters not within bounds");
            return;
        }
        FadeInMusic(name, duration, volume);
    }
    public void FadeInMusic(string name, float duration, float volume)
    {
        if (name == null || name.Length < 1)
        {
            name = GetTrackForScene();
        }
        AudioClip a = FindMusicByString(name);
        if (a)
        {
            Debug.Log((a != null) + " " + a.name);
            if (musicSource.clip == a)
            {
                return;
            }
            musicSource.clip = a;
            musicSource.Play();
            //slowly raise the volume to desired level over duration of seconds
            Debug.Log($"fading in {name}: {duration}s, {volume} vol");
            StartCoroutine(FadeMixerGroup.StartFade(musicSource, "vol", duration, volume));
        }
    }
    [YarnCommand("musicout")]
    public void FadeOutMusic(string param)
    {
        float.TryParse(param, out float duration);

        //slowly lower the volume to 0 over duration of seconds
        FadeOutMusic(duration);
    }
    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeMixerGroup.StartFade(musicSource, "vol", duration, 0));
    }
}
