using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles music input and output using PianoKeys, also controls MusicNoteUI objects on the staff
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    [Header("Input/Output")]
    public InputHelper input;
    public PianoKey[] pianoKeys;
    public Color blackNorm, blackPress;
    public Color whiteNorm, whitePress;
    public Ease successEase;
    public CanvasGroup successGroup;
    public Animator successAnim;

    public UnityEvent successEvents, failEvents;

    [Header("Note Display")]
    public MusicNoteUI currentNoteUI;
    public List<MusicNoteUI> noteUIPool;
    int currentPieceIndex;
    public RectTransform sheetTransform;
    public float vSpacing, hSpacing, cOffset, xOffset;

    [Header("Positions")]
    public float[] keyYPositions;

    // Start is called before the first frame update
    void Start()
    {
        InputAction[] keys = input.pianoKeys;
        if (pianoKeys == null || pianoKeys.Length != 13)
        {
            Debug.LogWarning("Piano is messed up!");
            return;
        }

        for (int i = 0; i < keys.Length; i++)
        {
            int _i = i;
            keys[i].started += _ => PlayAndHighlight(_i);
            keys[i].canceled += _ => Release(_i);
            pianoKeys[i].button.onClick.AddListener(() => Play(_i));
            pianoKeys[i].SetColor(whiteNorm, whitePress, blackNorm, blackPress);
        }

        vSpacing = sheetTransform.rect.height / 4;
    }
    #region Input/Output
    void PlayAndHighlight(int num)
    {
        if (!MusicManager.Instance.CanPlay())
        {
            return;
        }
        Try(num);
        pianoKeys[num].PlayAndHighlight();
    }
    void Play(int num)
    {
        if (!MusicManager.Instance.CanPlay())
        {
            return;
        }
        Try(num);
        pianoKeys[num].Play();
    }
    void Release(int num)
    {
        pianoKeys[num].Release();
    }
    void Try(int num)
    {
        currentNoteUI.gameObject.SetActive(MusicManager.Instance.state == MusicManager.MusicState.FREE);

        if (MusicManager.Instance.state == MusicManager.MusicState.FREE)
        {
            int staffKey = GetStaffKey(num);
            currentNoteUI.SetPositionRaw(xOffset, keyYPositions[staffKey], staffKey);
        }
        else if (MusicManager.Instance.state == MusicManager.MusicState.PIECE)
        {
            TryPlayNoteInPiece((MusicKey)num);
        }
    }
    #endregion


    #region Note Display and Piece Playback
    /// <summary>
    /// Tests input key against the current piece
    /// </summary>
    /// <param name="key"></param>
    void TryPlayNoteInPiece(MusicKey key)
    {
        if (!CurrentPiece())
        {
            return;
        }
        //note is current
        if (CurrentPiece().TryKey(key, currentPieceIndex))
        {
            //this is the last note in the piece
            if (!GetNextNoteUIForPiece())
            {
                Debug.Log("Success!");
                OnSuccess();
            }
        }
        else
        {
            Debug.Log("Failure...");
            OnFail();
        }
    }
    /// <summary>
    /// Opens staff, shows preview, and then allows input
    /// </summary>
    public void OpenWithPiece()
    {
        PlayPiece();
    }
    /// <summary>
    /// Redraws currently displayed notes based on most recent input device
    /// </summary>
    public void Redraw()
    {
        Redraw(InputHelper.Instance.ActiveDevice);
    }
    void Redraw(DeviceType device)
    {
        MusicPiece piece = CurrentPiece();
        if (piece == null)
        {
            return;
        }

        ResetNotes(MusicNoteUI.MusicNoteState.INACTIVE);
        for (int i = 0; i < piece.notes.Count; i++)
        {
            DrawNote(piece, i);
        }
    }
    /// <summary>
    /// Places note UI based on the given piece and index
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="noteIndex"></param>
    /// <param name="state"></param>
    void DrawNote(MusicPiece piece, int noteIndex, MusicNoteUI.MusicNoteState state = MusicNoteUI.MusicNoteState.NONE)
    {
        int staffKey = GetStaffKey((int)(piece.notes[noteIndex].note.noteKey));
        noteUIPool[noteIndex].SetPositionRaw(xOffset + (hSpacing * noteIndex), keyYPositions[staffKey], staffKey);
        if (state != MusicNoteUI.MusicNoteState.NONE)
        {
            noteUIPool[noteIndex].SetState(state);
        }
    }
    /// <summary>
    /// Sets all note UI to a default state
    /// </summary>
    /// <param name="defState"></param>
    void ResetNotes(MusicNoteUI.MusicNoteState defState)
    {
        currentPieceIndex = 0;
        MusicPiece piece = CurrentPiece();
        for (int i = 0; i < noteUIPool.Count; i++)
        {
            if (i < piece.notes.Count)
            {
                noteUIPool[i].SetState(defState);
            }
            else
            {
                noteUIPool[i].SetState(MusicNoteUI.MusicNoteState.HIDDEN);
            }
        }
    }
    /// <summary>
    /// Returns next note UI in the current piece
    /// </summary>
    /// <returns></returns>
    MusicNoteUI GetNextNoteUIForPiece()
    {
        if (currentPieceIndex > noteUIPool.Count)
        {
            Debug.LogError("Current piece has too many notes to display!");
            return null;
        }

        noteUIPool[currentPieceIndex].SetState(MusicNoteUI.MusicNoteState.ACTIVE);
        Debug.Log($"Played note at index {currentPieceIndex}");

        //end of piece
        if (currentPieceIndex == CurrentPiece().PieceLength() - 1)
        {
            currentPieceIndex = 0;
            return null;
        }
        currentPieceIndex++;
        return noteUIPool[currentPieceIndex];
    }
    /// <summary>
    /// Plays piece before allowing input
    /// </summary>
    void PlayPiece()
    {
        StartCoroutine(PiecePlayback());
    }
    IEnumerator PiecePlayback()
    {
        MusicManager.Instance.EnablePlay(false);
        ResetNotes(MusicNoteUI.MusicNoteState.HIDDEN);

        MusicPiece piece = CurrentPiece();
        for (int i = 0; i < piece.notes.Count; i++)
        {
            pianoKeys[(int)piece.notes[i].note.noteKey].Play();
            DrawNote(piece, i, MusicNoteUI.MusicNoteState.INACTIVE);
            yield return new WaitForSeconds(piece.notes[i].duration);
        }
        MusicManager.Instance.EnablePlay(true);
    }
    void OnSuccess()
    {
        successEvents.Invoke();

        successGroup.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(successGroup.DOFade(1f, 0.2f));

        PlayPiece();
    }
    void OnFail()
    {
        failEvents.Invoke();
        ResetNotes(MusicNoteUI.MusicNoteState.INACTIVE);
    }

    /// <summary>
    /// Converts a note number to a functional index on the staff for display
    /// </summary>
    /// <param name="noteNum"></param>
    /// <param name="useSharps"></param>
    /// <returns></returns>
    public int GetStaffKey(int noteNum)
    {
        if (PianoKey.IsBlackKey((MusicKey)noteNum))
        {
            if (CurrentPiece().useSharps)
            {
                noteNum -= 1;
            }
            else
            {
                noteNum += 1;
            }
        }
        //adjust for octave
        int octave = noteNum / 12;
        noteNum += octave * 2;

        //convert from music note to staff note
        if (noteNum % 2 == 0)
        {
            noteNum = noteNum / 2;
        }
        else
        {
            noteNum = (noteNum + 1) / 2;
        }
        return noteNum;
    }
    /// <summary>
    /// Helper for current piece
    /// </summary>
    /// <returns></returns>
    MusicPiece CurrentPiece()
    {
        return MusicManager.Instance.currentPiece;
    }
    #endregion
}
