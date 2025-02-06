using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Handles second-to-second time tracking and events
/// </summary>
public class Clock : MonoBehaviour
{
    [System.Serializable]
    public class ClockTime : IEqualityComparer<ClockTime>
    {
        [Range(0, 23)]
        public int hour;
        [Range(0, 59)]
        public int minute;

        public ClockTime(int hour, int minute = 0)
        {
            this.hour = hour;
            this.minute = minute;
        }
        public ClockTime()
        {
            hour = 0;
            minute = 0;
        }

        public void SetTime(ClockTime other)
        {
            int h = other.hour;
            int m = other.minute;
            SetTime(h, m);
        }
        public void SetTime(int h, int m = 0)
        {
            if (h >= 0 && h <= 23)
            {
                hour = h;
            }
            else
            {
                Debug.Log("Clock not set: Out of range.");
            }
            if (m >= 0 && m <= 59)
            {
                minute = m;
            }
            else
            {
                minute = 0;
            }
        }
        public void AddMinute()
        {
            minute++;
            if (minute >= 60)
            {
                minute = 0;
                hour++;
                if (hour >= 24)
                {
                    hour = 0;
                }
            }
        }
        public void AddMinute(int add)
        {
            if (add >= 60)
            {
                return;
            }
            minute += add;
            if (minute >= 60)
            {
                if (minute % 10 != 0)
                {
                    minute = minute % 10;
                }
                else
                {
                    minute = 0;
                }
                hour++;
                if (hour >= 24)
                {
                    hour = 0;
                }
            }
        }
        public string GetString(bool in24 = true)
        {
            if (in24)
            {
                return hour + ":" + string.Format("{0:00}", minute);
            }
            else
            {
                if (hour % 12 == 0)
                {
                    return 12 + ":" + string.Format("{0:00}", minute);
                }
                else
                {
                    return (hour % 12) + ":" + string.Format("{0:00}", minute);
                }
            }
        }
        public int GetInt()
        {
            return (hour * 100) + minute;
        }
        bool IEqualityComparer<ClockTime>.Equals(ClockTime x, ClockTime y)
        {
            if (x.hour != y.hour || x.minute != y.minute)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        int IEqualityComparer<ClockTime>.GetHashCode(ClockTime obj)
        {
            return GetInt();
        }
    }

    public TextMeshProUGUI timeText;

    [SerializeField]
    bool isTicking;
    ClockTime currentTime;
    public ClockTime startTime;
    public ClockTime stopTime;
    public bool ignoreGameTime;
    public bool useSecondaryTime;
    float secondsPerMinute;
    float elapsedTime = 0;
    float secondaryElapsedTime = 0;
    bool showColon = true;

    public List<TimedEvent> timedEvents;
    public DialogueRunner dr;
    public DebugHelper debugHelper;

    // Start is called before the first frame update
    void Start()
    {
        secondsPerMinute = GameManager.Instance.secondsPerMinute;
        if (!ignoreGameTime && GameManager.Instance.CurrentTime != null)
        {
            startTime = GameManager.Instance.CurrentTime;
        }
        else
        {
            GameManager.Instance.SetTime(startTime);
        }
        currentTime = startTime;
        timeText.text = currentTime.GetString(false);

        isTicking = secondsPerMinute > 0;
        
        /*
        if (isTicking)
        {
            Debug.Log("ticking true");
            StartCoroutine(StartTicking());
        }*/
    }
    private void Update()
    {
        if (!PhoneManager.Instance.IsHidden() && !PhoneManager.Instance.IsInApp() && isTicking && !GameManager.Instance.inConvo)
        {
            elapsedTime += Time.deltaTime * debugHelper.TimeMultiplier();
            if (elapsedTime > GameManager.Instance.secondsPerMinute)
            {
                Tick();
                elapsedTime = 0;
            }

            if (useSecondaryTime)
            {
                secondaryElapsedTime += Time.deltaTime;
                if (secondaryElapsedTime >= 1f)
                {
                    showColon = !showColon;
                    secondaryElapsedTime = 0;
                    if (showColon)
                        timeText.text = timeText.text.Replace(':', ' ');
                    else
                        timeText.text = timeText.text.Replace(' ', ':');
                }
            }
        }
    }
    public void SetState(bool isOn)
    {
        isTicking = isOn;
    }
    void Tick(int ticks = 1)
    {
        currentTime.AddMinute(ticks);
        timeText.text = currentTime.GetString(false);
        if (currentTime.GetInt() == stopTime.GetInt())
        {
            isTicking = false;
            Debug.Log("Clock stopped!");
        }
        CheckClockEvents();
    }
    void CheckClockEvents()
    {
        if (GameManager.Instance.IsOpen() && timedEvents.Count > 0)
        {
            //Debug.Log("looking through events");
            foreach (TimedEvent te in timedEvents)
            {
                if (te.IsTime(currentTime) && te.IsDay(GameManager.Instance.CurrentDay, GameManager.Instance.CurrentTimeOfDay))
                {
                    te.TriggerEvent();
                }
            }
        }
    }
    public void SetTimeAndStart(string startTime, string endTime, int secondsPerMinute = -1)
    {
        SetClockStart(startTime);
        SetClockEnd(endTime);
        if (secondsPerMinute > -1)
        {
            SetSecondsPerMinute(secondsPerMinute);
        }
        StartClock();
    }
    [YarnCommand("clockspan")]
    public void SetClockSpan(string dayStart, string dayEnd)
    {
        SetClockStart(dayStart);
        SetClockEnd(dayEnd);
    }
    [YarnCommand("clockset")]
    public void SetClockStart(string param)
    {
        Debug.Log($"setting time to {param}");
        string[] time = param.Split(':');
        if (time.Length == 2)
        {
            int.TryParse(time[0], out int hour);
            int.TryParse(time[1], out int minute);
            SetTime(hour, minute);
        }
        else if (time.Length == 1)
        {
            int.TryParse(time[0], out int hour);
            SetTime(hour);
        }
    }
    [YarnCommand("clockend")]
    public void SetClockEnd(string param)
    {
        string[] time = param.Split(':');
        if (time.Length == 2)
        {
            int.TryParse(time[0], out int hour);
            int.TryParse(time[1], out int minute);
            SetEndTime(hour, minute);
        }
        else if (time.Length == 1)
        {
            int.TryParse(time[0], out int hour);
            SetEndTime(hour);
        }
    }
    [YarnCommand("clockstart")]
    public void StartClock()
    {
        SetTicking(true);
    }
    [YarnCommand("clockstop")]
    public void StopClock()
    {
        SetTicking(false);
    }
    [YarnCommand("clockscale")]
    public void SetSecondsPerMinute(string param)
    {
        int.TryParse(param, out int scaledSPM);
        Debug.Log($"scaling clock to {scaledSPM}");
        SetSecondsPerMinute(scaledSPM);
        if (scaledSPM == 0)
        {
            StopClock();
            PhoneManager.Instance.Hide();
        }
        else
        {
            StartClock();
            PhoneManager.Instance.Unhide();
        }
    }
    void SetSecondsPerMinute(int secondsPerMinute)
    {
        GameManager.Instance.secondsPerMinute = secondsPerMinute;
    }
    public void SetTime(string param)
    {
        string[] time = param.Split(':');
        if (time.Length == 2)
        {
            int.TryParse(time[0], out int hour);
            int.TryParse(time[1], out int minute);
            SetTime(hour, minute);
        }
        else if (time.Length == 1)
        {
            int.TryParse(time[0], out int hour);
            SetTime(hour);
        }
    }
    public void SetTime(int hour, int minute = 0)
    {
        if (currentTime == null)
        {
            currentTime = startTime;
        }
        currentTime.SetTime(hour, minute);
        timeText.text = currentTime.GetString(false);
    }
    public void SetEndTime(int hour, int minute = 0)
    {
        stopTime.SetTime(hour, minute);
    }
    public void SetTicking(string param)
    {
        bool.TryParse(param, out bool setting);
        SetTicking(setting);
    }
    public void SetTicking(bool setting)
    {
        isTicking = setting;
        Debug.Log($"set ticking to {setting}");
    }
}
