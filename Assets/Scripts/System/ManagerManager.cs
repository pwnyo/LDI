using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerManager : MonoBehaviour
{
    [Header("Other Managers")]
    public AudioManager audioManager; //musicplay and stop, musicin and out
    public CameraManager cameraManager; //cameratarget and shake
    public EventManager eventManager; //doessay, quiz, trash, rice
    //facemanager
    public GameDialogueManagerOld dialogueManager; //setdialoguestate, setboxlocation, setspritecolor, setviet, shake
    public ObjectSchedule objectSchedule; //setday and timeofday
    public UIManager uiManager; //uihide and show; clockset, start, and stop; addtrash and finishtrash
    public ViewManager viewManager; //goto, toblack, fadetoblack

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    IEnumerator DelayDialogueUntilFade()
    {
        //yield return viewManager.Tran
    }*/
}
