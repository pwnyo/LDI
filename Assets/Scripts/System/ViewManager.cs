using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class ViewManager : MonoBehaviour
{
    [Header("Fades and Transitions")]
    public PlayerControl pc;
    public SpawnPoint[] spawnPoints;

    public Animator fadeAnimator;
    public float fadeInTime;
    public float fadeOutTime;
    public FadeState fadeState;
    public FadeDirection fadeDirection;
    bool ignoreNextSpawn;
    string queuedDialogue;

    [System.Serializable]
    public class SpawnPoint
    {
        public string sceneName;
        public Vector3 location;
        public bool facingLeft;
    }
    public enum FadeState
    {
        NONE,
        IN,
        OUT
    }
    public enum FadeDirection
    {
        BLACK,
        FULL,
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        TOPLEFT,
        TOPRIGHT,
        BOTLEFT,
        BOTRIGHT,
    }

    // Start is called before the first frame update
    void Start()
    {
        //FadeIn();
        //SpawnPlayer();
        Screen.SetResolution(1280, 960, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetSpawnPoint(string name = "")
    {
        //TODO: Problem for new game is right here
        if (name.Length > 0)
        {
            PlayerPrefs.SetString("PreviousScene", name);
            Debug.Log(PlayerPrefs.GetString("PreviousScene"));
        }
        else
        {
            PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
            Debug.Log("Setting previous scene to " + PlayerPrefs.GetString("PreviousScene"));
        }
    }
    [YarnCommand("setspawn")]
    public void SpawnPlayer(string spawnPointName)
    {
        //StopAllCoroutines();
        foreach (SpawnPoint s in spawnPoints)
        {
            if (s.sceneName == spawnPointName)
            {
                SetSpawnPoint(spawnPointName);
                Debug.Log("Found spawn point and ignoring next one: " + spawnPointName);
                //pc.Spawn(s.location, s.facingLeft);
                //ignoreNextSpawn = true;
                return;
            }
        }
    }
    void SpawnPlayer()
    {
        //Might need a default as well, in case you can enter from multiple scenes
        //(like from the supermarket/June's house to your house's front entrance)
        //and want to avoid duplicating the same stuff.
        string previousSceneName = PlayerPrefs.GetString("PreviousScene");
        if (previousSceneName == null || previousSceneName == "")
        {
            Debug.Log("No previous scene");
            return;
        }

        foreach (SpawnPoint s in spawnPoints)
        {
            if (s.sceneName == previousSceneName)
            {
                Debug.Log("Found previous spawn point " + previousSceneName);
                pc.Spawn(s.location, s.facingLeft);
                return;
            }
        }
    }

    public void ResetGame()
    {
        Debug.Log("Starting new game!");
        
    }
    public void StartGame()
    {
        //Does some kind of stuff
        PlayerPrefs.SetString("PreviousScene", "HomeUpper");
        FadeOut("EddyRoom");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void FadeContinue()
    {
        string temp = PlayerPrefs.GetString("PreviousScene");
        Debug.Log("Continuing from previous scene, " + temp);
        SetSpawnPoint();
        StartCoroutine(temp);
    }
    
    [YarnCommand("goto")]
    public void FadeOut(string param)
    {
        Debug.Log("trying to go to " + param);
        if (ignoreNextSpawn)
        {
            SetSpawnPoint();
            ignoreNextSpawn = false;
        }
        StartCoroutine(TransitionOut(param));
    }
    [YarnCommand("goto2")]
    public void FadeOut2(string param)
    {
        Debug.Log("trying to go to " + param);
        StartCoroutine(TransitionOut(param));
    }
    public void FadeIn()
    {
        SpawnPlayer();
        StartCoroutine(TransitionIn());
    }
    //[YarnCommand("toblack")]
    public void ToBlack()
    {
        fadeAnimator.SetInteger("Direction", (int)FadeDirection.BLACK);
    }
    //[YarnCommand("fadetoblack")]
    public void FadeToBlack(string[] param)
    {
        StartCoroutine(TransitionAndBack());
    }
    IEnumerator TransitionAndBack()
    {
        fadeAnimator.SetInteger("State", (int)FadeState.OUT);
        yield return new WaitForSeconds(fadeOutTime);
        fadeAnimator.SetInteger("State", (int)FadeState.IN);
        yield return new WaitForSeconds(fadeInTime);
    }
    IEnumerator TransitionOut(string sceneToLoad)
    {
        //Debug.Log("Fading out");
        if (pc)
        {
            pc.SetPlayerState(PlayerControl.PlayerState.BUSY);
        }
        fadeAnimator.SetInteger("State", (int)FadeState.OUT);
        yield return new WaitForSeconds(fadeOutTime);
        Debug.Log("Loading scene " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
    IEnumerator TransitionIn()
    {
        //Debug.Log("Fading in");
        if (pc)
        {
            pc.SetPlayerState(PlayerControl.PlayerState.BUSY);
        }
        fadeAnimator.SetInteger("State", (int)FadeState.IN);
        yield return new WaitForSeconds(fadeInTime);
        fadeAnimator.SetInteger("State", (int)FadeState.NONE);
        if (pc)
        {
            pc.SetPlayerState(PlayerControl.PlayerState.NONE);
        }
        FindObjectOfType<ObjectSchedule>().LoadDialogue();
        //Debug.Log("Faded in " + pc.state);
    }
    private void OnDrawGizmos()
    {
        if (spawnPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (SpawnPoint s in spawnPoints)
            {
                Gizmos.DrawWireSphere(s.location, .25f);
            }
        }
    }
}
