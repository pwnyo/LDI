using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Dictionary<string, Yarn.Value> variables = new Dictionary<string, Yarn.Value>();

    public enum TimeOfDay
    {
        ALL,
        MORNING,
        AFTERNOON,
        FREETIME,
        EVENING,
        DINNER,
        NIGHT,
        BEDTIME,
        BETWEEN,
    }
    public enum ClosedStateReasons
    {
        OPEN,
        TRANSITION,
        CONVO,
        ESSAY,
        PHONE,
        MUSIC,
    }

    public VariableManager.DefaultVariable[] defaultVariables;
    public VariableStorageBehaviour vars;

    [Header("Game Vars")]
    public bool inConvo;
    public bool inEssay;
    public bool inPiano;
    public bool inTransition;
    public bool isPhoneFocused;
    public bool doneEssay;
    public bool doneQuiz;
    public bool doneTrash;
    public bool doneRice;
    public int trashCollected;

    private HashSet<ClosedStateReasons> closedStateReasons;

    public int CurrentDay { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public Clock.ClockTime CurrentTime { get; private set; }

    [Header("Debug Helpers")]
    public bool skipDialogue;
    public bool debugMode;
    public int secondsPerMinute;

    public bool enteringNodeName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResetGame();
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    private void Start()
    {
    }
    public void ResetGame()
    {
        Debug.Log("reset");
        variables.Clear();
        FreeControls();

        CurrentDay = 0;
        CurrentTimeOfDay = TimeOfDay.FREETIME;
        CurrentTime = new Clock.ClockTime(19, 30);
        secondsPerMinute = 60;

        doneEssay = false;
        doneQuiz = false;
        doneTrash = false;
        doneRice = false;
        trashCollected = 0;

        EssayGrader.Reset();
    }
    public void FreeControls()
    {
        inConvo = false;
        inEssay = false;
        inTransition = false;

        closedStateReasons = new HashSet<ClosedStateReasons>();
    }
    public bool IsOpen()
    {
        if (inConvo || inEssay || inTransition || inPiano)
        {
            return false;
        }
        return true;
    }
    public void AddClosedReason(ClosedStateReasons reason)
    {
        closedStateReasons.Add(reason);
    }
    public void RemoveClosedReason(ClosedStateReasons reason)
    {
        closedStateReasons.Remove(reason);
    }
    public void ResetToDefaults() {
        foreach (var variable in defaultVariables)
        {

            object value;

            switch (variable.type)
            {
                case Yarn.Value.Type.Number:
                    float f = 0.0f;
                    float.TryParse(variable.value, out f);
                    value = f;
                    break;

                case Yarn.Value.Type.String:
                    value = variable.value;
                    break;

                case Yarn.Value.Type.Bool:
                    bool b = false;
                    bool.TryParse(variable.value, out b);
                    value = b;
                    break;

                case Yarn.Value.Type.Variable:
                    // Doesn't support assigning default variables from other variables yet
                    Debug.LogErrorFormat("Can't set variable {0} to {1}: You can't " +
                        "set a default variable to be another variable, because it " +
                        "may not have been initialised yet.", variable.name, variable.value);
                    continue;

                case Yarn.Value.Type.Null:
                    value = null;
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            var v = new Yarn.Value(value);

            SetValue("$" + variable.name, v);
            //SetValue(variable.name, v);
        }
    }
    public void SetValue(string variableName, Yarn.Value value)
    {
        variables[variableName] = new Yarn.Value(value);
        //Debug.Log("Setting " + variableName + " " + value.AsString);
    }
    public Yarn.Value GetValue(string variableName)
    {
        //Debug.Log("trying to get value " + variableName);
        if (!variables.ContainsKey(variableName))
            return Yarn.Value.NULL;

        return variables[variableName];
    }
    public void SetDay(int day)
    {
        VariableStorageBehaviour var = GameDialogueManager.Instance.dr.variableStorage;
        CurrentDay = day;
        var.SetValue("$ricedone", false);
        if (!var.GetValue("$trashdone").AsBool)
        {
            var.SetValue("$trashdone", false);
        }
    }
    public void SetDay(string param)
    {
        int.TryParse(param, out int day);
        if (day <= 4 && day >= 0)
        {
            SetDay(day);
        }
    }
    public void SetTimeOfDay(TimeOfDay t)
    {
        CurrentTimeOfDay = t;
    }

    public void SetTime(Clock.ClockTime time)
    {
        CurrentTime = time;
    }

    public void SetTimeOfDay(string param)
    {
        TimeOfDay timeOfDay;
        switch (param)
        {
            case "morning":
                timeOfDay = TimeOfDay.MORNING;
                break;
            case "afternoon":
                timeOfDay = TimeOfDay.AFTERNOON;
                break;
            case "freetime":
                timeOfDay = TimeOfDay.FREETIME;
                break;
            case "evening":
                timeOfDay = TimeOfDay.EVENING;
                break;
            case "dinner":
                timeOfDay = TimeOfDay.DINNER;
                break;
            case "night":
                timeOfDay = TimeOfDay.NIGHT;
                break;
            case "bedtime":
                timeOfDay = TimeOfDay.BEDTIME;
                break;
            case "between":
                timeOfDay = TimeOfDay.BETWEEN;
                break;
            default:
                Debug.Log("No matching time of day found");
                return;
        }
        CurrentTimeOfDay = timeOfDay;
    }
    public string GetTimeOfDayString()
    {
        Debug.Log("gamemanager converting time of day to string " + CurrentTimeOfDay.ToString().ToLower());
        return CurrentTimeOfDay.ToString().ToLower();
    }
}
