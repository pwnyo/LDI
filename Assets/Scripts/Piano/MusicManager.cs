using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Manager for music minigame (NOT audio controls)
/// </summary>
public class MusicManager : AppManager
{
    public static MusicManager Instance { get; private set; }
    public List<MusicPiece> pieces;
    public MusicPiece currentPiece;
    public MusicState state;
    public MusicPlayer musicPlayer;
    public bool showingPiece;
    public List<MusicPieceUI> pieceUIList;
    public CanvasGroup canvasGroup;

    [Header("Piano Display Settings")]
    public GameObject piano;
    public GameObject staff;

    [Header("Folder Display Settings")]
    public Animator folderAnimator;
    public GameObject background;

    [Header("Piece Display Settings")]
    public Ease easeType;
    public Color lightColor, darkColor;
    public float minX, maxX;
    public float minY, maxY;
    public float duration;
    public bool IsPlayable { get; private set; }

    public enum MusicState
    {
        OFF,
        FREE,
        PIECE,
    }
    enum MusicFolderState
    {
        AWAY,
        UP,
        DOWN,
        HOLD,
        SIDE,
        SIDEUP,
    }

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
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Redraw()
    {

    }

    [YarnCommand("pianoup")]
    public void ShowPieces()
    {
        GameManager.Instance.inPiano = true;
        canvasGroup.interactable = false;

        folderAnimator.gameObject.SetActive(true);
        background.SetActive(true);
        StartCoroutine(ShowPieceDelay());
    }
    IEnumerator ShowPieceDelay()
    {
        SetAnimState(MusicFolderState.UP);
        yield return new WaitForSeconds(0.75f);

        for (int i = 0; i < pieces.Count; i++)
        {
            MusicPiece p = pieces[i];
            MusicPieceUI pui = pieceUIList[i];
            float lerpAmount = i / (float)(pieces.Count - 1);
            pui.gameObject.SetActive(true);
            pui.SetPiece(p.title, p.subtitle, false, Color.Lerp(darkColor, lightColor, lerpAmount));
            pui.MovePos(Mathf.Lerp(minX, maxX, lerpAmount), Mathf.Lerp(minY, maxY, lerpAmount), duration, easeType, true);
        }

        SetAnimState(MusicFolderState.SIDE);
        yield return new WaitForSeconds(0.4f);
        canvasGroup.interactable = true;
    }
    public void ExpandPiece(int index)
    {
        state = MusicState.PIECE;
        currentPiece = pieces[index];

        //hide selection UI
        foreach (MusicPieceUI pui in pieceUIList)
        {
            pui.gameObject.SetActive(false);
        }
        //populate with notes
        piano.SetActive(true);
        staff.SetActive(true);
        currentPiece = pieces[Mathf.Clamp(index, 0, pieces.Count)];
        musicPlayer.OpenWithPiece();
    }
    public void UnexpandPiece()
    {

    }
    public void Close()
    {
        StartCoroutine(CloseDelay());
    }
    IEnumerator CloseDelay()
    {
        SetAnimState(MusicFolderState.SIDEUP);

        for (int i = 0; i < pieces.Count; i++)
        {
            MusicPieceUI pui = pieceUIList[i];
            pui.MovePos(0, 0, duration, easeType, false);
        }

        yield return new WaitForSeconds(1f);
        SetAnimState(MusicFolderState.DOWN);
        yield return new WaitForSeconds(0.5f);

        folderAnimator.gameObject.SetActive(false);
        background.SetActive(false);

        GameManager.Instance.inPiano = false;
    }
    public override void Back()
    {
        base.Back();
        if (expanded)
        {
            if (!showingPiece)
                UnexpandPiece();
            else
                Close();
        }
    }
    public void EnablePlay(bool setting)
    {
        IsPlayable = setting;
        canvasGroup.interactable = CanPlay();
    }
    public bool CanPlay()
    {
        return IsPlayable && musicPlayer.isActiveAndEnabled;
    }
    void SetAnimState(MusicFolderState state)
    {
        folderAnimator.SetInteger("State", (int)state);
    }
}
