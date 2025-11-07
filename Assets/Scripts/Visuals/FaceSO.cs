using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Face Map", menuName = "ScriptableObjects/CharacterFaceMap")]
public class FaceSO : ScriptableObject
{
    [SerializeField]
    string characterName;

    [SerializeField]
    List<SpriteInfo> faceMap;

    public string CharacterName { get => characterName; set => characterName = value; }

    [System.Serializable]
    public class SpriteInfo
    {
        [SerializeField]
        string name;
        [SerializeField]
        Sprite sprite;

        public string Name { get => name; }
        public Sprite Sprite { get => sprite; }
    }
    public Dictionary<string, Sprite> GetFaceDictionary()
    {
        Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
        foreach (SpriteInfo fs in faceMap)
        {
            dict.Add(fs.Name, fs.Sprite);
        }
        return dict;
    }
}
