using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles UI display for the piano keys
/// </summary>
public class PianoKey : MonoBehaviour
{
    public MusicKey key;
    public AudioSource sound;
    public Button button;
    Color normalColorW, pressColorW;
    Color normalColorB, pressColorB;

    public void Play()
    {
        sound.Play();
    }
    public void PlayAndHighlight()
    {
        sound.Play();
        SetAlpha(0.75f);
    }
    public void Release()
    {
        SetAlpha(1f);
    }
    void SetAlpha(float alpha)
    {
        Color c = button.image.color;
        c.a = alpha;
        button.image.color = c;
    }
    public void SetColor(Color normalW, Color pressW, Color normalB, Color pressB)
    {
        normalColorW = normalW;
        pressColorW = pressW;
        normalColorB = normalB;
        pressColorB = pressB;
        PaintColor();
    }
    void PaintColor()
    {
        ColorBlock block = button.colors;
        block.normalColor = IsBlackKey(key) ? normalColorB : normalColorW;
        block.pressedColor = IsBlackKey(key) ? pressColorB : pressColorW;
        block.highlightedColor = IsBlackKey(key) ? pressColorB : pressColorW;
        button.colors = block;
    }

    public static bool IsBlackKey(MusicKey key)
    {
        switch (key)
        {
            case (MusicKey.CD):
            case (MusicKey.DE):
            case (MusicKey.FG):
            case (MusicKey.GA):
            case (MusicKey.AB):
                return true;
            default:
                return false;
        }
    }
}
