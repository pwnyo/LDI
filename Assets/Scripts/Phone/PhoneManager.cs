using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;

//TODO: Possibly rename this to focus on UIAnimationManager? That's all this does.
public class PhoneManager : AppManager, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static PhoneManager Instance { get; private set; }
    public bool IsReady { get; private set; }

    //Reference to other UIElement, to hide it when this is focused
    public enum PhoneState
    {
        NONE,
        HOVER,
        UNHOVER,
        FOCUS,
        FOCUSED,
        HIDDEN
    }
    [Header("Animations")]
    public AnimationClip idle;
    public AnimationClip hover, unhover, focus, focused, unfocus, hide, unhide, alert;
    private AnimationClip lastClip;
    public PhoneState phoneState;
    private bool isAnimating = false;

    [Header("References")]
    public InputHelper input;
    public Animator animator;
    public Button putAway;
    public CanvasGroup canvasGroup;
    private TimeManager timeManager;

    public enum PhoneApp
    {
        NONE,
        HOME,
        TEXTS,
        NOTES,
        PHOTOS,
        SETTINGS,
    }

    [Header("Apps")]
    public PhoneApp currentApp;
    [SerializeField]
    private AppManager _home;
    [SerializeField]
    private TextManager _texts;
    [SerializeField]
    private NoteManager _notes;
    [SerializeField]
    private SettingsManager _settings;
    //_photos, _settings;
    [SerializeField]
    private AppManager[] allApps;
    private AppManager _currentAppManager;
    private RectTransform _currentAppRt;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);
    }
    void Update()
    {
        //TODO: Remove this!
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.Play("PhoneFocus");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            animator.Play("PhoneUnfocus");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            animator.Play("PhoneHide");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            animator.Play("PhoneUnhide");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            animator.Play("PhoneAlert");
        }
    }
    void Start()
    {
        if (input == null)
        {
            input = InputHelper.Instance;
        }
        input.clickAction.started += Click;
        input.moveAction.performed += Navigate;
        input.moveAction.canceled += Navigate;
        input.jumpAction.started += Select;
        input.backAction.started += Back;
        canvasGroup.interactable = false;

        currentApp = PhoneApp.HOME;
        _currentAppManager = this;
        _currentAppRt = _home.rt;
        _texts.rt.gameObject.SetActive(false);

        OpenApp("home");
        IsReady = true;
    }
    public void SetTimeManager(TimeManager tm)
    {
        timeManager = tm;
    }

    #region Interactions
    void Click(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.inConvo && input.clickAction.triggered && phoneState == PhoneState.UNHOVER) //also, check if you're texting or in a story-specific message
        {
            UnfocusPhone();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.inConvo)
        {
            return;
        }
        if (phoneState == PhoneState.NONE || phoneState == PhoneState.UNHOVER)
        {
            phoneState = PhoneState.HOVER;
            if (!GameManager.Instance.inConvo)
            {
                animator.SetInteger("State", (int)phoneState);
            }
            //Debug.Log("Mouse enter");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance.inConvo)
        {
            return;
        }
        if (phoneState == PhoneState.NONE || phoneState == PhoneState.HOVER)
        {
            phoneState = PhoneState.UNHOVER;
            animator.SetInteger("State", (int)phoneState);
            //Debug.Log("Mouse exit");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.inConvo && (phoneState == PhoneState.NONE || phoneState == PhoneState.HOVER))
        {
            FocusPhone();
        }
    }

    void Navigate(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.inConvo || !IsFocused() || isAnimating)
        {
            return;
        }
        Vector2 input = context.ReadValue<Vector2>();
        if (currentApp != PhoneApp.HOME)
        {
            _currentAppManager.Navigate(input);
            return;
        }
        
        if (input.x > 0.1)
        {
            CheckOption(buttonIndex + 1);
        }
        else if (input.x < -0.1)
        {
            CheckOption(buttonIndex - 1);
        }
    }
    void Select(InputAction.CallbackContext context)
    {
        if (!IsFocused() || isAnimating)
        {
            return;
        }
        _currentAppManager.SelectOption(context);
    }
    public override void Back(InputAction.CallbackContext context)
    {
        if (!IsFocused() || GameManager.Instance.inConvo)
        {
            return;
        }
        _currentAppManager.Back();
    }
    #endregion

    public void SetInteractable(bool setting)
    {
        canvasGroup.interactable = setting;
    }
    //Previously Enter/Exit
    public void FocusPhone()
    {
        if (IsFocused())
        {
            return;
        }
        if (GameManager.Instance.inConvo)
        {
            putAway.gameObject.SetActive(false);
        }
        StartCoroutine(PlayFocusPhone());
    }
    public void UnfocusButton()
    {
        if (!GameManager.Instance.inConvo)
        {
            UnfocusPhone();
        }
    }
    [YarnCommand("unfocus")]
    public void UnfocusPhone()
    {
        if (!IsFocused())
        {
            return;
        }
        StartCoroutine(PlayUnfocusPhone());
    }
    public bool IsInApp()
    {
        return currentApp != PhoneApp.NONE && currentApp != PhoneApp.HOME;
    }
    /// <summary>
    /// True if state is HIDE or HIDDEN
    /// </summary>
    /// <returns></returns>
    public bool IsHidden()
    {
        return phoneState == PhoneState.HIDDEN;
    }
    /// <summary>
    /// True if currently stuck in an animation
    /// </summary>
    /// <returns></returns>
    public bool IsAnimating()
    {
        return isAnimating;
    }
    public void FreeControls()
    {
        isAnimating = false;
    }
    /// <summary>
    /// True if state is FOCUS
    /// </summary>
    /// <returns></returns>
    public bool IsFocused()
    {
        return isActiveAndEnabled && phoneState == PhoneState.FOCUS;
    }
    IEnumerator PlayFocusPhone()
    {
        Debug.Log("playing focus");
        PlayerControl.Instance.Pause();

        canvasGroup.interactable = true;
        putAway.gameObject.SetActive(true);
        isAnimating = true;

        animator.Play(focus.name);
        Focus();

        yield return new WaitForSeconds(focus.length);
        animator.Play(focused.name);

        phoneState = PhoneState.FOCUS;
    }
    IEnumerator PlayUnfocusPhone()
    {
        Debug.Log("playing unfocus");

        canvasGroup.interactable = false;
        putAway.gameObject.SetActive(false);
        isAnimating = true;

        OpenApp("home");
        animator.Play(unfocus.name);
        yield return new WaitForSeconds(unfocus.length);
        //animator.Play(idle.name);

        phoneState = PhoneState.NONE;
    }
    [YarnCommand("unhide")]
    public void Unhide()
    {
        if (phoneState != PhoneState.HIDDEN)
        {
            return;
        }
        Animate(unhide, PhoneState.NONE);
    }
    [YarnCommand("hide")]
    public void Hide()
    {
        Animate(hide, PhoneState.HIDDEN);
    }

    public void Alert()
    {
        Animate(alert);
    }
    void Animate(AnimationClip clip, PhoneState newState = PhoneState.NONE)
    {
        if (lastClip == clip)
        {
            Debug.Log($"trying to enter the same state - {clip.name}");
            return;
        }
        StartCoroutine(PlayAnim(clip, newState));
    }
    IEnumerator PlayAnim(AnimationClip clip, PhoneState newState = PhoneState.NONE)
    {
        Debug.Log($"playing clip {clip.name}");

        isAnimating = true;
        animator.Play(clip.name);
        yield return new WaitForSeconds(clip.length);
        phoneState = newState;
        //TODO: Remove this OR remove the trigger calls on animations
        //isAnimating = false;
    }

    #region App Controls
    public void PressHomeButton()
    {
        if (GameManager.Instance.inConvo)
        {
            return;
        }
        OpenApp("home");
    }
    public void OpenApp(string app)
    {
        PhoneApp p = (PhoneApp)Enum.Parse(typeof(PhoneApp), app, true);
        if (p == currentApp)
        {
            return;
        }

        currentApp = p;
        Debug.Log("Opening app " + currentApp);
        //timeManager.SetState(p == PhoneApp.HOME);

        if (p == PhoneApp.HOME)
        {
            FocusApp(_home, false);
            foreach (AppManager a in allApps)
            {
                a.Back();
            }
        }
        else if (p == PhoneApp.NOTES)
        {
            FocusApp(_notes);
        }
        else if (p == PhoneApp.TEXTS)
        {
            FocusApp(_texts);
        }
        else if (p == PhoneApp.SETTINGS)
        {
            FocusApp(_settings);
        }
    }
    void FocusApp(AppManager appToFocus, bool fromRight = true)
    {
        RectTransform rtToFocus = appToFocus.rt;
        foreach (AppManager app in allApps)
        {
            if (!(app is PhoneManager))
            {
                app.rt.gameObject.SetActive(false);
            }
            app.rt.anchoredPosition = new Vector2(0, 0f);
        }

        if (_currentAppRt != null)
            _currentAppRt.gameObject.SetActive(true);

        if (fromRight)
        {
            rtToFocus.anchoredPosition = new Vector2(56, 0f);
            rtToFocus.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuart);
            if (_currentAppRt != null)
                _currentAppRt.DOAnchorPosX(-56, 0.5f).SetEase(Ease.OutQuart);
            rtToFocus.gameObject.SetActive(true);
        }
        else
        {
            rtToFocus.anchoredPosition = new Vector2(-56, 0f);
            rtToFocus.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuart);
            if (_currentAppRt != null)
                _currentAppRt.DOAnchorPosX(56, 0.5f).SetEase(Ease.OutQuart);
            rtToFocus.gameObject.SetActive(true);
        }
        appToFocus.Focus();

        _currentAppManager = appToFocus;
        _currentAppRt = rtToFocus;
    }
    public void ClearBackable()
    {
        foreach (AppManager manager in allApps) {
            manager.SetBackable(true);
        }
    }
    #endregion

    public void OpenText(string contactName, Message m)
    {
        _texts.ForceOpen(contactName, m);
        if (currentApp != PhoneApp.TEXTS)
        {
            StartCoroutine(ForceText(0f));
        }
    }
    IEnumerator ForceText(float dur)
    {
        Debug.Log("forcing texts");
        yield return new WaitForSeconds(dur);
        FocusPhone();
        OpenApp("texts");
        _texts.SetBackable(false);
    }
    public void NotifyText(string contactName, Message m)
    {
        _texts.Notify(contactName, m);
    }
    public void NotifyText(Message m)
    {
        _texts.Notify(m);
    }
    public void ForceTextBackable(bool setting)
    {
        _texts.SetBackable(setting);
    }
    public void NotifyNote(string header)
    {
        _notes.ShowNote(header);
    }
    public void FinishAnimation()
    {
        isAnimating = false;
        Debug.Log($"finishing animation {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");
    }
    public void TestAnim(string state)
    {
        animator.Play(state);
    }
}
