using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;


public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    public Animator wipe;
    public Animator cross;
    public PlayerControl pc;
    public Canvas canvas;
    public GameDialogueManager gdm;
    public float transitionTime;
    public string currentScene;
    public string currentSpawnPoint;
    [SerializeField]
    private TimeManager tm;
    private SpawnInfo spawnInfo;
    private bool ignoreNextSpawn;

    public ScenePoint[] scenes;

    [System.Serializable]
    public class ScenePoint
    {
        public string sceneName;
        public Scene scene;
    }

    [System.Serializable]
    public class SpawnPoint
    {
        public string sceneName;
        public Vector3 location;
        public bool facingLeft;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += ShowLevel;
        }
        else if (Instance != this)
            Destroy(this.gameObject);

    }
    void Start()
    {
        //ShowLevel();
    }
    public void ShowLevel() {
        if (GameDialogueManager.Instance.dr.enabled)
            ShowLevel(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    public void ShowLevel(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer();
        StartCoroutine(ShowScene());
    }
    IEnumerator ShowScene()
    {
        if (!tm)
            tm = FindObjectOfType<TimeManager>();
        GameManager.Instance.inTransition = true;
        GameDialogueManager.Instance.dr.variableStorage.SetValue("$currentscene", SceneManager.GetActiveScene().name);
        //Debug.Log("showing scene");
        //cross.SetInteger("Black", 1);
        if (tm)
        {
            tm.PlayPreTransition();
            tm.Begin();
        }
        yield return new WaitForSeconds(transitionTime);
        GameManager.Instance.inTransition = false;
    }

    SpawnPoint FindSpawnPoint(string name)
    {
        spawnInfo = FindObjectOfType<SpawnInfo>();
        if (spawnInfo)
        {
            return spawnInfo.FindSpawnPoint(name);
        }
        return null;
    }
    void SpawnPlayer()
    {
        if (!pc)
        {
            pc = FindObjectOfType<PlayerControl>();
        }
        SpawnPoint s = FindSpawnPoint(currentSpawnPoint);
        currentScene = SceneManager.GetActiveScene().name;
        if (pc)
        {
            pc.enabled = false;
            pc.enabled = true;
            if (s != null)
            {
                Debug.Log(s.sceneName);
                pc.Spawn(s.location, s.facingLeft);
            }
        }
    }
    [YarnCommand("ignorenextspawn")]
    public void IgnoreNextSpawn()
    {
        ignoreNextSpawn = true;
    }
    [YarnCommand("tor")]
    public IEnumerator GoCoroutine(string[] param)
    {
        if (param.Length == 2)
        {
            Debug.Log("'to' has 2 parameters");

            Debug.Log("going to scene " + param[0] + " at spawnpoint " + param[1]);
            currentSpawnPoint = param[1];
            yield return LoadCoroutine(param[0]);
        }
        else if (param.Length == 1)
        {
            currentSpawnPoint = SceneManager.GetActiveScene().name;
            Debug.Log("going to scene " + param[0]);
            yield return LoadCoroutine(param[0]);
        }
    }
    [YarnCommand("sortorder")]
    public void MoveLayer(int num)
    {
        canvas.sortingOrder = num;
    }
    [YarnCommand("to")]
    public void GoTo(string[] param)
    {
        if (param.Length == 2)
        {
            Debug.Log("'to' has 2 parameters");

            Debug.Log("going to scene " + param[0] + " at spawnpoint " + param[1]);
            currentSpawnPoint = param[1];
            LoadLevel(param[0]);
        }
        else if (param.Length == 1)
        {
            currentSpawnPoint = SceneManager.GetActiveScene().name;
            Debug.Log("going to scene " + param[0]);
            LoadLevel(param[0]);
        }
    }
    public void GoTo(string param, bool force = false)
    {
        currentSpawnPoint = SceneManager.GetActiveScene().name;
        LoadLevel(param, force);
    }
    [YarnCommand("spawn")]
    public void SpawnAt(string param)
    {
        SpawnPoint s = FindSpawnPoint(param);
        if (s != null)
        {
            PlayerControl.Instance.Spawn(s.location, s.facingLeft);
            Debug.Log("tospawn at " + s.sceneName);
        }
    }
    public void EnterExitBuilding(string param)
    {
        SpawnPoint s = FindSpawnPoint(param);
        if (s != null)
        {
            PlayerControl.Instance.Spawn(s.location, PlayerControl.Instance.spriteRenderer.flipX);
        }
    }
    [YarnCommand("toscene")]
    public void LoadLevel(string param, bool force = false)
    {
        if (SceneManager.GetActiveScene().name == param && !force)
        {
            Debug.Log("already at scene " + param);
            return;
        }
        Debug.Log("going to scene " + param);
        StopAllCoroutines();
        StartCoroutine(LoadScene(param));
        GameDialogueManager.Instance.dr.variableStorage.SetValue("$reload", true);
        GameDialogueManager.Instance.StopDialogue();
    }
    private IEnumerator LoadCoroutine(string param)
    {
        if (SceneManager.GetActiveScene().name == param)
        {
            Debug.Log("already at scene " + param);
            yield return null;
            yield break;
        }
        StopAllCoroutines();
        yield return LoadScene(param);
        GameDialogueManager.Instance.dr.variableStorage.SetValue("$reload", true);
    }
    IEnumerator LoadScene(string sceneName)
    {
        string currentTrack = AudioManager.Instance.GetTrackForScene();
        string nextTrack = AudioManager.Instance.GetTrackForScene(sceneName);

        wipe.SetBool("WipeIn", false);

        Debug.Log("waiting");
        PhoneManager.Instance.Unfocus();
        GameManager.Instance.inTransition = true;
        if (currentTrack != nextTrack && nextTrack != "")
        {
            AudioManager.Instance.FadeOutMusic(transitionTime);
        }
        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);

        if (currentTrack != nextTrack || !AudioManager.Instance.musicSource.isPlaying)
        {
            AudioManager.Instance.FadeInMusic("", transitionTime, 1);
        }
        GameDialogueManager.Instance.dr.variableStorage.SetValue("$currentscene", sceneName);
        wipe.SetBool("WipeIn", true);
        Debug.Log("wiped");
        GameManager.Instance.inTransition = false;
        yield return null;
    }
    [YarnCommand("setfade")]
    public void SetFade(string param)
    {
        StopAllCoroutines();
        int.TryParse(param, out int setting);
        StartCoroutine(DelayControls());
        cross.SetInteger("Black", setting);
    }
    IEnumerator DelayControls()
    {
        yield return new WaitForSeconds(transitionTime);
        GameManager.Instance.inTransition = false;
    }
    [YarnCommand("fromblack")]
    public void FadeFromBlack(string param)
    {
        StopAllCoroutines();
        float.TryParse(param, out float waitTime);

        StartCoroutine(IFadeFromBlack(waitTime));
    }
    IEnumerator IFadeFromBlack(float waitTime)
    {
        GameManager.Instance.inTransition = true;
        cross.SetInteger("Black", 0);
        yield return new WaitForSeconds(waitTime);
        GameManager.Instance.inTransition = false;
        cross.SetInteger("Black", -1);
    }
    [YarnCommand("cross")]
    public void CrossFade(string param)
    {
        float.TryParse(param, out float waitTime);
        StartCoroutine(ICrossFade(waitTime));
    }
    IEnumerator ICrossFade(float waitTime)
    {
        cross.SetInteger("Black", 1);
        yield return new WaitForSeconds(waitTime);
        cross.SetInteger("Black", -1);
    }
}