using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;
using DG.Tweening;

public class DebugHelper : MonoBehaviour
{
    public TimeManager timeManager;
    public LevelLoader levelLoader;
    public Clock clock;
    public TextMeshProUGUI dayText;
    public GameObject dayUI;
    public GameObject varUI;
    public GameObject nodeUI;
    public Button firstButton;
    bool dayIsVisible;
    bool varIsVisible;
    public float debugMiscVal;
    public Ease debugMiscEase;

    public Button[] buttons;
    private bool isVisible;
    private bool isOpen;

    [Header("Debug Vars")]
    [SerializeField]
    private float timeMultiplier = 1;
    [SerializeField]
    private float quickHoldMultiplier = 1;

    private void Start()
    {
        SetVisible(GameManager.Instance.debugMode);
        FindHelpers();
    }
    void FindHelpers()
    {
        if (levelLoader == null)
        {
            levelLoader = GameObject.FindObjectOfType<LevelLoader>();
        }
        if (timeManager == null)
        {
            timeManager = GameObject.FindObjectOfType<TimeManager>();
        }
    }
    private void Update()
    {
        Keyboard k = Keyboard.current;
        if (k.ctrlKey.isPressed && k.tabKey.wasReleasedThisFrame)
        {
            SetVisible(!isVisible);
        }
    }
    #region UI Helpers
    void SetVisible(bool setting)
    {
        isVisible = setting;
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(isVisible);
        }
        GameManager.Instance.debugMode = isVisible;
        isOpen = setting;
        nodeUI.SetActive(setting);
        ToggleDayUI(false);
        ToggleVarUI(false);
    }
    public void Toggle()
    {
        isOpen = !isOpen;
        nodeUI.SetActive(isOpen);
        if (!isOpen)
        {
            ToggleDayUI(false);
            ToggleVarUI(false);
        }
    }
    void ToggleDayUI(bool setting)
    {
        dayIsVisible = setting;
        dayUI.SetActive(dayIsVisible);
    }
    public void ToggleDayUI()
    {
        dayIsVisible = !dayIsVisible;
        ToggleDayUI(dayIsVisible);
    }
    public void UpdateDay()
    {
        string s = "Day: " + GameManager.Instance.CurrentDay + ", Time of Day: " + GameManager.Instance.CurrentTimeOfDay;
        s += "\nIs Busy? " + GameManager.Instance.IsOpen();
        dayText.text = s;
    }
    void ToggleVarUI(bool setting)
    {
        varIsVisible = setting;
        varUI.SetActive(setting);
    }
    public void ToggleVarUI()
    {
        varIsVisible = !varIsVisible;
        varUI.SetActive(varIsVisible);
    }
    #endregion

    #region Game Helpers
    public void PlayNode(string nodeName)
    {
        DialogueRunner dr = GameDialogueManager.Instance.dr;
        dr.StartDialogue(nodeName);
        Toggle();
    }
    public void SetDay(int day)
    {
        FindHelpers();
        timeManager.SetDay(day.ToString());
    }
    public void SetTimeOfDay(string param)
    {
        FindHelpers();
        timeManager.SetTimeOfDay(param);
    }
    public void SetLocation(string scene)
    {
        FindHelpers();
        LevelLoader.Instance.GoTo(scene);
    }
    public void PlayDayNode(int day)
    {
        FindHelpers();
        timeManager.StartDay(day.ToString());
    }
    public void PlayDay0Dinner()
    {
        timeManager.SetDayAndTimeOfDay(0, GameManager.TimeOfDay.DINNER);
        clock.SetTimeAndStart("20:30", "22:00", 5);
        levelLoader.GoTo("HomeLower", true);
    }
    public void PlayDay0Bedtime()
    {
        timeManager.SetDayAndTimeOfDay(0, GameManager.TimeOfDay.BEDTIME);
        clock.SetTimeAndStart("22:00", "23:00", 0);
        levelLoader.GoTo("EddyRoom", true);
    }
    public void PlayDay1Dinner()
    {
        timeManager.SetDayAndTimeOfDay(0, GameManager.TimeOfDay.DINNER);
        clock.SetTimeAndStart("20:30", "22:00", 5);
        levelLoader.GoTo("HomeLower", true);
    }
    #endregion

    #region Debug Vars
    public float TimeMultiplier()
    {
        return GameManager.Instance.debugMode ? timeMultiplier : 1f;
    }
    public float QuickHoldMultiplier()
    {
        return GameManager.Instance.debugMode ? quickHoldMultiplier : 1f;
    }
    #endregion
}
