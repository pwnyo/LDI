using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicPiece : MonoBehaviour
{
    public string title, subtitle;
    public List<PlayableMusicNote> notes;
    public bool useSharps;
    public bool isDone;

    [System.Serializable]
    public class PlayableMusicNote
    {
        public MusicNote note;
        [Range(0,1)]
        public float duration;
    }
    private void Start()
    {
    }

    public bool TryKey(MusicKey testKey, int index)
    {
        if (notes == null || testKey == notes[index].note.noteKey)
        {
            return true;
        }
        return false;
    }
    public int PieceLength()
    {
        return notes.Count;
    }    

    /// <summary>
    /// Currently unused
    /// </summary>
    public class MusicMeasure
    {
        public bool isPickup;
        public List<MusicNote> notes;
        public int MeasureNumber
        {
            get;
            set;
        }
        public int CurrentIndex
        {
            get;
            set;
        }
        public bool IsFinished
        {
            get;
            set;
        }
        public bool TryKey(MusicKey testKey)
        {
            if (notes == null || testKey == notes[CurrentIndex].noteKey)
            {
                NextNote();
                return true;
            }
            return false;
        }
        public void NextNote()
        {
            CurrentIndex += 1;
            Debug.Log(MeasureNumber + " " + CurrentIndex);
            if (CurrentIndex >= notes.Count)
            {
                CurrentIndex = notes.Count;
                IsFinished = true;
            }
        }
        public void Restart()
        {
            CurrentIndex = 0;
            IsFinished = false;
        }
    }
}
