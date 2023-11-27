﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Dictionary<string, Yarn.Value> variables = new Dictionary<string, Yarn.Value>();

    public enum TimeOfDay
    {
        MORNING,
        AFTERNOON,
        FREETIME,
        EVENING,
        DINNER,
        NIGHT,
        BEDTIME,
        BETWEEN,
    }

    public VariableManager.DefaultVariable[] defaultVariables;
    public bool isBusy;
    public bool inConvo;
    public int CurrentDay { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public Clock.ClockTime CurrentTime { get; private set; }
    public int secondsPerMinute;

    public bool doneEssay;
    public bool doneQuiz;
    public bool doneTrash;
    public bool doneRice;

    public int trashCollected;

    public bool debugMode;
    public bool enteringNodeName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    public void ResetGame()
    {
        variables.Clear();
        isBusy = false;
        inConvo = false;
        CurrentDay = 0;
        CurrentTimeOfDay = TimeOfDay.EVENING;
        //currentDay = 1;
        //currentTimeOfDay = TimeOfDay.MORNING;
        CurrentTime = new Clock.ClockTime(20, 30);
        secondsPerMinute = 1;

        doneEssay = false;
        doneQuiz = false;
        doneTrash = false;
        doneRice = false;
        trashCollected = 0;

        EssayGrader.Reset();

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
        CurrentDay = day;
    }
    public void SetDay(string param)
    {
        int.TryParse(param, out int day);
        if (day <= 4 && day >= 0)
        {
            CurrentDay = day;
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