using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using System.Text;
using UnityEngine.InputSystem;

//Communicates with the DialogueRunner and DialogueUI
//to play appropriate animations and parse text.

public class GameDialogueManager : MonoBehaviour
{
    public static GameDialogueManager Instance { get; private set; }
    public DialogueRunner dr;
    public DialogueUI dui;
    public PhoneManager phoneManager;
    public FaceManager faceManager;
    public GameObject showFullButton;
    public GameObject continueButton;
    public List<Button> optionsTalk;
    public List<Button> optionsText; 
    public InputHelper input;

    public enum DialogueState
    {
        NONE,
        TALK,
        TEXT,
        WHISPER,
        EXPO,
    }
    public enum BoxLocation
    {
        UPPER,
        UP, //Only these middle 2 are used for texts
        DOWN,
        DOWNER,
        CENTER,
    }
    public enum BoxDirection
    {
        LEFT,
        RIGHT
    }

    public DialogueState dialogueState;
    public BoxLocation boxLocation;
    public bool isViet;
    private bool dontHideBetweenScenes;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip sfx;
    public AudioClip phoneSfxMe, phoneSfxOther;
    public AudioClip boxSfx;
    public AudioClip[] sounds;

    [Header("Wait Times")]
    public float charTime;
    public float commaTime;
    public float periodTime;
    private bool isWaiting;

    public float boxOpenTime;
    public float boxCloseTime;
    public float phoneUpTime;
    public float textOpenTime;
    public Animator blinker;
    private Message currentMessage;

    [Header("Talking Mode")]
    public TalkMessage talker;
    public TextMeshProUGUI subtitle;
    public RectTransform subtitleBox;
    bool isSubtitleUp;

    [Header("Texting Mode")]
    public TextMessage texter;
    public TextConversation convo;
    string currentContactName;
    int lineCount;
    bool isWaitingForPhone;

    [Header("Exposition")]
    public GameObject expositionObject;
    public Animator expositionAnimator;
    public TextMeshProUGUI expositionText;
    public float expoHoldTime, expoWaitTime;

    [Header("Other Vars")]
    public DebugHelper debugHelper;
    private DialogueState prevState;
    public float quickNextTime;
    public float quickNextTimeText;
    private float nextHoldTime;
    private bool isHolding;

    private bool waitingForOptions;
    private List<Button> buttons;
    private int buttonCount;
    private int buttonIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            //Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (input == null)
        {
            input = InputHelper.Instance;
        }
        //HideAll();
        input.jumpAction.started += Next;
        input.jumpAction.canceled += Next;
        input.jumpAction.started += Select;
        input.moveAction.performed += Navigate;
        input.moveAction.canceled += Navigate;
        //input.interactAction.started += Select;
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueState != DialogueState.NONE)
        {
            if (isHolding && !GameManager.Instance.inTransition)
            {
                float targetTime = 1f;
                if (dialogueState == DialogueState.TALK)
                    targetTime = quickNextTime;
                else if (dialogueState == DialogueState.TEXT)
                    targetTime = quickNextTimeText;

                nextHoldTime += Time.deltaTime * debugHelper.QuickHoldMultiplier();
                if (nextHoldTime > targetTime)
                {
                    Next();
                    nextHoldTime = 0;
                }
            }
            else
            {
                nextHoldTime = 0;
            }
        }
    }
    void Next(InputAction.CallbackContext context)
    {
        if (dialogueState != DialogueState.NONE && context.phase == InputActionPhase.Started)
        {
            isHolding = true;
            if (!isWaitingForPhone && dialogueState != DialogueState.EXPO)
            {
                Next();
                nextHoldTime = 0;
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isHolding = false;
            nextHoldTime = 0;
        }
    }
    void Navigate(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.inTransition || dialogueState == DialogueState.NONE || !waitingForOptions)
        {
            return;
        }
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x > 0.1 && buttonIndex % 2 == 0)
        {
            CheckOption(buttonIndex + 1);
        }
        else if (input.x < -0.1 && buttonIndex % 2 != 0)
        {
            CheckOption(buttonIndex - 1);
        }
        if (input.y > 0.1)
        {
            CheckOption(buttonIndex - 2);
        }
        else if (input.y < -0.1)
        {
            CheckOption(buttonIndex + 2);
        }
    }
    void Select(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.inTransition || dialogueState == DialogueState.NONE || !waitingForOptions || 
            buttons == null || buttons.Count == 0 || buttonIndex < 0 || buttonIndex >= buttons.Count)
        {
            return;
        }
        buttons[buttonIndex].onClick.Invoke();
    }
    public void StartDialogue()
    {
        if (dialogueState == DialogueState.NONE)
        {
            dialogueState = DialogueState.TALK;
        }
        if (dialogueState != DialogueState.NONE)
        {
            PlayerControl.Instance.SetPlayerState(PlayerControl.PlayerState.BUSY);
            GameManager.Instance.inConvo = true;
        }
        PlayerControl.Instance.DisableInteractArrow();
    }
    public void StopDialogue()
    {
        if (dialogueState == DialogueState.TEXT)
        {
            Debug.Log("Trying to close phone");
            StartCoroutine(ClosePhone());
        } 
        else
        {
            HideAll();
            PlayerControl.Instance.SetPlayerState(PlayerControl.PlayerState.NONE);
            GameManager.Instance.inConvo = false;
        }
    }
    void HideAll()
    {
        nextHoldTime = 0;
        PhoneManager.Instance.ClearBackable();
        PlayerControl.Instance.SetPlayerState(PlayerControl.PlayerState.NONE);
        PlayerControl.Instance.EnableInteractArrow();

        SetBlinker(false);
        isWaiting = false;
        isSubtitleUp = false;
        dialogueState = DialogueState.NONE;
        boxLocation = BoxLocation.UP;
        isViet = false;
        talker.SetViet(isViet);

        lineCount = 0;
        currentContactName = "";
        dontHideBetweenScenes = false;
        //PhoneManager.Instance.SetInteractable(true);

        talker.ResetFace();
        talker.gameObject.SetActive(false);
        showFullButton.SetActive(false);
        continueButton.SetActive(false);
        CloseSubtitle();

        sfxSource.Stop();
    }
    void CheckOption(int index)
    {
        if (index >= 0 && index < buttonCount)
        {
            buttonIndex = index;
            buttons[index].Select();
        }
    }
    public void StartOptions()
    {
        waitingForOptions = true;
        buttons = dui.optionButtons;
        buttonIndex = 0;

        if (buttons == null || buttons.Count <= 0)
        {
            return;
        }
        int count = 0;
        foreach (Button b in buttons)
        {
            if (b.isActiveAndEnabled)
                count++;
        }
        buttonCount = count;

        buttons[0].Select();
    }
    public void EndOptions()
    {
        waitingForOptions = false;
    }
    public void ShowBetweenScenes(string param)
    {
        bool.TryParse(param, out bool show);
        dontHideBetweenScenes = show;
    }
    [YarnCommand("textcontact")]
    public void SetTextPartner(string contactName)
    {
        currentContactName = contactName;
        dialogueState = DialogueState.TEXT;
        PhoneManager.Instance.tManager.SetBackable(false);
    }
    [YarnCommand("hidetalk")]
    public void HideTalk()
    {
        talker.gameObject.SetActive(false);
    }
    [YarnCommand("setdialogue")]
    public void SetDialogueState(string param)
    {
        string setting = param;
        if (setting == "talk")
        {
            dialogueState = DialogueState.TALK;
        }
        else if (setting == "text")
        {
            dialogueState = DialogueState.TEXT;
            PhoneManager.Instance.tManager.SetBackable(false);
            PhoneManager.Instance.Focus();
        }
        else if (setting == "whisper")
        {
            dialogueState = DialogueState.WHISPER;
        }
        else if (setting == "exposition" || setting == "expo")
        {
            prevState = dialogueState;
            dialogueState = DialogueState.EXPO;
        }
        Debug.Log("Dialogue setting is currently " + dialogueState);
    }
    [YarnCommand("setboxlocation")]
    public void SetBoxLocation(string param)
    {
        string setting = param.ToLower();
        if (setting == "upper")
        {
            boxLocation = BoxLocation.UPPER;
        }
        else if (setting == "up")
        {
            boxLocation = BoxLocation.UP;
        }
        else if (setting == "down")
        {
            boxLocation = BoxLocation.DOWN;
        }
        else if (setting == "downer")
        {
            boxLocation = BoxLocation.DOWNER;
        }
        else if (setting == "center")
        {
            boxLocation = BoxLocation.CENTER;
        }
    }
    [YarnCommand("shake")]
    public void ShakeBox(string[] param)
    {
        if (param.Length != 2)
        {
            Debug.Log("Shake command doesn't have two parameters.");
        }
        else
        {
            int.TryParse(param[0], out int amount);
            int.TryParse(param[0], out int duration);
            if (dialogueState == DialogueState.TALK)
            {
                Debug.Log("Shaking text box! " + amount + ", " + duration);
            }
        }
    }

    public void GetLine(string line)
    {
        //Debug.Log(dialogueState);

        lineCount++;

        //Defaults to TALK if no other setting has been selected
        switch (dialogueState) {
            case (DialogueState.TALK):
                //Debug.Log("Trying to talk");
                dui.optionButtons = optionsTalk;
                PhoneManager.Instance.SetInteractable(false);
                talker.gameObject.SetActive(true);
                TalkLine(line);
                break;
            case (DialogueState.TEXT):
                //Debug.Log("Trying to text");
                //dui.optionButtons = optionsText;

                //PhoneManager.Instance.SetInteractable(true);
                TextLine(currentContactName, line);
                break;
            case (DialogueState.EXPO):
                ExpoLine(line);
                break;
        }
    }
    Message ParseLinesToMessage(string line)
    {
        Message m = new Message();
        string[] lines = line.Split(new char[] { ':' }, 3);

        if (lines.Length > 1)
        {
            //Debug.Log("Speaker: " + lines[0] + "Line: " + lines[1].TrimStart(' '));

            m = new Message(lines[0], lines[1]); 

        }
        else
        {
            //Debug.Log("Line: " + line);

            m = new Message("", lines[0]);
        }
        if (isSubtitleUp)
        {
            CloseSubtitle();
        }
        if (lines.Length > 2)
        {
            ShowSubtitle(lines[2]);
        }
        return m;
    }
    string[] ParseLines(string line)
    {
        //Debug.Log(line);
        string[] lines = line.Split( new char[] { ':' }, 3);
        if (isSubtitleUp)
        {
            CloseSubtitle();
        }
        if (lines.Length > 2)
        {
            ShowSubtitle(lines[2]);
        }
        return lines;
    }
    string ParseExpression(string line)
    {
        string[] lines = line.Split('^');
        string s = "";
        foreach (string spl in lines)
        {
            s += spl + ",";
        }
        Debug.Log("Parsed expression lines: " + s);
        if (lines.Length > 1 && lines[1].Length > 0)
        {
            return lines[1];
        }
        else
        {
            return null;
        }
    }
    void ExpoLine(string line)
    {
        Debug.Log("Trying to exposit " + line);
        HideAll();
        expositionText.text = line;
        StartCoroutine(Exposition());
    }
    IEnumerator Exposition()
    {
        expositionObject.SetActive(true);
        yield return new WaitForSeconds(expoHoldTime);
        expositionAnimator.SetTrigger("Start");
        yield return new WaitForSeconds(expoWaitTime / 2f);

        expositionObject.SetActive(false);
        dui.MarkLineComplete();
        dialogueState = prevState;
        yield return new WaitForSeconds(expoWaitTime / 2f);

        expositionAnimator.ResetTrigger("Start");
    }
    void TalkLine(string line)
    {
        isWaiting = true;
        SetBlinker(false);

        line = ParseBoxLocation(line);
        line = ParseFace(line);
        line = ParseLanguage(line);

        currentMessage = ParseLinesToMessage(line);
        talker.Push(currentMessage);
        
        StartCoroutine(PlayDialogue(currentMessage));
    }
    void TextLine(string contactName, string line)
    {
        Debug.Log($"Contacting {contactName}");
        //Debug.Log("Called text");
        currentMessage = ParseLinesToMessage(line);

        //PhoneManager.Instance.NotifyText(contactName, currentMessage);
        PhoneManager.Instance.OpenText(contactName, currentMessage);
        sfxSource.PlayOneShot(phoneSfxMe);
        //convo.Add(currentMessage);
        //Resize buttons
        showFullButton.SetActive(false);
        continueButton.SetActive(true);
    }
    void TextLine(string line)
    {
        sfxSource.PlayOneShot(sfx);
        //Debug.Log("Called text");
        currentMessage = ParseLinesToMessage(line);

        PhoneManager.Instance.NotifyText(currentMessage);
        //convo.Add(currentMessage);
        //Resize buttons
        showFullButton.SetActive(false);
        continueButton.SetActive(true);
    }
    void WhisperLine(string line)
    {

    }
    string ParseBoxLocation(string line)
    {
        ///TODO: Not quite the loop you want -- needs to remove everything BETWEEN two delimiters, or after the last one, if there's only 1 left
        List<string> parsed = new List<string>(line.Split('^'));
        if (parsed.Count <= 1)
        {
            return line;
        }
        for (int i = 1; i < parsed.Count; i += 2)
        {
            BoxLocation bl = BoxLocation.UP;
            string setting = parsed[i].ToLower();
            if (setting == "upper" || setting == "uu")
            {
                bl = BoxLocation.UPPER;
            }
            else if (setting == "up" || setting == "u")
            {
                bl = BoxLocation.UP;
            }
            else if (setting == "down" || setting == "d")
            {
                bl = BoxLocation.DOWN;
            }
            else if (setting == "downer" || setting == "dd")
            {
                bl = BoxLocation.DOWNER;
            }
            else if (setting == "center" || setting == "c")
            {
                bl = BoxLocation.CENTER;
            }
            talker.Align(bl);
            boxLocation = bl;

            parsed.RemoveAt(i);
            Debug.Log($"removing {i}");
        }
        Debug.Log($"parsed {boxLocation}");
        return Combine(parsed);
    }
    string ParseFace(string line)
    {
        List<string> parsed = new List<string>(line.Split('&'));
        if (parsed.Count <= 1)
        {
            talker.Express("");
            return line;
        }
        for (int i = 1; i < parsed.Count; i += 2)
        {
            string setting = parsed[i];
            talker.Express(setting);

            parsed.RemoveAt(i);
        }
        return Combine(parsed);
    }
    string ParseLanguage(string line)
    {
        List<string> parsed = new List<string>(line.Split('@'));
        if (parsed.Count <= 1)
        {
            return line;
        }
        for (int i = 1; i < parsed.Count; i += 2)
        {
            string setting = parsed[i].ToLower();
            if (setting == "v" || setting == "viet")
            {
                talker.SetViet(true);
            }
            else
            {
                talker.SetViet(false);
            }

            parsed.RemoveAt(i);
        }
        return Combine(parsed);
    }
    string Combine(List<string> list)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string s in list)
        {
            sb.Append(s.TrimStart(' '));
        }
        return sb.ToString();
    }
    void ShowSubtitle(string line)
    {
        subtitleBox.gameObject.SetActive(true);
        subtitle.gameObject.SetActive(true);
        subtitle.text = line;
        subtitleBox.sizeDelta = new Vector2(subtitle.preferredWidth + 50, subtitle.preferredHeight);
        isSubtitleUp = true;
    }
    void CloseSubtitle()
    {
        subtitle.text = "";
        subtitleBox.gameObject.SetActive(false);
        subtitle.gameObject.SetActive(false);
        isSubtitleUp = false;
    }
    IEnumerator PlayDialogue(Message m)
    {
        WaitForSeconds wait = new WaitForSeconds(charTime);
        WaitForSeconds comWait = new WaitForSeconds(commaTime);
        WaitForSeconds perWait = new WaitForSeconds(periodTime);
        WaitForSeconds perWaitDouble = new WaitForSeconds(periodTime * 2);
        int consecutivePeriods = 0;

        string speech = talker.CurrentSpeechFull;
        string speaker = talker.CurrentSpeakerFull;
        int charsInBox = speech.Length;

        for (int i = speaker.Length - 1 > 0 ? speaker.Length - 1 : 0; i < charsInBox; i++)
        {
            char c = speech[i];
            talker.ShowMore();

            sfxSource.PlayOneShot(sfx);

            if (i < charsInBox - 1)
            {
                if (c.Equals(','))
                {
                    yield return comWait;

                    consecutivePeriods = 0;
                }
                else if (c.Equals('.') || c.Equals('!') || c.Equals('?') || c.Equals('-'))
                {
                    if (i < charsInBox - 2 && speech[i + 1] == ' ')
                    {
                        yield return perWaitDouble;
                    }
                    else if (consecutivePeriods > 0)
                    {
                        yield return new WaitForSeconds(periodTime * Mathf.Pow(1.1f, consecutivePeriods));
                    }
                    else
                    {
                        yield return perWait;
                    }

                    consecutivePeriods++;
                }
                else
                {
                    yield return wait;

                    consecutivePeriods = 0;
                }
            }
        }
        isWaiting = false;
        SetBlinker(true);
        SetContinue();
    }
    IEnumerator ShowPhone(string line)
    {
        isWaitingForPhone = true;
        //PhoneManager.Instance.Alert();
        phoneManager.Alert();
        sfxSource.PlayOneShot(sounds[0]);
        yield return new WaitForSeconds(PhoneManager.Instance.alertAnimationTime);
        //PhoneManager.Instance.Focus();
        phoneManager.Focus();
        yield return new WaitForSeconds(PhoneManager.Instance.focusAnimationTime + 0.1f);
        isWaitingForPhone = false;

        talker.gameObject.SetActive(false);
        TextLine(line);
    }
    IEnumerator ClosePhone()
    {
        //PhoneManager.Instance.Unfocus();
        phoneManager.Unfocus();
        yield return new WaitForSeconds(PhoneManager.Instance.hideAnimationTime - 0.5f);
        HideAll();
        GameManager.Instance.inConvo = false;
    }
    [YarnCommand("putawayphone")]
    public void PutAwayPhone()
    {
        StartCoroutine(HidePhone());
    }
    IEnumerator HidePhone()
    {
        phoneManager.Unfocus();
        yield return new WaitForSeconds(PhoneManager.Instance.hideAnimationTime - 0.5f);
    }

    public void Next()
    {
        if (GameManager.Instance.inTransition)
        {
            return;
        }
        Debug.Log("nexting");
        if (dialogueState == DialogueState.TEXT)
        {
            sfxSource.PlayOneShot(sfx);
        }
        if (isWaiting)
        {
            ShowFullDialogue();
            //Debug.Log("full");
        }
        else
        {
            dui.MarkLineComplete();
            //Debug.Log("done");
        }
    }
    public void ShowFullDialogue()
    {
        StopAllCoroutines();
        talker.ShowAll();

        SetContinue();
    }
    public void SetContinue()
    {
        isWaiting = false;
        SetBlinker(true);
        sfxSource.Stop();
        showFullButton.SetActive(false);
        continueButton.SetActive(true);
    }
    public void SetBlinker(bool setting)
    {
        blinker.SetBool("On", setting);
    }
}
