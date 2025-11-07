using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class FaceManager : MonoBehaviour
{
    //TODO: Make this compatible with emojis (e.g. prefaced by an @ :) )
    //Also, make it "remember" the previous expression of all characters in the convo, and reset them to nothing once a conversation ends

    public Sprite fillerSprite;

    public List<FaceSO> faceSOs;
    private Dictionary<string, Dictionary<string, Sprite>> faceMap;
    private Dictionary<string, Sprite> lastUsedFaces;

    //Sprite[] prevSprites;

    private void Awake()
    {
        faceMap = new Dictionary<string, Dictionary<string, Sprite>>();
        foreach (FaceSO fs in faceSOs)
        {
            faceMap.Add(fs.CharacterName, fs.GetFaceDictionary());
        }
    }
    private void Start()
    {
        //ResetSprites();
    }
    public bool IsNamedCharacter(string name)
    {
        return name.Length == 0 || !faceMap.ContainsKey(name) || faceMap[name].Count > 0;
    }
    public Sprite GetFaceFromName(string charName, string exp)
    {
        if (string.IsNullOrEmpty(charName))
        {
            charName = "Eddy";
        }
        if (string.IsNullOrEmpty(exp))
        {
            exp = "normal";
        }

        if (faceMap == null || faceMap.Count == 0 || !faceMap.ContainsKey(charName) || faceMap[charName].Count == 0)
        {
            Debug.LogError($"No faces found for {charName}");
            return fillerSprite;
        }
        if (faceMap[charName].ContainsKey(exp))
        {
            return faceMap[charName][exp];
        }
        else
        {
            Debug.LogWarning($"Using default face for {charName} instead of {exp}");
            return faceMap[charName]["normal"];
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
