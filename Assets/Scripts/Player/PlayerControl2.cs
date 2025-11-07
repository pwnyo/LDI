using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Yarn.Unity;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]

public class PlayerControl2 : MonoBehaviour, ISoundMaker
{
    public static PlayerControl2 Instance { get; private set; }
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
        SIT
    }
    public PlayerState state;
    public bool startSneaky;
    public bool isSmall;

    [Header("Sounds")]
    public AudioManager.AudioTrack[] sounds;

    #region References
    [Header("Input Actions")]
    public PlayerInput input;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction interactAction;
    InputAction sneakAction;
    InputAction phoneAction;
    InputAction resetAction;
    InputAction freeAction;
    [Header("References")]
    public Animator animator;
    public GameObject interactArrow;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer altSpriteRenderer;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private AudioSource sfxPlayer;

    private DialogueRunner dr;
    public float interactMinDistance;
    private List<Interactable> interactList = new List<Interactable>();
    private Interactable interactSelection;
    private int interactIndex;

    #endregion

    #region Sprites
    [Header("Sprites")]
    public FaceSO.SpriteInfo[] altSprites;
    #endregion

    #region Variables
    [Header("Movement")]
    private bool canMove;
    private Vector2 move;
    public float walkSpeed;
    public float sneakSpeed;

    [Header("Fall and Jump")]
    public LayerMask jumpLayerMask;
    public float jumpSpeed;
    public float jumpTime;
    public float landTime;
    float timeSinceJumpInput;
    public bool grounded;
    public float fallFastMultiplier;
    [SerializeField]
    private float fallSpeedActual;
    private bool isFastfall;
    private bool isFalling;

    [Header("Utility")]
    Vector2 velocity;
    [SerializeField]
    Vector2 maxVelocity;

    [SerializeField]
    float moveSpeed;
    [SerializeField]
    float fallSpeed, floatyMultiplier;
    [SerializeField]
    float coyoteDur, jumpBufferDur;
    float coyoteTime, jumpBufferTime, wallJumpBufferTime;
    bool fastFell;
    int jumpCount;

    [SerializeField]
    float groundCheckDistance;
    [SerializeField]
    LayerMask groundLayers;

    [Header("Stealth")]
    public LayerMask stealthLayerMask;
    public Vector3 soundOffset;
    public float sneakRadius;
    public float walkRadius;
    public bool isDetectable;

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            moveAction = input.actions["Move"];
            jumpAction = input.actions["Jump"];
            interactAction = input.actions["Interact"];
            sneakAction = input.actions["Sneak"];
            phoneAction = input.actions["TogglePhone"];
            resetAction = input.actions["Reset"];
            freeAction = input.actions["Free"];
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
        interactIndex = -1;

        canMove = true;
        fallSpeedActual = fallSpeed;
        isFastfall = false;
    }

    // Update is called once per frame
    void Update()
    {
        DebugControls();
        if (!IsOpen())
        {
            Pause();
            return;
        }
            
        CheckGround();
        Jump();
        Move();

        
        if (grounded && interactAction.phase == InputActionPhase.Started)
        {
            if (interactList.Count > 0)
            {
                //Pause();
                SelectInteractable(moveAction.ReadValue<Vector2>().x);

            }
        }
        else if (interactAction.phase == InputActionPhase.Canceled)
        {
            if (interactSelection != null)
            {
                interactSelection.Interact();
                interactArrow.SetActive(false);
                sfxPlayer.PlayOneShot(sounds[3].audioClip, 0.2f);
            }
        }
        if (phoneAction.phase == InputActionPhase.Started)
        {
            Debug.Log(PhoneManager.Instance.phoneState);
            if (PhoneManager.Instance.IsFocused())
            {
                PhoneManager.Instance.UnfocusPhone();
            }
            else
            {
                PhoneManager.Instance.FocusPhone();
            }
        }

        float x = moveAction.ReadValue<Vector2>().x;
        if (Mathf.Abs(x) > 0.1f)
        {
            spriteRenderer.flipX = x < 0;
            SetMoveSpeed();
        }
    }
    void FixedUpdate()
    {
        //transform.position = transform.position + new Vector3(move.x, 0, 0) * Time.fixedDeltaTime;
        Fall(fastFell ? fallSpeed : fallSpeed * floatyMultiplier);
        rb.velocity = velocity;
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxVelocity.x, maxVelocity.x), velocity.y);
    }
    bool IsOpen()
    {
        return canMove && state != PlayerState.BUSY && GameManager.Instance.IsOpen();
    }
    public void PlaySound(int index)
    {
        if (sounds == null || sounds.Length < 1)
            return;
        AudioManager.AudioTrack a = sounds[index];
        if (a != null && a.audioClip != null)
        {
            sfxPlayer.PlayOneShot(a.audioClip);
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
    void DebugControls()
    {
        if (!GameManager.Instance.debugMode)
            return;
        if (resetAction.phase == InputActionPhase.Started)
        {
            GameManager.Instance.FreeControls();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (freeAction.phase == InputActionPhase.Started)
        {
            state = PlayerState.NONE;
            GameManager.Instance.FreeControls();
        }
    }
    void Move()
    {
        velocity.x = moveAction.ReadValue<Vector2>().x * (grounded ? moveSpeed : moveSpeed * sneakSpeed);
        if (grounded)
        {
            if (sneakAction.ReadValue<float>() > 0.2f)
            {
                animator.SetInteger("State", (int)PlayerState.CROUCH);
                //AdjustCollider(true);
            }
            else
            {
                animator.SetInteger("State", (int)PlayerState.NONE);
                //AdjustCollider(false);
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
    void SetMoveSpeed()
    {
        if (!grounded)
            return;
        if (startSneaky || sneakAction.ReadValue<float>() > 0.2f)
        {
            animator.SetInteger("State", (int)PlayerState.SNEAK);
        }
        else
        {
            animator.SetInteger("State", (int)PlayerState.WALK);
        }
        //AdjustCollider(false);
    }
    void Jump()
    {
        if (jumpAction.phase == InputActionPhase.Started)
        {
            jumpBufferTime = jumpBufferDur;
        }
        bool unbuffered = grounded && jumpAction.phase == InputActionPhase.Started; //&& Input.GetKeyDown(KeyCode.Space)
        if (unbuffered || jumpBufferTime > 0)
        {
            bool singleJumped = jumpCount < 1;
            if (singleJumped)
            {
                //Debug.Log(unbuffered ? "jump" : "buffer");
                jumpBufferTime = 0;
                velocity.y = jumpSpeed;
                Debug.Log("jump");
                state = PlayerState.JUMP;
                jumpCount++;
            }
        }

        jumpBufferTime -= Time.deltaTime;
    }
    void Fall(float vel)
    {
        velocity.y = Mathf.Clamp(velocity.y - vel, -maxVelocity.y, maxVelocity.y);

        if (velocity.y < 0)
        {
            state = PlayerState.FALL;
            animator.SetInteger("State", (int)state);
        }
    }
    void Fastfall()
    {
        //TESTING: Removed ground check
        if (!isFastfall)
        {
            //Debug.Log("Fastfalling");
            isFastfall = true;
            //TESTING
            fallSpeedActual = fallSpeed * fallFastMultiplier;
            rb.gravityScale = fallSpeedActual;
        }
    }
    void Land()
    {
        isFalling = false;
        //Debug.Log("Grounded");
        StopAllCoroutines();

        isFastfall = false;
        fallSpeedActual = fallSpeed;
        rb.gravityScale = fallSpeedActual;

        if ((Time.time - timeSinceJumpInput) < jumpBufferTime)
        {
            Debug.Log("buffered jump!");
            //Jump();
        }
        else
        {
            state = PlayerState.LAND;
            animator.SetInteger("State", (int)state);
            //StartCoroutine(ILand());
            //TESTING
            sfxPlayer.PlayOneShot(sounds[2].audioClip);
            state = PlayerState.NONE;
            animator.SetInteger("State", (int)state);
        }
    }

    void CheckGround()
    {
        bool groundCheck = IsGrounded();
        //Debug.Log(grounded);
        if (groundCheck)
        {
            if (!grounded)
                StartCoroutine(LandHelper(3));
        }
        else if (grounded && !groundCheck)
        {
            coyoteTime = coyoteDur;
            //or coyote coroutine
        }
        else if (state == PlayerState.JUMP && jumpAction.phase == InputActionPhase.Canceled)
        {
            //velocity.y = velocity.y < 0 ? velocity.y : 0;
            fastFell = true;
        }

        if (coyoteTime > 0)
            coyoteTime -= Time.deltaTime;
        else
        {
            if (!groundCheck && jumpCount == 0)
            {
                jumpCount++;
                Debug.Log("missed coyote");
            }
        }
        grounded = groundCheck;
    }
    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayers.value);
    }
    IEnumerator LandHelper(int frames)
    {
        int count = 0;
        while (count < frames)
        {
            yield return null;
            count++;
        }
        fastFell = false;
        state = PlayerState.LAND;
        jumpCount = 0;
    }

    public void Pause()
    {
        move = Vector2.zero;
        animator.SetInteger("State", (int)PlayerState.NONE);
    }
    public void AllowMovement(bool setting)
    {
        canMove = setting;
    }
    public IEnumerator StopMovement(float waitTime)
    {
        move = Vector2.zero;
        canMove = false;
        yield return new WaitForSeconds(waitTime);
        canMove = true;
    }
    public void Spawn(Vector3 location, bool setting)
    {
        transform.position = location;
        spriteRenderer.flipX = setting;
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
    //TODO: Should be changed to use another component with events that call this :)
    [YarnCommand("usealt")]
    public void UseAlt(string spriteName)
    {
        foreach (FaceSO.SpriteInfo f in altSprites)
        {
            if (f.Name == spriteName)
            {
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
        }
    }

    public void MakeSound()
    {
        if (isDetectable)
        {
            Collider2D[] colliders;
            if (state == PlayerControl2.PlayerState.SNEAK)
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
    public void Mute()
    {

    }
    public void Unmute()
    {

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

            interactIndex = interactList.Count - 1;

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

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
