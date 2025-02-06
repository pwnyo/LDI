using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class FaceManager : MonoBehaviour
{
    //TODO: Make this compatible with emojis (e.g. prefaced by an @ :) )
    //Also, make it "remember" the previous expression of all characters in the convo, and reset them to nothing once a conversation ends

    [System.Serializable]
    public class Face
    {
        public string expression;
        public Sprite sprite;
    }

    public Sprite fillerSprite;
    public Face[] eddyFaces;
    public Face[] juneFaces;
    public Face[] ellenFaces;
    public Face[] momFaces;
    public Face[] dadFaces;

    //Sprite[] prevSprites;

    private void Start()
    {
        //ResetSprites();
    }
    /*
    public void ResetSprites()
    {
        Debug.Log("reset");
        prevSprites = new Sprite[]
        {
            eddyFaces[0].sprite,
            fillerSprite,
            fillerSprite,
            fillerSprite,
            fillerSprite,
        };
    }*/

    Sprite SpriteFromExpression(Face[] faces, string expression)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i].expression == expression)
            {
                //Debug.Log("found face " + expression);
                return faces[i].sprite;
            }
        }
        //Debug.Log("didn't find expression " + expression);
        return null;
    }
    public Sprite GetDefaultFace(string charName = "")
    {
        string lower = charName.ToLower();
        switch (lower)
        {
            case ("eddy"):
            case (""):
                return eddyFaces[0].sprite;
            case ("june"):
                return juneFaces[0].sprite;
            case ("ellen"):
                return ellenFaces[0].sprite;
            case ("mom"):
                return momFaces[0].sprite;
            case ("dad"):
                return dadFaces[0].sprite;
            default:
                return fillerSprite;
        }
    }
    public bool IsNamedCharacter(string name)
    {
        string lower = name.ToLower();
        switch (lower)
        {
            case ("eddy"):
            case (""):
            case ("june"):
            case ("ellen"):
            case ("mom"):
            case ("dad"):
                return true;
            default:
                return false;
        }
    }
    public Sprite GetFaceFromName(string exp, string charName = "")
    {
        if (exp.Length < 1)
            return GetDefaultFace(charName);
        string lower = charName.ToLower();
        //Debug.Log($"looking for name {lower} {exp}");
        switch (lower)
        {
            case ("eddy"):
            case (""):
                Debug.Log("found name " + charName);
                //prevSprites[0] = SpriteFromExpression(eddyFaces, exp);
                return SpriteFromExpression(eddyFaces, exp);
            case ("june"):
                //prevSprites[1] = SpriteFromExpression(juneFaces, exp);
                return SpriteFromExpression(juneFaces, exp);
            case ("ellen"):
                //prevSprites[2] = SpriteFromExpression(ellenFaces, exp);
                return SpriteFromExpression(ellenFaces, exp);
            case ("mom"):
                //prevSprites[3] = SpriteFromExpression(momFaces, exp);
                return SpriteFromExpression(momFaces, exp);
            case ("dad"):
                //prevSprites[4] = SpriteFromExpression(dadFaces, exp);
                return SpriteFromExpression(dadFaces, exp);
            default:
                return fillerSprite;
        }
    }
    /*
    public void GetPrevFace(string charName = "")
    {
        string lower = charName.ToLower();
        Debug.Log("looking for name " + lower);
        switch (lower)
        {
            case ("eddy"):
            case (""):
                imageTalkL.sprite = eddyFaces[0].sprite;
                //imageTalkL.sprite = prevSprites[0];
                break;
            case ("june"):
                imageTalkL.sprite = juneFaces[0].sprite;
                //imageTalkL.sprite = prevSprites[1];
                break;
            case ("ellen"):
                imageTalkL.sprite = ellenFaces[0].sprite;
                //imageTalkL.sprite = prevSprites[2];
                break;
            case ("mom"):
                imageTalkL.sprite = momFaces[0].sprite;
                //imageTalkL.sprite = prevSprites[3];
                break;
            case ("dad"):
                imageTalkL.sprite = dadFaces[0].sprite;
                //imageTalkL.sprite = prevSprites[4];
                break;
            default:
                //imageTalkL.sprite = fillerSprite;
                break;
        }
    }*/
}
