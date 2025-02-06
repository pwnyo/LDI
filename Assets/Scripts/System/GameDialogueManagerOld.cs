using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

//Communicates with the DialogueRunner and DialogueUI
//to play appropriate animations and parse text.

public class GameDialogueManagerOld : MonoBehaviour
{
    public static GameDialogueManagerOld Instance { get; private set; }
    public PlayerControl pc;
    public DialogueUI dui;
    public DialogueRunner dr;
    public UIManager uiManager;
    public FaceManager faceManager;
    public GameObject showFullButton;
    public GameObject continueButton;
    public List<Button> optionsTalk;
    public List<Button> optionsText;

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


    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip sfx;
    public AudioClip boxSfx;
    public AudioClip[] sounds;

    //Should eventually have two settings: one for texting and one for speech

    [Header("Sprites")]
    public Image imageTalkL;
    public Image imageTalkR;
    public Image imageTextA;
    public Image imageTextB;
    public Sprite[] eddyFaces;
    public Sprite[] juneFaces;

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

    [Header("Talking Mode")]
    public TMP_FontAsset englishFont;
    public TMP_FontAsset vietFont;
    public Color englishFontColor;
    public Color vietFontColor;
    public Color englishBoxColor;
    public Color vietBoxColor;
    public Color eNameColor;
    public Color vNameColor;
    public float eFontSize;
    public float vFontSize;
    public Animator talkAnimator;
    public RectTransform talkContainer;
    public Image talkBox;
    public TextMeshProUGUI speech;
    public TextMeshProUGUI subtitle;
    public RectTransform subtitleBox;
    bool isSubtitleUp;

    [Header("Texting Mode")]
    public Color leftColor;
    public Color rightColor;
    public Animator textAnimatorA;
    public Animator textAnimatorB;
    public GameObject textContainer;
    public GameObject textOverlay;
    public RectTransform textAreaA;
    public RectTransform textAreaB;
    public RectTransform textBoxA;
    public RectTransform textBoxB;
    public RectTransform messengerBoxA;
    public RectTransform messengerBoxB;
    public TextMeshProUGUI messengerA;
    public TextMeshProUGUI messengerB;
    public TextMeshProUGUI messageA;
    public TextMeshProUGUI messageB;
    int lineCount;
    bool isALeft;
    bool isBLeft;
    bool isWaitingForPhone;

    [Header("Exposition")]
    public GameObject expositionObject;
    public Animator expositionAnimator;
    public TextMeshProUGUI expositionText;

    private void Awake()
    {
        if (Instance == null || Instance != this)
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //HideAll();
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueState != DialogueState.NONE)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            { 
                if (!isWaitingForPhone && dialogueState != DialogueState.EXPO) {
                    Next();
                }
                else
                {
                    Debug.Log("not nexting");
                }
            }
            /*
            if (dui.optionButtons[0].isActiveAndEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    dui.optionButtons[0].Select();
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    dui.optionButtons[1].Select();
                }
            }*/
        }
    }
    public void StartDialogue()
    {
        if (dialogueState == DialogueState.NONE)
        {
            dialogueState = DialogueState.TALK;
        }
        pc.SetPlayerState(PlayerControl.PlayerState.BUSY);
        GameManager.Instance.inConvo = true;
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
            pc.SetPlayerState(PlayerControl.PlayerState.NONE);
            GameManager.Instance.inConvo = false;
        }
    }
    void HideAll()
    {
        SetBlinker(false);
        isWaiting = false;
        isSubtitleUp = false;
        dialogueState = DialogueState.NONE;
        boxLocation = BoxLocation.UP;
        isViet = false;

        lineCount = 0;
        isALeft = true;
        isBLeft = false;

        textAreaA.gameObject.SetActive(true);
        textAreaB.gameObject.SetActive(false);

        textContainer.SetActive(false);
        talkContainer.gameObject.SetActive(false);
        textOverlay.SetActive(false);
        showFullButton.SetActive(false);
        continueButton.SetActive(false);
        CloseSubtitle();

        sfxSource.Stop();
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
        }
        else if (setting == "whisper")
        {
            dialogueState = DialogueState.WHISPER;
        }
        else if (setting == "exposition" || setting == "expo")
        {
            dialogueState = DialogueState.EXPO;
        }
        //Debug.Log("Dialogue setting is currently " + dialogueState);
    }
    [YarnCommand("setbl")]
    public void SetBoxLocation(string param)
    {
        string setting = param;
        if (setting == "uu")
        {
            boxLocation = BoxLocation.UPPER;
        }
        else if (setting == "u")
        {
            boxLocation = BoxLocation.UP;
        }
        else if (setting == "d")
        {
            boxLocation = BoxLocation.DOWN;
        }
        else if (setting == "d")
        {
            boxLocation = BoxLocation.DOWNER;
        }
        else if (setting == "c")
        {
            boxLocation = BoxLocation.CENTER;
        }
    }
    [YarnCommand("setspritecolor")]
    public void SetSprite(string[] param)
    {
        string color = param[0];
        //Debug.Log(param.Length + " " + color);
        if (color.Equals("red"))
        {
            imageTalkL.color = Color.red;
        }
        else if (color.Equals("orange"))
        {
            imageTalkL.color = new Color(255, 165, 0);
        }
        else if (color.Equals("yellow"))
        {
            imageTalkL.color = Color.yellow;
        }
        else if (color.Equals("green"))
        {
            imageTalkL.color = Color.green;
        }
        else
        {
            Debug.Log("Not a valid color!");
        }
    }
    [YarnCommand("setviet")]
    public void SetViet(string param)
    {
        if (param == "true")
        { 
            SetViet(true);
        }
        else if (param == "false")
        {
            SetViet(false);
        }
    }
    void SetViet(bool setting)
    {
        isViet = setting;
        if (isViet)
        {
            talkBox.color = vietBoxColor;
            speech.font = vietFont;
            speech.color = vietFontColor;
        }
        else
        {
            talkBox.color = englishBoxColor;
            speech.font = englishFont;
            speech.color = englishFontColor;
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
        Debug.Log(dialogueState);

        lineCount++;

        //Defaults to TALK if no other setting has been selected
        switch (dialogueState) {
            case (DialogueState.TALK):
                Debug.Log("Trying to talk");
                dui.optionButtons = optionsTalk;
                textContainer.SetActive(false);
                talkContainer.gameObject.SetActive(true);
                TalkLine(line);
                break;
            case (DialogueState.TEXT):
                //Debug.Log("Trying to text");
                dui.optionButtons = optionsText;
                if (lineCount > 1)
                {
                    TextLine(line);
                }
                else if (PhoneManager.Instance.phoneState != PhoneManager.PhoneState.HIDDEN)
                {
                    StartCoroutine(ShowPhone(line));
                }
                break;
            case (DialogueState.EXPO):
                ExpoLine(line);
                break;
        }
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
        yield return new WaitForSeconds(2f);
        expositionAnimator.SetTrigger("Start");
        yield return new WaitForSeconds(.8f);

        expositionObject.SetActive(false);
        dui.MarkLineComplete();
        //expositionAnimator.ResetTrigger("Start");
    }
    void TalkLine(string line)
    {
        isWaiting = true;
        SetBlinker(false);
        SetViet(isViet);
        RectTransform rt = talkContainer;
        switch (boxLocation)
        {
            case (BoxLocation.UPPER):
                rt.localPosition = new Vector3(0, -50, 0);
                break;
            case (BoxLocation.UP):
                rt.localPosition = new Vector3(0, -150, 0);
                break;
            case (BoxLocation.DOWN):
                rt.localPosition = new Vector3(0, -615, 0);
                break;
            case (BoxLocation.DOWNER):
                rt.localPosition = new Vector3(0, -725, 0);
                break;
            case (BoxLocation.CENTER):
                rt.localPosition = new Vector3(0, -400, 0);
                break;
        }
        //Debug.Log("Called talk");
        string[] lines = ParseLines(line);
        string expression = ParseExpression(line);
        string s;
        //if (lines.Length > 1 && lines[0].Length > 0)
        if (lines.Length > 1)
        {
            //Debug.Log("Speaker: " + lines[0] + "Line: " + lines[1].TrimStart(' '));

            if (lines[0].Length > 0)
            {
                speech.maxVisibleCharacters = lines[0].Length + 2;
                if (isViet)
                {
                    speech.fontSize = vFontSize;
                    speech.text = "<b><#" + ColorUtility.ToHtmlStringRGB(vNameColor) + ">" + lines[0] + "</color></b>:";
                }
                else
                {
                    speech.fontSize = eFontSize;
                    speech.text = "<b><#" + ColorUtility.ToHtmlStringRGB(eNameColor) + ">" + lines[0] + "</color></b>:";
                }

                speech.text += lines[1];

                /*
                if (expression != null)
                {
                    faceManager.GetFaceFromName(expression, lines[0]);
                }
                else
                {
                    faceManager.GetPrevFace(lines[0]);
                }*/
            }
            else
            {
                if (isViet)
                {
                    speech.fontSize = vFontSize;
                }
                else
                {
                    speech.fontSize = eFontSize;
                }

                speech.maxVisibleCharacters = 0;

                speech.text = lines[1];
            }
            s = lines[1];
        }
        else
        {
            //Debug.Log("Line: " + line);
            if (isViet)
            {
                speech.fontSize = vFontSize;
            }
            else
            {
                speech.fontSize = eFontSize;
            }

            speech.maxVisibleCharacters = 0;
            speech.text = line;
            s = lines[0];

            if (expression != null)
            {
                faceManager.GetFaceFromName(expression);
            }
        }

        StartCoroutine(PlayDialogue(s));
    }
    void TextLine(string line)
    {
        sfxSource.PlayOneShot(sfx);
        //Debug.Log("Called text");
        string[] lines = ParseLines(line);

        bool onLeft = lines.Length > 1;
        bool onA = (lineCount % 2 != 0);
        //Debug.Log("Left " + onLeft + " A " + onA);

        if (lineCount > 1)
        {
            textAreaB.gameObject.SetActive(true);
        }

        //Look for odd lines to act on A, even for B
        if (onA) {
            //Set variable
            isALeft = onLeft;

            //Place left or right
            if (onLeft)
            {
                textBoxA.anchoredPosition = new Vector2(-15, 0);
                textBoxA.GetComponent<Image>().color = leftColor;
                messengerBoxA.anchoredPosition = new Vector2(-400, 100);
                messengerA.text = lines[0];

                messageA.text = lines[1].TrimStart(' ');
                messageA.alignment = TextAlignmentOptions.TopLeft;
                //messageA.GetComponent<Image>().color = leftColor;
            }
            else
            {
                textBoxA.anchoredPosition = new Vector2(15, 0);
                textBoxA.GetComponent<Image>().color = rightColor;
                messengerBoxA.anchoredPosition = new Vector2(400, 100);
                messengerA.text = "";

                messageA.text = line;
                messageA.alignment = TextAlignmentOptions.TopRight;
            }

            //Resize text box
            textBoxA.sizeDelta = new Vector2(textBoxA.sizeDelta.x, messageA.preferredHeight + 35);

            //Place up or down
            //Play animation to move the other message up, and an animation to pop into place
            if (lineCount > 1) //Odd-numbered message
            {
                if (isALeft == isBLeft && messengerA.text.Equals(messengerB.text)) //Determine if the image should be hidden
                {
                    messengerBoxA.gameObject.SetActive(false);
                    messengerBoxB.gameObject.SetActive(true);
                }
                else
                {
                    messengerBoxA.gameObject.SetActive(true);
                    messengerBoxB.gameObject.SetActive(true);
                }

                textAreaB.anchoredPosition = new Vector2(0, 500);
                textAreaA.anchoredPosition = new Vector2(0, textAreaB.localPosition.y - textBoxB.sizeDelta.y - 25);
            }
            else { //First message

                textAreaA.localPosition = new Vector2(0, 500);
                //textAreaB.gameObject.SetActive(false);
            }
        }
        else
        {
            //Set variable
            //textAreaB.gameObject.SetActive(true);
            isBLeft = onLeft;

            //Place left or right
            if (onLeft)
            {
                textBoxB.anchoredPosition = new Vector2(-15, 0);
                textBoxB.GetComponent<Image>().color = leftColor;
                messengerBoxB.anchoredPosition = new Vector2(-400, 100);
                messengerB.text = lines[0];

                messageB.text = lines[1].TrimStart(' ');
                messageB.alignment = TextAlignmentOptions.TopLeft;
            }
            else
            {
                textBoxB.anchoredPosition = new Vector2(15, 0);
                textBoxB.GetComponent<Image>().color = rightColor;
                messengerBoxB.anchoredPosition = new Vector2(400, 100);
                messengerB.text = "";

                messageB.text = line;
                messageB.alignment = TextAlignmentOptions.TopRight;
            }
            //Resize text box
            textBoxB.sizeDelta = new Vector2(textBoxB.sizeDelta.x, messageB.preferredHeight + 35);

            //Place up or down
            //Play animation to move the other message up, and an animation to pop into place
            if (lineCount > 1) //Even-numbered message
            {
                if (isBLeft == isALeft && messengerB.text.Equals(messengerA.text)) //Determine if the image should be hidden
                {
                    
                    messengerBoxB.gameObject.SetActive(false);
                    messengerBoxA.gameObject.SetActive(true);
                }
                else
                {
                    messengerBoxB.gameObject.SetActive(true);
                    messengerBoxA.gameObject.SetActive(true);
                }
                textAreaA.anchoredPosition = new Vector2(0, 500);
                textAreaB.anchoredPosition = new Vector2(0, textAreaA.localPosition.y - textBoxA.sizeDelta.y - 25);
            }
        }

        //Debug.Log("A " + onA + ", ALeft " + isALeft + "| BLeft " + isBLeft);

        //Resize buttons
        showFullButton.SetActive(false);
        continueButton.SetActive(true);
    }
    void WhisperLine(string line)
    {

    }
    void ShowSubtitle(string line)
    {
        subtitleBox.gameObject.SetActive(true);
        subtitle.gameObject.SetActive(true);
        subtitle.text = line;
        subtitleBox.sizeDelta = new Vector2(subtitle.preferredWidth + 50, subtitle.preferredHeight + 25);
        isSubtitleUp = true;
    }
    void CloseSubtitle()
    {
        subtitle.text = "";
        subtitleBox.gameObject.SetActive(false);
        subtitle.gameObject.SetActive(false);
        isSubtitleUp = false;
    }
    IEnumerator PlayDialogue(string line)
    {
        WaitForSeconds wait = new WaitForSeconds(charTime);
        WaitForSeconds comWait = new WaitForSeconds(commaTime);
        WaitForSeconds perWait = new WaitForSeconds(periodTime);
        WaitForSeconds perWaitDouble = new WaitForSeconds(periodTime * 2);
        int consecutivePeriods = 0;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            speech.maxVisibleCharacters++;

            sfxSource.PlayOneShot(sfx);

            if (i < line.Length - 1)
            {
                if (c.Equals(','))
                {
                    yield return comWait;

                    consecutivePeriods = 0;
                }
                else if (c.Equals('.') || c.Equals('!') || c.Equals('?') || c.Equals('-'))
                {
                    if (i < line.Length - 2 && line[i + 1] == ' ')
                    {
                        yield return perWaitDouble;
                    }
                    else if (consecutivePeriods > 0)
                    {
                        yield return new WaitForSeconds(periodTime * Mathf.Pow(1.2f, consecutivePeriods));
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
        //uiManager.AlertPhone();
        sfxSource.PlayOneShot(sounds[0]);
        yield return new WaitForSeconds(PhoneManager.Instance.alertAnimationTime);
        //PhoneManager.Instance.Focus();
        //uiManager.FocusPhone();
        yield return new WaitForSeconds(PhoneManager.Instance.focusAnimationTime + 0.1f);
        isWaitingForPhone = false;

        talkContainer.gameObject.SetActive(false);
        textContainer.SetActive(true);
        TextLine(line);
        textOverlay.SetActive(true);
    }
    IEnumerator ClosePhone()
    {
        //PhoneManager.Instance.Unfocus();
        //uiManager.UnfocusPhone();
        yield return new WaitForSeconds(PhoneManager.Instance.hideAnimationTime - 0.5f);
        HideAll();
        pc.SetPlayerState(PlayerControl.PlayerState.NONE);
        GameManager.Instance.inConvo = false;
    }
    [YarnCommand("putawayphone")]
    public void PutAwayPhone()
    {
        StartCoroutine(HidePhone());
    }
    IEnumerator HidePhone()
    {
        //uiManager.UnfocusPhone();
        textOverlay.SetActive(false);
        yield return new WaitForSeconds(PhoneManager.Instance.hideAnimationTime - 0.5f);
    }

    public void Next()
    {
        Debug.Log("nexting");
        if (dialogueState == DialogueState.TEXT)
        {
            sfxSource.PlayOneShot(sounds[1]);
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
        speech.maxVisibleCharacters = speech.text.Length;

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
