using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class YarnItem : MonoBehaviour
{
    public string id;
    public GameObject item;
    Animator anim;
    SpriteRenderer sr;
    public bool startOff;

    [System.Serializable]
    public class SpriteInfo
    {
        public string name;
        public Sprite sprite;
    }

    //TODO: Could rework this into FaceSO.NamedSprite, but probably not worth it
    public SpriteInfo[] sprites;

    private void Start()
    {
        if (item == null)
            item = this.gameObject;
        anim = item.GetComponent<Animator>();
        sr = item.GetComponent<SpriteRenderer>();

        YarnItemController.Instance.Add(this);
        if (startOff)
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Shows or hides gameobject
    /// </summary>
    /// <param name="setting"></param>
    public void ShowObject(bool setting)
    {
        if (item)
            item.SetActive(setting);
        else
            gameObject.SetActive(setting);
    }
    /// <summary>
    /// Plays a given animation state
    /// </summary>
    /// <param name="state"></param>
    public void Animate(string state)
    {
        if (anim)
        {
            anim.Play(state);
        }
    }
    /// <summary>
    /// Sets a sprite in the SR based on the SpriteInfo list
    /// </summary>
    /// <param name="sprite"></param>
    public void UseAltSprite(string sprite)
    {
        foreach (SpriteInfo si in sprites)
        {
            if (si.name == sprite)
            {
                sr.sprite = si.sprite;
                return;
            }
        }
    }
    /// <summary>
    /// Shows or hides sprite renderer
    /// </summary>
    /// <param name="setting"></param>
    public void ShowSpriteRenderer(bool setting)
    {
        sr.enabled = setting;
    }
}
