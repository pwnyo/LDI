using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class PlayerControl : MonoBehaviour, ISoundMaker
{
    public static PlayerControl Instance { get; private set; }
    public enum PlayerState
    {
        NONE, //can go to any state
        BUSY, //can't go to any state but NONE
        JUMP, //only if you're not busy
        FALL,
        LAND,
        SNEAK, //only if you're not jumping (same for others)
        WALK,
        CROUCH,
        SIT,
        CAUGHT,
    }
    public PlayerState state;
    public bool forceSneak;
    public bool isSmall;
    public bool debugPause;

    [Header("Sounds")]
    public AudioManager.AudioTrack[] sounds;
    [Range(0, 1.5f)]
    public float minPitch, maxPitch;
    [Range(0, 2f)]
    public float sneakPitchMultiplier, sneakVolMultiplier;

    #region References
    [Header("Input Actions")]
    public InputHelper input;
    [Header("References")]
    public Animator animator;
    public GameObject interactArrow;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer altSpriteRenderer;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private AudioSource sfxPlayer;
    public Material activeInteractableMaterial;

    private DialogueRunner dr;
    public float interactMinDistance;
    private List<Interactable> interactList = new List<Interactable>();
    private Interactable interactSelection; 
    private int interactIndex;
    private bool prevArrowState;

    #endregion

    #region Sprites
    [Header("Sprites")]
    public FaceManager.Face[] altSprites;
    #endregion

    #region Variables
    [Header("Movement")]
    private bool canMove;
    private Vector3 move;
    public float walkSpeed;
    public float sneakSpeed;
    float moveX;

    [Header("Fall and Jump")]
    public LayerMask jumpLayerMask;
    public float jumpSpeed;
    public float jumpTime;
    public float landTime;
    public float jumpBufferTime;
    float timeSinceJumpInput;
    public float fallSpeed;
    public float fallFastMultiplier;
    [SerializeField]
    private float fallSpeedActual;
    private bool isFastfall;
    [SerializeField]
    GroundState groundState;

    enum GroundState
    {
        GROUNDED,
        RISING,
        FALLING,
    }

    [Header("Stealth")]
    public LayerMask stealthLayerMask;
    public Vector3 soundOffset;
    public float sneakRadius;
    public float walkRadius;
    public bool isDetectable;
    private bool isSneaking;

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        sfxPlayer = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        dr = FindObjectOfType<DialogueRunner>();

        input = InputHelper.Instance;
        interactIndex = -1;

        canMove = true;
        fallSpeedActual = fallSpeed;
        isFastfall = false;
    }
    void OnEnable()
    {
        input.jumpAction.started += Jump;
        input.jumpAction.canceled += Fastfall;
        input.sneakAction.started += Sneak;
        input.sneakAction.canceled += Sneak;
        //input.moveAction.performed += Move;
        //input.moveAction.canceled += Move;
        input.interactAction.started += Interact;
        input.interactAction.canceled += Interact;
        input.phoneAction.started += TogglePhone;
        input.freeAction.started += Free;
        input.resetAction.started += Reset;
    }
    void OnDisable()
    {
        input.jumpAction.started -= Jump;
        input.jumpAction.canceled -= Fastfall;
        input.sneakAction.started -= Sneak;
        input.sneakAction.canceled -= Sneak;
        //input.moveAction.performed -= Move;
        //input.moveAction.canceled -= Move;
        input.interactAction.started -= Interact;
        input.interactAction.canceled -= Interact;
        input.phoneAction.started -= TogglePhone;
        input.freeAction.started -= Free;
        input.resetAction.started -= Reset;
    }

    // Update is called once per frame
    void Update()
    {
        if (CanAct() || IsStopped())
        {
            CheckGround();
            CheckFall();
            CheckAnimation();
        }
    }
    void FixedUpdate()
    {
        if (CanAct()) 
        {
            transform.position += move * Time.deltaTime;
        }
    }
    public void SetPlayerState(PlayerState setting)
    {
        state = setting;
        if (state == PlayerState.BUSY)
        {
            Pause();
        }
        //Debug.Log(setting);
    }

    #region Movement Functions
    void CheckAnimation()
    {
        CheckMove();
        move.x = IsSneaky() && groundState == GroundState.GROUNDED ? moveX * sneakSpeed : moveX * walkSpeed;
        if (groundState != GroundState.GROUNDED)
        {
            return;
        }
        if (IsStopped())
        {
            if (IsSneaky())
            {
                PlayAnimation(PlayerState.CROUCH);
            }
            else
            {
                PlayAnimation(PlayerState.NONE);
            }
        }
        else
        {
            if (IsSneaky())
            {
                PlayAnimation(PlayerState.SNEAK);
            }
            else
            {
                PlayAnimation(PlayerState.WALK);
            }
        }
    }

    void CheckMove()
    {
        //quit early if we stopped moving
        if (!CanAct())
        {
            return;
        }
        moveX = input.moveAction.ReadValue<Vector2>().x;
        if (input.moveAction.IsPressed())
        {
            spriteRenderer.flipX = moveX < 0;
        }
    }
    void Move(InputAction.CallbackContext context)
    {
        //read input
        moveX = context.ReadValue<Vector2>().x;
        //quit early if we stopped moving and we're grounded
        if (!CanAct() || IsStopped() || (context.phase == InputActionPhase.Canceled && groundState == GroundState.GROUNDED))
        {
            moveX = 0;
            return;
        }
        spriteRenderer.flipX = moveX < 0;
    }
    void Sneak(InputAction.CallbackContext context)
    {
        if (!CanAct())
        {
            return;
        }
        isSneaking = context.phase != InputActionPhase.Canceled;
        if (groundState != GroundState.GROUNDED)
        {
            return;
        }
        AdjustCollider(isSneaking);
        if (isSneaking)
        {
            if (IsStopped())
            {
                PlayAnimation(PlayerState.NONE);
            }
            else
            {
                PlayAnimation(PlayerState.WALK);
            }
        }
        else
        {
            if (IsStopped())
            {
                PlayAnimation(PlayerState.CROUCH);
            }
            else
            {
                PlayAnimation(PlayerState.SNEAK);
            }
        }
    }
    void AdjustCollider(bool isCrouch)
    {
        if (!isSmall)
        {
            if (isCrouch)
            {
                col.offset = new Vector2(0, .75f);
                col.size = new Vector2(.75f, 1.5f);
            }
            else
            {
                col.offset = new Vector2(0, 1.875f);
                col.size = new Vector2(.75f, 3.75f);
            }
        }
    }
    void Jump(InputAction.CallbackContext context)
    {
        if (!CanAct() || GameDialogueManager.Instance.dialogueState != GameDialogueManager.DialogueState.NONE)
        {
            return;
        }
        timeSinceJumpInput = Time.time;
        if (groundState == GroundState.GROUNDED)
        {
            //Debug.Log("Trying to jump!");
            groundState = GroundState.RISING;
            PlayAnimation(PlayerState.JUMP);
            //PlaySound("JUMP", 0.5f);
            rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        }
    }
    void CheckFall()
    {
        if (groundState != GroundState.GROUNDED && rb.velocity.y < -0.05)
        {
            groundState = GroundState.FALLING;
            PlayAnimation(PlayerState.FALL);
        }
    }
    void Fastfall(InputAction.CallbackContext context)
    {
        if (groundState != GroundState.GROUNDED && !isFastfall)
        {
            //Debug.Log("Fastfalling");
            isFastfall = true;
            rb.gravityScale = fallSpeed * fallFastMultiplier;
        }
    }
    void Land()
    {
        Debug.Log("landed");
        isFastfall = false;
        groundState = GroundState.GROUNDED;
        rb.gravityScale = fallSpeed;

        if (false && (Time.time - timeSinceJumpInput) < jumpBufferTime)
        {
            Debug.Log("buffered jump!");
            //Jump();
        }
        else
        {
            PlaySound("LAND", 0.5f);
            if (!IsStopped())
            {
                PlayAnimation(PlayerState.WALK);
            }
            else
            {
                PlayAnimation(PlayerState.NONE);
            }
        }
    }

    void CheckGround()
    {
        if (groundState != GroundState.FALLING)
        {
            return;
        }
        Bounds colBounds = col.bounds;
        Vector2 colBox = new Vector2(colBounds.size.x - .25f, colBounds.size.y);
        Vector2 colCenter = new Vector2(colBounds.center.x, colBounds.center.y - .05f);
        Collider2D raycastHit = Physics2D.OverlapBox(colCenter, colBox, 0, jumpLayerMask);
        //Debug.Log(raycastHit != null);
        bool hitGround = raycastHit != null;
        if (hitGround)
        {
            Land();
        }
    }

    public void Pause()
    {
        moveX = 0;
        move = Vector2.zero;
        PlayAnimation(PlayerState.NONE);
    }
    bool IsStopped()
    {
        return Mathf.Abs(moveX) < 0.1f;
    }
    public void AllowMovement(bool setting)
    {
        canMove = setting;
        if (!canMove)
        {
            Pause();
        }
    }
    public void Spawn(Vector3 location, bool setting)
    {
        transform.position = location;
        spriteRenderer.flipX = setting;
    }
    public void SpawnKeepFlip(Vector3 location)
    {
        Spawn(location, spriteRenderer.flipX);
    }
    #endregion

    [YarnCommand("showplayer")]
    public void ShowPlayer(string param)
    {
        
        bool.TryParse(param, out bool setting);
        spriteRenderer.enabled = setting;
        canMove = setting;
        Debug.Log("showing player " + setting);
        interactArrow.GetComponent<SpriteRenderer>().enabled = setting;

    }
    [YarnCommand("usealt")]
    public void UseAlt(string spriteName)
    {
        foreach (FaceManager.Face f in altSprites)
        {
            if (f.expression == spriteName) {
                altSpriteRenderer.sprite = f.sprite;
                altSpriteRenderer.enabled = true;
                spriteRenderer.enabled = false;
            }
        }
    }
    [YarnCommand("usebase")]
    public void UseBase()
    {
        foreach (FaceManager.Face f in altSprites)
        {
            altSpriteRenderer.sprite = f.sprite;
            altSpriteRenderer.enabled = false;
            spriteRenderer.enabled = true;
        }
    }
    void Interact(InputAction.CallbackContext context)
    {
        if (!CanAct())
        {
            return;
        }
        if (groundState == GroundState.GROUNDED && context.phase == InputActionPhase.Started)
        {
            if (interactList.Count > 0)
            {
                SelectInteractable(input.moveAction.ReadValue<Vector2>().x);
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            if (interactSelection != null)
            {
                interactSelection.Interact();
                interactArrow.SetActive(false);
                PlaySound("INTERACT", 0.2f);
            }
        }
    }
    void CheckInteractables()
    {
        Debug.Log("checking interactables");
        interactList.Sort();
        foreach (Interactable i in interactList)
        {
            Debug.Log($"{i.interactableName} {Mathf.Abs(transform.position.x - i.transform.position.x)}");
        }
        interactSelection = interactList[0];
    }
    public void AddInteractable(Interactable i)
    {
        if (interactList == null)
            Debug.Log("failed");
        interactList.Add(i);
        Debug.Log($"added {i.interactableName}");
    }
    public void SelectInteractable(float f)
    {
        //Debug.Log(interactList.Count);
        
        if (f > 0)
        {
            interactIndex++;
            if (interactIndex >= interactList.Count)
            {
                interactIndex = 0;
            }
        }
        else if (f < 0)
        {
            interactIndex--;
            if (interactIndex < 0)
            {
                interactIndex = interactList.Count - 1;
            }
        }
        PlaceInteractArrow(interactList[interactIndex]);
    }
    public void DisableInteractArrow()
    {
        prevArrowState = interactArrow.activeSelf;
        interactArrow.SetActive(false);
    }
    public void EnableInteractArrow()
    {
        interactArrow.SetActive(prevArrowState);
    }
    public void PlaceInteractArrow(Interactable interact)
    {
        if (interact.showArrow)
        {
            //Debug.Log("showing");
            interactArrow.SetActive(true);
            switch (interact.arrowDirection)
            {
                case (Interactable.ArrowDirection.DOWN):
                    interactArrow.transform.eulerAngles = new Vector3(0, 0, 0);
                    break;
                case (Interactable.ArrowDirection.LEFT):
                    interactArrow.transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                case (Interactable.ArrowDirection.RIGHT):
                    interactArrow.transform.eulerAngles = new Vector3(0, 0, 90);
                    break;
                case (Interactable.ArrowDirection.UP):
                    interactArrow.transform.eulerAngles = new Vector3(0, 0, 180);
                    break;
            }
            interactArrow.transform.localScale = interact.arrowScale;
            interactArrow.transform.position = interact.transform.position + interact.arrowOffset;
            interact.SetActiveMaterial(activeInteractableMaterial);
        }
    }

    public void MakeSound()
    {
        if (isDetectable)
        {
            Collider2D[] colliders;
            if (state == PlayerControl.PlayerState.SNEAK)
            {
                colliders = Physics2D.OverlapCircleAll(transform.position + soundOffset, sneakRadius, stealthLayerMask);
            }
            else
            {
                colliders = Physics2D.OverlapCircleAll(transform.position + soundOffset, walkRadius, stealthLayerMask);
            }
            if (colliders.Length > 0)
            {
                foreach (Collider2D c in colliders)
                {
                    c.gameObject.GetComponent<ISoundListener>().ListenSound(transform.position);
                }
            }
        }
        else
        {
            //Debug.Log("Made sound, but not detectable right now");
        }
    }
    public void PlaySound(int index)
    {
        AudioManager.AudioTrack a = sounds[index];
        if (a != null && a.audioClip != null)
        {
            PlaySound(a.clipName);
        }
    }
    void PlaySound(string key, float vol = 1)
    {
        AudioClip clip = null;
        switch (key)
        {
            case "WALK":
                clip = sounds[0].audioClip;
                break;
            case "JUMP":
                clip = sounds[1].audioClip;
                break;
            case "LAND":
                clip = sounds[2].audioClip;
                break;
            case "INTERACT":
                clip = sounds[3].audioClip;
                break;
        }
        if (clip != null)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            if (state == PlayerState.SNEAK)
            {
                pitch *= sneakPitchMultiplier;
                vol *= sneakVolMultiplier;
            }
            sfxPlayer.pitch = pitch;
            sfxPlayer.PlayOneShot(clip, vol);
        }
    }
    public void Mute()
    {

    }
    public void Unmute()
    {

    }
    void PlayAnimation(PlayerState state)
    {
        this.state = state;
        animator.SetInteger("State", (int)(state));
    }
    bool IsSneaky()
    {
        return forceSneak || isSneaking;
    }
    public bool CanAct(bool checkPhone = true)
    {
        if (debugPause) return false;
        return canMove && state != PlayerState.BUSY && GameManager.Instance.IsOpen() && !PhoneManager.Instance.IsFocusing() && (!checkPhone || !GameManager.Instance.isPhoneFocused);
    }
    void TogglePhone(InputAction.CallbackContext context)
    {
        if (!PhoneManager.Instance.isActiveAndEnabled || !CanAct(false) || 
            GameDialogueManager.Instance.dialogueState != GameDialogueManager.DialogueState.NONE || context.phase != InputActionPhase.Started)
        {
            return;
        }
        Debug.Log(PhoneManager.Instance.phoneState);
        if (GameManager.Instance.isPhoneFocused)
        {
            PhoneManager.Instance.Unfocus();
        }
        else
        {
            PhoneManager.Instance.Focus();
            SetPlayerState(PlayerState.BUSY);
        }
    }
    void Reset(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void Free(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.debugMode)
            return;
        SetPlayerState(PlayerState.NONE);
        GameManager.Instance.inConvo = false;
        GameManager.Instance.inEssay = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            Interactable interact = collision.gameObject.GetComponent<Interactable>();
            //Debug.Log("Contact " + interact.interactableName);
            if (interact != null && interact.isActiveAndEnabled)
            {
                interactList.Add(interact);

                interactIndex = interactList.Count - 1;
                interactSelection = interact;

                PlaceInteractArrow(interact);

                if (interactSelection.startOnCollision)
                {
                    Debug.Log("test");
                    interactSelection.Interact();
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //One possible solution: just do a distance check instead of removing anything
        if (collision.gameObject.CompareTag("Interactable"))
        {
            Interactable interact = collision.gameObject.GetComponent<Interactable>();
            //Debug.Log("Uncontact " + interact.interactableName);
            interactList.Remove(interact);
            interact.SetActiveMaterial();

            if (interactList.Count < 1)
            {
                interactArrow.SetActive(false);
                interactSelection = null;
            }
            else
            {
                PlaceInteractArrow(interactList[interactList.Count - 1]);
                interactSelection = interactList[interactList.Count - 1];
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + soundOffset, sneakRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + soundOffset, walkRadius);

        if (col)
        {
            Bounds colBounds = col.bounds;
            Vector2 colBox = new Vector2(colBounds.size.x - .25f, colBounds.size.y);
            Vector2 colCenter = new Vector2(colBounds.center.x, colBounds.center.y - .05f);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(colCenter, colBox);
        }
    }
}
