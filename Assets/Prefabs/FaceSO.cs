using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Face Map", menuName = "ScriptableObjects/CharacterFaceMap")]
public class FaceSO : ScriptableObject
{
    [SerializeField]
    string characterName;

    [SerializeField]
    List<FaceSprite> faceMap;

    [System.Serializable]
    class FaceSprite
    {
        public string faceName;

        public Sprite sprite;
    }

    public Sprite FindFace(string face)
    {
        if (faceMap == null || faceMap.Count == 0)
        {
            Debug.LogError($"No faces found for {characterName}");
            return null;
        }

        for (int i = 0; i < faceMap.Count; i++)
        {
            if (faceMap[i].faceName == face)
            {
                return faceMap[i].sprite;
            }
        }
        return faceMap[0].sprite;
    }
}
