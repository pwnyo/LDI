using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSwap : MonoBehaviour
{
    public SpriteRenderer sr;
    public bool hasAnimator;
    public Sprite[] sprites;
    int index;
    bool goingBackward;

    [YarnCommand("swapsprite")]
    public void SwapSprite(string param)
    {
        int.TryParse(param, out index);
        SwapSprite(index);
    }
    public void SwapSprite(int ind)
    {
        index = ind;
        if (index >= 0 && index <= sprites.Length)
        {
            sr.sprite = sprites[index];
        }
        if (hasAnimator)
        {
            GetComponent<Animator>().StopPlayback();
        }
    }
    public void SwapSprite()
    {
        index++;
        if (index >= sprites.Length) {
            index = 0;
        }
        sr.sprite = sprites[index];
    }
    public void SwapSpritePend()
    {
        if (index == 0)
        {
            goingBackward = false;
        }
        else if (index == sprites.Length - 1) {
            goingBackward = true;
        }
        if (goingBackward)
        {
            index--;
        }
        else {
            index++;
        }
        sr.sprite = sprites[index];
    }
}
