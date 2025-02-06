using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHelper : MonoBehaviour
{
    [Header("Input Actions")]
    public PlayerInput input;
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction interactAction;
    public InputAction sneakAction;
    public InputAction phoneAction;
    public InputAction resetAction;
    public InputAction freeAction;
    public InputAction clickAction;

    [Header("Piano Keys")]
    public InputAction cKey;
    public InputAction cdKey;
    public InputAction dKey;
    public InputAction deKey;
    public InputAction eKey;
    public InputAction fKey;
    public InputAction fgKey;
    public InputAction gKey;
    public InputAction gaKey;
    public InputAction aKey;
    public InputAction abKey;
    public InputAction bKey;
    public InputAction c2Key;
    public InputAction[] pianoKeys;
    InputAction allActions;

    public static InputHelper Instance { get; private set; }
    public DeviceType ActiveDevice { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            //Destroy(this.gameObject);
        }

        moveAction = input.actions["Move"];
        jumpAction = input.actions["Jump"];
        interactAction = input.actions["Interact"];
        sneakAction = input.actions["Sneak"];
        phoneAction = input.actions["TogglePhone"];
        resetAction = input.actions["Reset"];
        freeAction = input.actions["Free"];
        clickAction = input.actions["Click"];

        cKey = input.actions["PianoC"];
        cdKey = input.actions["PianoCD"];
        dKey = input.actions["PianoD"];
        deKey = input.actions["PianoDE"];
        eKey = input.actions["PianoE"];
        fKey = input.actions["PianoF"];
        fgKey = input.actions["PianoFG"];
        gKey = input.actions["PianoG"];
        gaKey = input.actions["PianoGA"];
        aKey = input.actions["PianoA"];
        abKey = input.actions["PianoAB"];
        bKey = input.actions["PianoB"];
        c2Key = input.actions["PianoC2"];
        InputAction[] keys = { cKey, cdKey, dKey, deKey, eKey, fKey, fgKey, gKey, gaKey, aKey, abKey, bKey, c2Key };
        pianoKeys = keys;

        ActiveDevice = DeviceType.KEYBOARD;
    }
    public void UpdateDevice()
    {
        Debug.Log(input.currentControlScheme);
        if (input.currentControlScheme.Contains("Keyboard"))
        {
            ActiveDevice = DeviceType.KEYBOARD;
        }
        else
        {
            ActiveDevice = DeviceType.GAMEPAD;
        }
    }
}
