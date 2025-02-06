using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class SecurityManager : MonoBehaviour
{
    public static SecurityManager Instance { get; private set; }
    //Has an image or display to show the current security level.
    //void AddThreat, called when the player is left in a security camera's view for too long or gets touched by a laser.
    //void SubtractThreat, called when undetected for long enough.
    //void CatchPlayer, called when threat limit is surpassed. Triggers a short cutscene, then respawns the player at the previous checkpoint.
    int currentThreat;
    float tempThreat;
    public int maxThreat = 100;
    public float threatAnimDuration;
    public GameObject threatMeter;
    public Image threatBar1, threatBar2;
    public TextMeshProUGUI threatText;
    public float timeUntilRelax = 0.5f;
    public float relaxInterval = 0.2f;
    public int relaxAmount = 4;
    float timeRelaxed;

    public GameObject gameOverObj;
    public RectTransform gameOverMask;
    public string caughtNodeName;
    public Transform defaultCheckpoint;
    Vector3 currentCheckpoint;
    List<SecurityObject> security;
    DialogueRunner dr;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
        security = new List<SecurityObject>();
    }
    void Start()
    {
        currentCheckpoint = defaultCheckpoint.position;
        dr = GameDialogueManager.Instance.dr;
    }
    private void Update()
    {
        if (!PlayerControl.Instance.CanAct())
        {
            return;
        }
        threatText.text = $"{currentThreat}/{maxThreat}";
        if (security.Count == 0)
        {
            timeRelaxed += Time.deltaTime;
            if (timeRelaxed > relaxInterval)
            {
                LoseThreat();
                timeRelaxed = 0;
            }
        }
    }
    public bool HasSeen(SecurityObject obj)
    {
        return security.Contains(obj);
    }
    public void AddSeenBySecurity(SecurityObject obj)
    {
        AddThreat(obj.threatAmount);
        if (security.Contains(obj))
        {
            return;
        }
        security.Add(obj);
    }
    public void RemoveSeenBySecurity(SecurityObject obj)
    {
        if (!security.Contains(obj))
        {
            return;
        }
        security.Remove(obj);
    }
    public void AddThreat(int amount)
    {
        threatMeter.SetActive(true);
        StopAllCoroutines();
        currentThreat = Mathf.Min(maxThreat, currentThreat + amount);
        //Debug.Log("+" + amount);
        if (currentThreat > 0)
        {
            UpdateMeter();
            if (currentThreat >= maxThreat)
            {
                CatchPlayer();
            }
        }
    }
    public void LoseThreat()
    {
        currentThreat = Mathf.Max(0, currentThreat - relaxAmount);
        UpdateMeter();
        if (currentThreat <= 0)
        {
            threatMeter.SetActive(false);
        }
    }
    void UpdateMeter()
    {
        threatBar1.DOFillAmount(currentThreat / (float)maxThreat, threatAnimDuration);
        threatBar2.DOFillAmount(currentThreat / (float)maxThreat, threatAnimDuration);
    }
    public void CatchPlayer()
    {
        if (!PlayerControl.Instance.CanAct())
        {
            return;
        }
        dr.StartDialogue(caughtNodeName);
    }
    [YarnCommand("spawncaught")]
    public void SpawnCaughtPlayer()
    {

    }
    public void SetSpawnPoint(Vector3 spawnPoint)
    {
        currentCheckpoint = spawnPoint;
    }
}
