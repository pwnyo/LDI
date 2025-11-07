using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Reactor : Interactable
{
    DialogueRunner dialogueRunner;
    Animator animator;
    public float animationTime;

    protected bool isOn;
    public bool startAutomatically;

    [Header("Optional")]
    public string talkToNode = "";
    public YarnProgram scriptToLoad;

    public AudioSource sfxPlayer;
    public AudioClip sound;
    public bool loopSound;
    public bool playOneShot;
    public bool playOn;
    public bool playOff;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        animator = GetComponent<Animator>();

        if (scriptToLoad != null)
        {
            Debug.Log("Adding script " + scriptToLoad.name);

            dialogueRunner.Add(scriptToLoad);
        }
    }

    public override void Interact()
    {
        if (talkToNode.Length > 0)
        {
            dialogueRunner.StartDialogue(talkToNode);
        }
        isOn = !isOn;
        if (animator)
        {
            animator.SetBool("On", isOn);
        }
        if (sound != null && sfxPlayer)
        {
            sfxPlayer.PlayOneShot(sound);
        }
    }

    public void PlayAnimation()
    {

    }
}
