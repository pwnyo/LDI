using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

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
        FALLSPIN,
        FLOOR,
        GETUP,
        LEDGEGRAB,
        LEDGEJUMP,
    }
    public enum PlayerSpecialState
    {
        NONE,
        CUTSCENE,
        CUTSCENEPAUSE,
    }
    public PlayerState state;
    public PlayerSpecialState specialState;
    int wiggleCount = 0, wiggleMax = 3;
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
    public Transform parentForShaker;
    public Shaker shaker;
    public Animator animator;
    public GameObject interactArrow;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer altSpriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private AudioSource sfxPlayer;
    public ParticleSystem stepParticles;
    public Vector3 stepOffset;
    public Color psColor;
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
    public FaceSO.SpriteInfo[] altSprites;
    #endregion

    #region Variables
    [Header("Movement")]
    private bool canMove;
    private Vector3 move;
    public float walkSpeed;
    public float sneakSpeed;
    float moveX;
    public Vector3 maxVel;

    [Header("Collider Sizes")]
    public Bounds groundedBounds;
    public Bounds miniGroundedBounds;
    public Bounds standingBounds, crouchingBounds;

    [Header("Fall and Jump")]
    public LayerMask jumpLayerMask;
    public float jumpSpeed;
    public float jumpTime;
    public float landTime;
    public float jumpBufferTime;
    public float jumpLockoutTime;
    public float ledgeGrabLockoutTime;
    float timeSinceJumpInput;
    float timeSinceLastJump;
    float timeSinceLedgeGrab;
    public float fallSpeed;
    public float fallFastMultiplier;
    public Vector3 ledgeGrabCheckSize, ledgeGrabCheckOffset;
    public Vector3 ledgeGrabCeilingCheckSize, ledgeGrabCeilingCheckOffset;
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
        col = GetComponent<BoxCollider2D>();
        dr = FindObjectOfType<DialogueRunner>();

        input = InputHelper.Instance;
        interactIndex = -1;

        canMove = true;
        fallSpeedActual = fallSpeed;
        isFastfall = false;
    }
    void OnEnable()
    {
        RegisterInputs(FindObjectOfType<InputHelper>());
    }
    void OnDisable()
    {
        DeregisterInputs();
    }

    // Update is called once per frame
    void Update()
    {
        if (specialState != PlayerSpecialState.NONE)
        {
            return;
        }
        if (CanAct() || IsStopped())
        {
            CheckMove();
        }
        CheckGround();
        CheckFall();
        CheckAnimation();
    }
    void FixedUpdate()
    {
        if (CanAct() && !IsStopped()) 
        {
            transform.position += move * Time.deltaTime;
        }
    }
    public void RegisterInputs(InputHelper inputHelper)
    {
        input = inputHelper;
        input.jumpAction.started += Jump; //PS4 X
        input.jumpAction.canceled += Fastfall;
        input.sneakAction.started += Sneak; //
        input.sneakAction.canceled += Sneak;
        input.moveAction.performed += Wiggle;
        //input.moveAction.canceled += Move;
        input.interactAction.started += Interact;
        input.interactAction.canceled += Interact;
        input.phoneAction.started += TogglePhone;
        input.freeAction.started += Free;
        input.resetAction.started += Reset;
    }
    public void DeregisterInputs()
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
        input = null;
    }
    public void SetPlayerState(PlayerState setting)
    {
        state = setting;
        //Debug.Log(setting);
    }

    #region Movement Functions
    void CheckAnimation()
    {
        move.x = IsSneaky() && groundState == GroundState.GROUNDED ? moveX * sneakSpeed : moveX * walkSpeed;
        if (groundState != GroundState.GROUNDED)
        {
            if (rb.velocity.y < 0)
            {
                PlayAnimation(PlayerState.JUMP);
            }
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
        if (moveX != 0)
        {
            spriteRenderer.flipX = moveX < 0;
        }
    }
    void Wiggle(InputAction.CallbackContext context)
    {
        if (specialState != PlayerSpecialState.CUTSCENE || (state != PlayerState.FLOOR && state != PlayerState.GETUP))
        {
            return;
        }
        float x = context.ReadValue<Vector2>().x;
        if (x == 0)
        {
            return;
        }
        Shake();
        wiggleCount++;
        Debug.Log($"wiggle count: {wiggleCount}");
        if (wiggleCount >= wiggleMax)
        {
            if (state == PlayerState.FLOOR)
            {
                PlayAnimation(PlayerState.GETUP, true);
            }
            else
            {
                PlayAnimation(PlayerState.NONE, true);
                AllowMovement(true);
            }
            wiggleCount = 0;
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
                col.offset = crouchingBounds.center;
                col.size = crouchingBounds.size;
            }
            else
            {
                col.offset = standingBounds.center;
                col.size = standingBounds.size;
            }
        }
    }
    void Jump(InputAction.CallbackContext context)
    {
        Jump();
    }
    void Jump()
    {
        if (!CanAct())
        {
            return;
        }
        timeSinceJumpInput = Time.time;
        if (groundState == GroundState.GROUNDED || state == PlayerState.LEDGEGRAB)
        {
            Debug.Log("Trying to jump!");
            timeSinceLastJump = Time.time;
            groundState = GroundState.RISING;
            if (state == PlayerState.LEDGEGRAB)
            {
                timeSinceLedgeGrab = Time.time;
                rb.gravityScale = fallSpeed;
                if (!IsStopped())
                {
                    PlayAnimation(PlayerState.LEDGEJUMP);
                }
                else
                {
                    PlayAnimation(PlayerState.JUMP);
                }
            }
            else
            {
                PlayAnimation(PlayerState.JUMP);
            }
            PlaySound("JUMP", 0.5f);
            rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        }
    }
    void CheckGround()
    {
        if (HasJustJumped())
        {
            return;
        }
        bool hitGround, atLedge, movingTowardLedge;

        Bounds bounds = isSmall ? miniGroundedBounds : groundedBounds;
        Vector2 colBox = new Vector2(bounds.size.x, bounds.size.y);
        Vector2 colCenter = new Vector3(bounds.center.x, bounds.center.y) + transform.position;
        Collider2D colliderLand = Physics2D.OverlapBox(colCenter, colBox, 0, jumpLayerMask);
        //Debug.Log(raycastHit != null);
        hitGround = colliderLand != null;

        Vector3 offsetA = GetOffsetForPlayerPosAndFlip(ledgeGrabCheckOffset);
        Vector3 offsetB = GetOffsetForPlayerPosAndFlip(ledgeGrabCeilingCheckOffset);
        Collider2D colliderLedge = Physics2D.OverlapBox(GetOffsetForPlayerPosAndFlip(ledgeGrabCheckOffset), ledgeGrabCheckSize, 0, jumpLayerMask);
        Collider2D colliderCeiling = Physics2D.OverlapBox(GetOffsetForPlayerPosAndFlip(ledgeGrabCeilingCheckOffset), ledgeGrabCeilingCheckSize, 0, jumpLayerMask);
        if (colliderCeiling)
        {
            Debug.Log($"hit ceiling: {colliderCeiling.name}");
        }
        atLedge = colliderLedge != null && colliderCeiling == null;
        movingTowardLedge = spriteRenderer.flipX ? (moveX < 0) : moveX > 0;

        if (atLedge)
        {
            if (!HasJustReleasedLedge() && movingTowardLedge &&
                   groundState != GroundState.GROUNDED && state != PlayerState.LEDGEGRAB)
            {
                Debug.Log($"{offsetA}/{offsetB}");
                LedgeGrab();
            }
        }
        else
        {
            Debug.Log("released ledge");
            if (state == PlayerState.LEDGEGRAB)
            {
                groundState = GroundState.FALLING;
                timeSinceLedgeGrab = Time.time;
            }
            else
            {
                if (state != PlayerState.JUMP && state != PlayerState.LEDGEJUMP)
                {
                    state = PlayerState.NONE;
                }
            }
        }
        if (hitGround)
        {
            if (groundState != GroundState.GROUNDED)
            {
                Land();
            }
        }
    }
    void CheckFall()
    {
        if (groundState != GroundState.GROUNDED)
        {
            if (rb.velocity.y < -0.05 && state != PlayerState.LEDGEJUMP)
            {
                groundState = GroundState.FALLING;
                PlayAnimation(PlayerState.JUMP);
            }
            if (!isFastfall && input.jumpAction.phase == InputActionPhase.Waiting)
            {
                Fastfall();
            }
        }
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxVel.x, maxVel.x), Mathf.Clamp(rb.velocity.y, -maxVel.y, maxVel.y));
    }
    void Fastfall(InputAction.CallbackContext context)
    {
        Fastfall();
    }
    void Fastfall()
    {
        if (groundState != GroundState.GROUNDED && !isFastfall && state != PlayerState.LEDGEGRAB)
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
        PlaySound("LAND", 0.5f);

        if (!HasJustJumped() && (Time.time - timeSinceJumpInput) < jumpBufferTime)
        {
            Debug.Log("buffered jump! currently disabled");
            //Jump();
        }
        else
        {
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
    bool HasJustJumped()
    {
        return (Time.time - timeSinceLastJump) < jumpLockoutTime;
    }
    bool HasJustReleasedLedge()
    {
        return (Time.time - timeSinceLedgeGrab) < ledgeGrabLockoutTime;
    }
    void LedgeGrab()
    {
        Debug.Log("grabbed ledge");
        isFastfall = false;
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        PlaySound("LAND", 0.5f);
        PlayAnimation("LEDGEGRAB");
    }
    /// <summary>
    /// Stops player movement and resets animation to default (NONE)
    /// </summary>
    public void Pause()
    {
        moveX = 0;
        move = Vector2.zero;
        PlayAnimation(PlayerState.NONE);
    }
    bool IsStopped()
    {
        return Mathf.Abs(moveX) < 0.125f;
    }
    [YarnCommand("allowMovement")]
    public void AllowMovement(string param)
    {
        bool.TryParse(param, out bool setting);
        AllowMovement(setting);
    }
    public void AllowMovement(bool setting)
    {
        Debug.Log($"allowing movement? {setting}");
        canMove = setting;
        if (canMove)
        {
            specialState = PlayerSpecialState.NONE;
        }
        else
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
        AllowMovement(setting);
        Debug.Log("showing player " + setting);
        interactArrow.GetComponent<SpriteRenderer>().enabled = setting;
    }
    [YarnCommand("usealt")]
    public void UseAlt(string spriteName)
    {
        foreach (FaceSO.SpriteInfo f in altSprites)
        {
            if (f.Name == spriteName) {
                Debug.Log($"showing alt sprite {spriteName}");
                altSpriteRenderer.sprite = f.Sprite;
                altSpriteRenderer.enabled = true;
                spriteRenderer.enabled = false;
            }
        }
    }
    [YarnCommand("usebase")]
    public void UseBase()
    {
        foreach (FaceSO.SpriteInfo f in altSprites)
        {
            altSpriteRenderer.sprite = f.Sprite;
            altSpriteRenderer.enabled = false;
            spriteRenderer.enabled = true;
        }
    }
    void Interact(InputAction.CallbackContext context)
    {
        if (!CanAct() || groundState != GroundState.GROUNDED)
        {
            return;
        }
        if (context.phase == InputActionPhase.Started)
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
                DisableInteractArrow();
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
        else
        {
            interactIndex = (int)f;
        }
        PlaceInteractArrow(interactList[interactIndex]);
    }
    public void DisableInteractArrow(bool savePrevState = false)
    {
        if (savePrevState)
        {
            prevArrowState = interactArrow.activeSelf;
        }
        interactArrow.SetActive(false);
    }
    public void EnableInteractArrow(bool usePrevState = false)
    {
        if (usePrevState)
        {
            interactArrow.SetActive(usePrevState && prevArrowState);
        }
        else
        {
            interactArrow.SetActive(true);
        }
    }
    public void PlaceInteractArrow(Interactable interact)
    {
        if (interact.showArrow)
        {
            string interactableName = string.IsNullOrEmpty(interact.interactableName) ? interact.gameObject.name : interact.interactableName;
            Debug.Log($"showing arrow for { interactableName }");
            EnableInteractArrow();
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
            if (state == PlayerState.SNEAK)
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
                ShowParticles();
                break;
            case "JUMP":
                clip = sounds[1].audioClip;
                ShowParticles();
                break;
            case "LAND":
                clip = sounds[2].audioClip;
                ShowParticles();
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
    [YarnCommand("shake")]
    public void Shake()
    {
        transform.parent = parentForShaker;
        shaker.Shake();
    }
    void ShowParticles()
    {
        if (stepParticles)
        {
            stepParticles.transform.position = transform.position + new Vector3(stepOffset.x * (spriteRenderer.flipX ? 1 : -1), stepOffset.y);
            ParticleSystem.MainModule mod = stepParticles.main;
            mod.startColor = psColor;
            stepParticles.Play();
        }
    }
    public void Mute()
    {

    }
    public void Unmute()
    {

    }

    [YarnCommand("setSpecialState")]
    public void SetSpecialState(string state)
    {
        System.Enum.TryParse(state, out PlayerSpecialState sps);
        specialState = sps;
    }
    [YarnCommand("playAnimation")]
    public void PlayAnimation(string state)
    {
        System.Enum.TryParse(state, out PlayerState s);
        PlayAnimation(s, true);
    }
    void PlayAnimation(PlayerState state, bool force = false)
    {
        //manually controlled
        if (specialState != PlayerSpecialState.NONE && !force)
        {
            return;
        }
        if (this.state != state)
        {
            //Debug.Log($"changing to state {state}");
        }
        this.state = state;
        animator.SetInteger("State", (int)(state));
    }
    bool IsSneaky()
    {
        return (forceSneak || isSneaking) && groundState == GroundState.GROUNDED;
    }
    /// <summary>
    /// True if the player can take all normal actions (e.g. moving, jumping, sneaking)
    /// False if the player cannot move (e.g. cutscene, phone is up, in dialogue)
    /// </summary>
    /// <param name="checkPhone"></param>
    /// <returns></returns>
    public bool CanAct(bool checkPhone = true)
    {
        if (debugPause) return false;
        if (input == null) return false;
        if (specialState != PlayerSpecialState.NONE) return false;
        return canMove && GameManager.Instance.IsOpen() && !PhoneManager.Instance.IsAnimating() && (!checkPhone || !PhoneManager.Instance.IsFocused());
    }
    void TogglePhone(InputAction.CallbackContext context)
    {
        if (!PhoneManager.Instance.isActiveAndEnabled || !CanAct(false) || groundState != GroundState.GROUNDED ||
            GameDialogueManager.Instance.dialogueState != GameDialogueManager.DialogueState.NONE || context.phase != InputActionPhase.Started)
        {
            return;
        }
        Debug.Log(PhoneManager.Instance.phoneState);
        Pause();
        if (PhoneManager.Instance.IsFocused())
        {
            PhoneManager.Instance.UnfocusPhone();
        }
        else
        {
            PhoneManager.Instance.FocusPhone();
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
        GameManager.Instance.FreeControls();
        PhoneManager.Instance.FreeControls();
    }
    Vector3 GetOffsetForPlayerPosAndFlip(Vector3 a)
    {
        return new Vector3(a.x * (spriteRenderer.flipX ? -1 : 1), a.y, a.z) + transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            Interactable interact = collision.gameObject.GetComponent<Interactable>();
            //Debug.Log("Contact " + interact.interactableName);
            if (interact != null && interact.isActiveAndEnabled && GameManager.Instance.IsOpen())
            {
                interactList.Add(interact);

                interactIndex = interactList.Count - 1;
                interactSelection = interact;

                PlaceInteractArrow(interact);

                if (interactSelection.startOnCollision)
                {
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
                DisableInteractArrow();
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
            Bounds bounds = isSmall ? miniGroundedBounds : groundedBounds;
            Vector2 colBox = new Vector2(bounds.size.x, bounds.size.y);
            Vector2 colCenter = new Vector3(bounds.center.x, bounds.center.y) + transform.position;
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(colCenter, colBox);
        }

        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(GetOffsetForPlayerPosAndFlip(ledgeGrabCheckOffset), ledgeGrabCheckSize);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(GetOffsetForPlayerPosAndFlip(ledgeGrabCeilingCheckOffset), ledgeGrabCeilingCheckSize);
    }
}
