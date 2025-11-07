using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Input Sprite Map", menuName = "ScriptableObjects/InputSpriteMap")]
public class InputSO : ScriptableObject
{
    [System.Serializable]
    public class InputSpriteInfo
    {
        [SerializeField]
        string actionName;
        [SerializeField]
        string keybind;
        [SerializeField]
        Sprite sprite;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
