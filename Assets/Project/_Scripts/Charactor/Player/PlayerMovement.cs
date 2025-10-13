using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;




public class PlayerStates
{
    public enum MovementStates 
    { 
        Die,
        Idle, 
        Running, 
        Jumping, 
        Falling, 
        Sliding, 
        Grabbing, 
        Dashing
    }
}

public class PlayerMovement : CharacterMovement
{
    public PlayerStates playerState { get; protected set; }
    public MyStateManager<PlayerStates.MovementStates> movementState;

    public PlayerData data;

    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D boxCollider { get; private set; }

    public Animator animator;
    public Transform CameraTarget;


    [Header("움직임 제어")]
    public bool CanWallJumping = true;
    public bool CanWallSliding = true;
    public bool CanWallGrabbing = true;
    public bool CanDasing = true;
    public bool ApplyGravityOnDeath;

    [Header("VFX")]
    public ParticleSystem jumpStartVFX;
    public ParticleSystem jumpAirVFX;
    public ParticleSystem jumpLandingVFX;
    public PlaySound walkSound;
    public PlaySound jumpSound;
    public PlaySound landingSound;

    public bool isAttacking { get; set; }
    public bool isFacingRight { get; private set; }
    public bool isJumping { get; private set; }
    public bool isWallJumping { get; private set; }
    public bool isSliding { get; private set; }
    public bool isWallGrabbing { get; private set; }
    public bool isDashing { get; private set; }
    public bool checkOneWayPlatformBelow { get; private set; }
    public bool doKnockback { get; private set; }
    public bool isControlSleep { get; private set; }


    // 이동 플랫폼
    public Transform platformTransform { get; set; }
    public Vector3 lastPlatformPosition { get; set; }
    public Vector2 platformDirection { get; set; }
    public bool isOnMovingPlatform { get; set; }
    public bool isJumpingOnMovingPlatform { get; set; }

    // 점프 패드
    public bool isOnJumpPad { get; set; }
    public Vector2 padDirection { get; set; }
    public float padForce { get; set; }
    private float lastOnJumpPadTime;

    // 타이머
    public float lastOnGroundTime { get; private set; }
    public float lastOnWallTime { get; private set; }
    public float lastOnWallRightTime { get; private set; }
    public float lastOnWallLeftTime { get; private set; }
    public float lastOnGrabTime { get; private set; }

    // 점프
    private bool isJumpCut;
    private bool isJumpFalling;
    private float m_jumpDisableGroundCheckTime = 0.1f;
    private float m_jumpEndIgnoreGroundUntil = -1f;
    public float jumpsLeft { get; private set; }
    private bool jumpRefilling;

    // 벽 점프
    private float wallJumpStartTime;
    private int lastWallJumpDirection;

    // 벽 이동
    private int lastGrabDirection;
    private bool isLookingOther;

    // 대쉬
    private enum DashDirection { TwoDirections, FourDirections, EightDirections}
    [Header("대쉬 방향")]
    [SerializeField] private DashDirection dashDirection;

    public int dashesLeft { get; private set; }
    private bool dashRefilling;
    private Vector2 lastDashDirection;
    private bool isDashAttacking;

    // 입력
    private Vector2 moveInput;
    private int lastMoveDirection = 0;

    public float lastPressedMoveInputTime { get; private set; }
    public float lastPressedJumpTime { get; private set; }
    public float lastPressedGrabTime { get; private set; }
    public float lastPressedDashTime { get; private set; }

    [Header("콜라이더 확인")]
    public int numberOfVerticalRays = 4;

    [SerializeField] private Transform headCheckPoint;
    [SerializeField] private Vector2 headCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]

    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 grabCheckSize = new Vector2(0.03f, 0.49f);
    [Space(5)]

    [Header("모서리 감지")]
    [SerializeField] private Vector2 frontDownRay;
    //[SerializeField] private Vector2 front_top_ray;
    //[SerializeField] private Vector2 back_top_ray;
    [SerializeField] public float rayDistance = 0.55f; // Ray의 길이
    private RaycastHit2D hit;
    //private bool isAtEdge = false;
    public float climbForce { get; private set; } = 12f;

    [Header("레이어")]
    public LayerMask platform;
    public LayerMask movingPlatform;
    public LayerMask onewayPlatform;

    [SerializeField] private LayerMask groundLayer;
    [MyReadOnly] public LayerMask m_currentPlatform;

    [Header("이벤트")]
    public bool SendStateChangeEvents = true;


    public Vector2 speed { get; private set; }
    private Vector2 m_currentPosition;
    private Vector2 m_previousPosition;

    private Health m_health;
    private Vector2 m_bounds;
    private Vector2 m_boundsCenter;
    private Vector2 m_boundsTopLeftCorner;
    private Vector2 m_boundsBottomLeftCorner;
    private Vector2 m_boundsTopRightCorner;
    private Vector2 m_boundsBottomRightCorner;
    private float m_boundsWidth;
    private float m_boundsHeight;
    private RaycastHit2D[] m_belowHitsStorage;
    private Vector2 m_virticalRaycastFromLeft;
    private Vector2 m_virticalRaycastToRight;
    public bool isOnSlope;
    public float m_belowSlopeAngle;
    public Vector2 m_currentSlopeDirection;
    private RaycastHit2D m_stickRaycast;

    private PlayerSpriteColorInversion m_spriteColorInversion;
    private bool m_playSFX;
    private float m_playWalkSFXTimer;



    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        m_health = GetComponent<Health>();
        m_spriteColorInversion = GetComponent<PlayerSpriteColorInversion>();

        movementState = new MyStateManager<PlayerStates.MovementStates>(this.gameObject, SendStateChangeEvents);
        movementState.StateChange(PlayerStates.MovementStates.Idle);

        playerState = new PlayerStates();

        //임시
        //CinemachineCamera cam = FindAnyObjectByType<CinemachineCamera>();
        //cam.Target.TrackingTarget = transform;

        m_belowHitsStorage = new RaycastHit2D[numberOfVerticalRays];

        groundLayer |= platform;
        groundLayer |= movingPlatform;
        groundLayer |= onewayPlatform;
    }
    private void Start()
    {   
        Initialization();
        isFacingRight = true;
    }

    private void OnEnable()
    {
        Initialization();
    }

    private void Initialization()
    {
        //var cam = FindFirstObjectByType<CinemachineCamera>();
        //cam.Target.TrackingTarget = transform;

        moveInput = Vector2.zero;

        if (movementState != null)
        {
            movementState.StateChange(PlayerStates.MovementStates.Idle);
        }

        SetGravityScale(data.gravityScale);

        isJumping = false;
        isJumpFalling = false;
        isJumpCut = false;
        isDashing = false;
        isStunned = false;
        doKnockback = false;

        m_currentPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.HasInstance)
        {
            if (GameManager.Instance.paused)
            {
                m_health.immuneToDamage = true;

                if (m_spriteColorInversion != null)
                {
                    m_spriteColorInversion.enabled = false;
                }

                return;
            }
            else
            {
                m_health.immuneToDamage = false;

                if (m_spriteColorInversion != null)
                {
                    m_spriteColorInversion.enabled = true;
                }
            }
        }

        if (doKnockback)
        {
            //StartCoroutine(nameof(PerformControllSleep), 0.25f);
            //doKnockback = false;
            return;
        }

        if (movementState.currentState == PlayerStates.MovementStates.Die)
        {
            //Debug.Log("사망");

            if (false == ApplyGravityOnDeath)
            {
                SetGravityScale(0);
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                SetGravityScale(data.gravityScale * data.fastFallGravityMult);

                rb.linearVelocity =
                    new Vector2(0, Mathf.Max(rb.linearVelocity.y, -data.maxFastFallSpeed));
            }

            return;
        }

        m_previousPosition = m_currentPosition;

        #region TIMERS
        lastOnGroundTime -= Time.deltaTime;
        lastOnWallTime -= Time.deltaTime;
        lastOnWallRightTime -= Time.deltaTime;
        lastOnWallLeftTime -= Time.deltaTime;

        lastOnJumpPadTime -= Time.deltaTime;

        lastPressedMoveInputTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;
        lastPressedDashTime -= Time.deltaTime;
        lastPressedGrabTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        if (false == isControlSleep)
        {
            if (isStunned)
            {
                moveInput.x = 0;

            }
            else
            {
                int currentDirection = 0;

                moveInput.x = Input.GetAxisRaw("Horizontal");
                moveInput.y = Input.GetAxisRaw("Vertical");

                if (moveInput.x != 0)
                {
                    CheckDirectionToFace(moveInput.x > 0);

                    currentDirection = (moveInput.x > 0) ? 1 : -1;
                }

                if (true == isWallGrabbing && false == isLookingOther)
                {
                    CheckDirectionToFace(lastGrabDirection == 1);
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    OnJumpInput();
                }

                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    OnJumpUpInput();
                }

                if (Input.GetKeyDown(KeyCode.X) && false == data.doDoubleTap)
                {
                    OnDashInput();
                }

                if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) &&
                    false == isDashing && data.doDoubleTap)
                {
                    bool sameDirection = (currentDirection == lastMoveDirection) ? true : false;

                    if (Time.time - lastPressedMoveInputTime <= data.doubleTapThreshold && sameDirection)
                    {
                        OnDashInput();
                        lastPressedMoveInputTime = -1f;
                        lastMoveDirection = 0;
                    }
                    else
                    {
                        lastPressedMoveInputTime = Time.time;
                        lastMoveDirection = currentDirection;
                    }
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    checkOneWayPlatformBelow = true;
                }

                if (Input.GetKey(KeyCode.C))
                {
                    OnGrabInput();
                }
            }
        }
        #endregion

        #region COLLISION CHECKS
        SetRaysParameters();
        if (false == isDashing)
        {
            if (Time.time > m_jumpEndIgnoreGroundUntil)
            {
                CastRaysBelow();

                if (checkOneWayPlatformBelow)
                {
                    //Collider2D oneway = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, onewayPlatform);

                    //if (oneway != null)
                    //{
                    //    StartCoroutine(DownJump(oneway));
                    //}
                }
            }

            //if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight)
            //    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight)) &&
            //    false == isJumping && false == isWallJumping)
            //{
            //    //Debug.Log("오른쪽 벽 확인");
            //    lastOnWallRightTime = data.coyoteTime;
            //}

            //if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight)
            //    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight)) &&
            //    false == isJumping && false == isWallJumping)
            //{
            //    //Debug.Log("왼쪽 벽 확인");
            //    lastOnWallLeftTime = data.coyoteTime;
            //}

            //lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }

        checkOneWayPlatformBelow = false;
        //EdgeDetection();

        #endregion

        #region JUMP CHECK
        if (rb.linearVelocity.y < 0)
        {
            isJumping = false;
            isJumpFalling = true;
        }

        if (true == isWallJumping &&
            Time.time - wallJumpStartTime > data.wallJumpTime)
        {
            isWallJumping = false;
        }

        if (lastOnGroundTime > 0 && false == isJumping && false == isWallJumping)
        {
            isJumpCut = false;
            isJumpFalling = false;
        }

        if (false == isOnMovingPlatform)
        {
            isJumpingOnMovingPlatform = false;
        }

        if (lastOnGroundTime > 0 &&
            (true == isJumping || true == isJumpFalling))
        {
            isJumping = false;
            isJumpFalling = false;
        }

        if (false == isDashing)
        {
            // 점프
            if ((true == CanJump() || (true == isWallGrabbing && 0 < lastOnGrabTime && false == isLookingOther)) &&
                lastPressedJumpTime > 0)
            {
                if (true == isWallGrabbing)
                {
                    lastOnGrabTime -= (data.grabStamina * 0.25f);
                }

                if (true == isJumpingOnMovingPlatform)
                {
                    //Debug.Log("이동 플랫폼에서 점프");
                    Jump(data.jumpForce, platformDirection);
                }
                else
                {
                    //Debug.Log("일반 점프");
                    Jump(data.jumpForce);
                }
            }
            // 벽 점프
            else if ((true == CanWallJump() || (true == isWallGrabbing && 0 < lastOnGrabTime && true == isLookingOther))
                && lastPressedJumpTime > 0)
            {
                lastWallJumpDirection = (lastOnWallRightTime > 0) ? -1 : 1;

                WallJump(lastWallJumpDirection);
            }
        }

        if (true == isOnJumpPad && lastOnJumpPadTime < 0 && padDirection != Vector2.zero)
        {
            isJumping = true;
            isJumpFalling = false;

            jumpsLeft = data.jumpAmount;
            StartCoroutine(nameof(RefillDash), 1);

            lastOnJumpPadTime = data.jumpInputBufferTime;

            if (padDirection == Vector2.up)
            {
                rb.linearVelocity = moveInput;

                float force = Mathf.Clamp(data.jumpForce * padForce, 20, 80);
                Jump(force);
            }
            else
            {
                lastWallJumpDirection = (padDirection == Vector2.right) ? 1 : -1;

                rb.linearVelocity = padDirection;
                WallJump(lastWallJumpDirection);
            }

            isOnJumpPad = false;
            padDirection = Vector2.zero;
        }
        #endregion

        #region SLIDE CHECK
        if (true == CanSlide() &&
            ((lastOnWallLeftTime > 0 && moveInput.x < 0) || (lastOnWallRightTime > 0 && moveInput.x > 0)))
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }
        #endregion

        #region GRAB CHECK
        if (true == CanGrab() && 0 < lastPressedGrabTime && false == isDashing)
        {
            if (false == isWallGrabbing && 0 < lastOnWallTime && 0 < lastOnGrabTime)
            {
                isWallGrabbing = true;

                lastGrabDirection = 0 < lastOnWallRightTime ? 1 : -1;
            }
            else if (true == isWallGrabbing && 0 < lastOnGrabTime)
            {
                isWallGrabbing = true;

                if ((lastGrabDirection > 0 && moveInput.x < 0) || (lastGrabDirection < 0 && moveInput.x > 0))
                {
                    isLookingOther = moveInput.y == 0;
                }
                else
                {
                    isLookingOther = false;
                }
            }
            else
            {
                isWallGrabbing = false;
            }
        }
        else
        {
            isWallGrabbing = false;
            lastGrabDirection = 0;
        }
        #endregion

        #region DASH CHECK
        if (true == CanDash() &&
            lastPressedDashTime > 0)
        {
            if (true == isWallGrabbing)
            {
                isWallGrabbing = false;
                lastGrabDirection = 0;
            }

            Sleep(data.dashSleepTime);

            if (moveInput != Vector2.zero)
            {
                lastDashDirection = moveInput;

                switch (dashDirection)
                {
                    case DashDirection.TwoDirections:
                        lastDashDirection = isFacingRight ? Vector2.right : Vector2.left;
                        break;

                    case DashDirection.FourDirections:
                        if (moveInput.x != 0)
                        {
                            lastDashDirection.y = 0;
                        }
                        else
                        {
                            lastDashDirection.x = 0;
                        }
                        break;

                    case DashDirection.EightDirections:
                        break;
                }
            }
            else
            {
                lastDashDirection = isFacingRight ? Vector2.right : Vector2.left;
            }

            StartCoroutine(nameof(StartDash), lastDashDirection);
        }
        #endregion

        #region GRAVITY
        if (false == isDashAttacking)
        {
            if (isSliding)
            {
                SetGravityScale(0);
            }
            else if (isWallGrabbing && false == isJumping && false == isWallJumping)
            {
                SetGravityScale(0);
            }
            else if (rb.linearVelocity.y < 0 && moveInput.y < 0)
            {
                SetGravityScale(data.gravityScale * data.fastFallGravityMult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFastFallSpeed));
            }
            else if (isJumpCut)
            {
                SetGravityScale(data.gravityScale * data.jumpCutGravityMult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFallSpeed));
            }
            else if ((isJumping || isWallJumping || isJumpFalling) &&
                Mathf.Abs(rb.linearVelocity.y) < data.jumpHangTimeThreshold)
            {
                SetGravityScale(data.gravityScale * data.jumpHangGravityMult);
            }
            else if (rb.linearVelocity.y < 0)
            {
                SetGravityScale(data.gravityScale * data.fallGravityMult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
        #endregion

        #region STATE CHANGE
        //animator.SetBool("isAttacking", false);
        //animator.SetBool("isDashing", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);

        if (isDashing)
        {
            movementState.StateChange(PlayerStates.MovementStates.Dashing);
            //animator.SetBool("isDashing", true);
        }
        else if (isJumping)
        {
            movementState.StateChange(PlayerStates.MovementStates.Jumping);
            animator.SetBool("isJumping", true);
        }
        else if (isJumpFalling)
        {
            movementState.StateChange(PlayerStates.MovementStates.Falling);
            animator.SetBool("isFalling", true);
        }
        else if (moveInput.x != 0 && lastOnGroundTime > 0)
        {
            movementState.StateChange(PlayerStates.MovementStates.Running);
            animator.SetBool("isRunning", true);

            m_playWalkSFXTimer -= Time.deltaTime;

            if (m_playWalkSFXTimer < 0f)
            {
                if (walkSound != null && false == m_playSFX)
                {
                    walkSound.PlaySoundFX();
                    m_playSFX = true;
                }
            }
        }
        else
        {
            movementState.StateChange(PlayerStates.MovementStates.Idle);
        }

        if (movementState.currentState != PlayerStates.MovementStates.Running)
        {
            if (walkSound != null && m_playSFX)
            {
                walkSound.StopSoundFX();
                m_playSFX = false;
                m_playWalkSFXTimer = 0.5f;
            }        
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (movementState.currentState == PlayerStates.MovementStates.Die)
        {
            return;
        }

        if (isOnMovingPlatform)
        {
            OnMovingPlatform();
        }

        if (false == isDashing)
        {
            if (isWallJumping)
            {
                Run(data.wallJumpRunLerp);
            }
            else if (isWallGrabbing && 0 < lastOnGrabTime)
            {
                //Run(0);
            }
            else 
            {
                Run(1);
            }
        }
        else if (false == isDashAttacking)
        {
            Run(data.dashEndRunLerp);
        }

        if (true == isSliding || true == isWallGrabbing)
        {
            Slide();
        }

        m_currentPosition = transform.position;
        speed = m_currentPosition - m_previousPosition;
    }

    #region INPUT CALLBACKS
    public void OnJumpInput()
    {
        lastPressedJumpTime = data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if ((CanJumpCut() || CanWallJumpCut()))
        {
            isJumpCut = true;
        }
    }

    public void OnDashInput()
    {
        lastPressedDashTime = data.dashInputBufferTime;
    }

    public void OnGrabInput()
    {
        lastPressedGrabTime = data.grabInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float _scale)
    {
        rb.gravityScale = _scale;
    }

    private void Sleep(float _duration)
    {
        StartCoroutine(nameof(PerformSleep), _duration);
    }

    private IEnumerator PerformSleep(float _duration)
    {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(_duration);

        Time.timeScale = 1;
    }

    private void ControllSleep(float _duration)
    {
        StartCoroutine(nameof(PerformControllSleep), _duration);
    }

    private IEnumerator DownJump(Collider2D _col)
    {
        Physics2D.IgnoreCollision(boxCollider, _col, true);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(boxCollider, _col, false);
    }

    private IEnumerator PerformControllSleep(float _duration)
    {
        isControlSleep = true;

        yield return new WaitForSecondsRealtime(_duration);

        isControlSleep = false;
    }

    public void RespawnAt(Transform _spawnPoint, bool _facingDirection)
    {
        ControllSleep(0.5f);

        Vector3 scale = transform.localScale;
        scale.x = _facingDirection ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        isFacingRight = _facingDirection;

        transform.position = _spawnPoint.position;

        m_health.ResetHealthToMaxHealth();
        m_health.Revive();

        Initialization();
    }

    private IEnumerator PlayVFX(ParticleSystem _ps, float _delay)
    {
        if (_ps == null)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(_delay);
        _ps.Play();
    }
    #endregion

    #region COLLISION METHODS
    private void SetRaysParameters()
    {
        //Debug.Log(boxCollider.size);
        float x = boxCollider.size.x;
        float y = boxCollider.size.y;

        float right = x * 0.5f;
        float left = -x * 0.5f;
        float top = y * 0.5f;
        float bottom = -y * 0.5f;


        m_boundsCenter = boxCollider.bounds.center;

        m_boundsTopLeftCorner.x = left;
        m_boundsTopLeftCorner.y = top;

        m_boundsBottomLeftCorner.x = left;
        m_boundsBottomLeftCorner.y = bottom;

        m_boundsTopRightCorner.x = right;
        m_boundsTopRightCorner.y = top;

        m_boundsBottomRightCorner.x = right;
        m_boundsBottomRightCorner.y = bottom;

        m_boundsTopLeftCorner = transform.TransformPoint(m_boundsTopLeftCorner);
        m_boundsBottomLeftCorner = transform.TransformPoint(m_boundsBottomLeftCorner);
        m_boundsTopRightCorner = transform.TransformPoint(m_boundsTopRightCorner);
        m_boundsBottomRightCorner = transform.TransformPoint(m_boundsBottomRightCorner);

        m_boundsWidth = Vector2.Distance(m_boundsTopLeftCorner, m_boundsTopRightCorner);
        m_boundsHeight = Vector2.Distance(m_boundsTopLeftCorner, m_boundsBottomLeftCorner);
    }

    private void CastRaysBelow()
    {
        m_virticalRaycastFromLeft = (m_boundsBottomLeftCorner + m_boundsTopLeftCorner) * 0.5f;
        m_virticalRaycastToRight = (m_boundsBottomRightCorner + m_boundsTopRightCorner) * 0.5f;
        m_virticalRaycastFromLeft += (Vector2)transform.up * 0.01f;
        m_virticalRaycastToRight += (Vector2)transform.up * 0.01f;

        if (m_belowHitsStorage.Length != numberOfVerticalRays)
        {
            m_belowHitsStorage = new RaycastHit2D[numberOfVerticalRays];
        }

        float smallestDistance = float.MaxValue;
        int smallestDistanceIndex = 0;
        bool hitConnected = false;
        for (int i = 0; i < numberOfVerticalRays; i++)
        {
            Vector2 rayOriginPoint = Vector2.Lerp(m_virticalRaycastFromLeft, m_virticalRaycastToRight, (float)i / (numberOfVerticalRays - 1));
            rayOriginPoint.y = m_boundsBottomLeftCorner.y + 0.001f;

            m_belowHitsStorage[i] = MyDebug.Raycast(rayOriginPoint, Vector2.down, 0.2f, groundLayer, Color.blue, true);

            float distance = MyMaths.DistanceBetweenPointAndLine(m_belowHitsStorage[i].point, m_virticalRaycastFromLeft, m_virticalRaycastToRight);

            if (m_belowHitsStorage[i])
            {
                hitConnected = true;

                if (m_belowHitsStorage[i].distance < smallestDistance)
                {
                    smallestDistance = m_belowHitsStorage[i].distance;
                    smallestDistanceIndex = i;
                }
            }

            if (distance < 0.0001f)
            {
                break;
            }
        }

        if (hitConnected)
        {
            LayerMask layer = m_belowHitsStorage[smallestDistanceIndex].collider.gameObject.layer;

            if (false == MyLayers.LayerInLayerMask(layer, groundLayer))
            {
                return;
            }

            if (isJumpFalling)
            {
                if (jumpLandingVFX != null && false == jumpLandingVFX.isPlaying)
                {
                    StartCoroutine(PlayVFX(jumpLandingVFX, 0.005f));
                }

                if (landingSound != null)
                {
                    landingSound.PlaySoundFX();
                }
            }

            lastOnGroundTime = data.coyoteTime;
            lastOnGrabTime = data.grabStamina;
            dashesLeft = data.dashAmount;

            if (MyLayers.LayerInLayerMask(layer, onewayPlatform) && checkOneWayPlatformBelow)
            {
                StartCoroutine(DownJump(m_belowHitsStorage[smallestDistanceIndex].collider));
            }

            if (MyLayers.LayerInLayerMask(layer, movingPlatform))
            {
                if (false == isOnMovingPlatform)
                {
                    platformTransform = m_belowHitsStorage[smallestDistanceIndex].transform;
                    lastPlatformPosition = platformTransform.position;
                    isOnMovingPlatform = true;
                }
            }

            m_currentPlatform = 1 << layer;
        }
        else
        {
            isOnMovingPlatform = false;
        }

        //Slope();
    }

    private float maxSlopeAngle = 46f;
    private void Slope()
    {
        isOnSlope = false;
        m_currentSlopeDirection = Vector2.zero;
        m_belowSlopeAngle = 0f;

        // 점프 중이거나 공중에 있을 때는 경사면 검사 생략
        if (isJumping || lastOnGroundTime < 0)
        {
            return;
        }

        // 레이캐스트 길이 계산
        float rayLength = m_boundsWidth * Mathf.Abs(Mathf.Tan(maxSlopeAngle * Mathf.Deg2Rad));
        rayLength += m_boundsHeight * 0.5f + 0.1f; // RayOffsetVertical 대신 작은 오프셋 추가

        Vector2 raycastOrigin = m_boundsCenter;
        raycastOrigin.y = m_boundsBottomLeftCorner.y;

        // 왼쪽과 오른쪽에서 레이캐스트 수행
        raycastOrigin.x = m_boundsBottomLeftCorner.x;
        RaycastHit2D stickRaycastLeft = MyDebug.Raycast(raycastOrigin, -transform.up, rayLength, groundLayer, Color.blue, true);

        raycastOrigin.x = m_boundsBottomRightCorner.x;
        RaycastHit2D stickRaycastRight = MyDebug.Raycast(raycastOrigin, -transform.up, rayLength, groundLayer, Color.blue, true);

        // 두 레이캐스트 중 더 가까운 히트 선택
        bool castFromLeft = stickRaycastLeft.distance < stickRaycastRight.distance;
        RaycastHit2D slopeHit = castFromLeft ? stickRaycastLeft : stickRaycastRight;

        if (slopeHit.collider != null)
        {
            // 경사면 각도 계산
            m_belowSlopeAngle = Vector2.Angle(slopeHit.normal, Vector2.up);

            // 경사면이 허용 각도 내에 있는지 확인
            if (m_belowSlopeAngle > 5f && m_belowSlopeAngle < maxSlopeAngle)
            {
                isOnSlope = true;

                // 경사면의 접선 방향 계산
                m_currentSlopeDirection = Vector2.Perpendicular(slopeHit.normal).normalized;

                // 접선 방향이 아래를 향하면 반전
                if (m_currentSlopeDirection.y < 0)
                {
                    m_currentSlopeDirection *= -1;
                }
            }
        }
    }
    #endregion

    #region RUN METHODS
    private void Run(float _lerpAmount)
    {
        // 이동하고자 하는 방향과 원하는 속도 계산
        float targetSpeed = moveInput.x * data.runMaxSpeed;

        // 방향과 속도로 부드럽게 조절
        targetSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, _lerpAmount);

        // 가속도 값 계산
        float accelerate;

        // 가속 중인지 여부(회전 포함)에 따라 가속도 값 계산
        if (lastOnGroundTime > 0)
        {
            accelerate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccelAmount : data.runDeccelAmount;
        }
        else
        {
            accelerate = (Mathf.Abs(targetSpeed) > 0.01f) ?
                data.runAccelAmount * data.accelInAir : data.runDeccelAmount * data.deccelInAir;
        }

        // 점프의 정점에 도달하면 가속도와 최대 속도가 증가
        if ((isJumping || isWallJumping || isJumpFalling) &&
            Mathf.Abs(rb.linearVelocity.y) < data.jumpHangTimeThreshold)
        {
            accelerate *= data.jumpHangAccelerationMult;
            targetSpeed *= data.jumpHangMaxSpeedMult;
        }

        // 속도 제어
        if (data.doConserveMomentum &&
            Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) &&
            Mathf.Abs(targetSpeed) > 0.01f &&
            lastOnGroundTime < 0)
        {
            accelerate = 0;
        }

        // 현재 속도와 원하는 속도 간의 차이 계산
        float speed_dif = targetSpeed - rb.linearVelocity.x;
        float movement = speed_dif * accelerate;

        // 경사면에서의 이동 처리
        if (isOnSlope)
        {
            // 경사면 각도에 따른 속도 보정
            float slopeFactor = Mathf.Cos(m_belowSlopeAngle * Mathf.Deg2Rad);

            // 올라갈 때와 내려갈 때 다른 보정 적용
            if (Mathf.Sign(moveInput.x) == Mathf.Sign(m_currentSlopeDirection.x))
            {
                // 올라갈 때: 약간의 추가 힘 필요
                slopeFactor = Mathf.Clamp(slopeFactor * 1.1f, 0.8f, 1.2f);
            }
            else
            {
                // 내려갈 때: 약간의 제동 필요
                slopeFactor = Mathf.Clamp(slopeFactor * 0.9f, 0.8f, 1.2f);
            }

            // 경사면 방향으로 힘 적용 (수평 속도 유지)
            Vector2 slopeForce = movement * m_currentSlopeDirection * slopeFactor;
            rb.AddForce(slopeForce, ForceMode2D.Force);
        }
        else
        {
            // 평지 이동
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }
    }

    private void OnMovingPlatform()
    {
        if (platformTransform == null)
        {
            return;
        }


        Vector3 deltaPosition = (platformTransform.position - lastPlatformPosition);
        rb.position += (Vector2)(deltaPosition);
        lastPlatformPosition = platformTransform.position;
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }
    #endregion

    #region JUMP METHODS
    public void Jump(float _force, Vector2 _dir = default)
    {
        if (jumpsLeft <= 0)
        {
            return;
        }

        Vector2 newRb = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity = newRb;

        if (rb.linearVelocity.y < 0)
        {
            _force -= rb.linearVelocity.y;
        }

        if (_dir == default)
        {
            _dir = Vector2.up;
        }
        else
        {
            _dir.y = Mathf.Max(_dir.y, _dir.y, 1);
        }

        rb.AddForce(_dir * _force, ForceMode2D.Impulse);


        if (jumpStartVFX != null && lastOnGroundTime > 0)
        {
            jumpStartVFX.Play();
        }
        else if (jumpAirVFX != null && lastOnGroundTime < 0)
        {
            jumpAirVFX.Play();
        }

        if (jumpSound != null)
        {
            jumpSound.PlaySoundFX();
        }

        jumpsLeft -= 1;

        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        m_jumpEndIgnoreGroundUntil = Time.time + m_jumpDisableGroundCheckTime;
    }

    private void WallJump(int _dir)
    {
        //Debug.Log("벽 점프");

        wallJumpStartTime = Time.time;
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;
        lastOnWallRightTime = 0;
        lastOnWallLeftTime = 0;

        isJumping = false;
        isWallJumping = true;
        isJumpCut = false;
        isJumpFalling = false;

        float addforce = (true == isOnJumpPad) ? 1.5f * padForce : 1;

        Vector2 force = new Vector2(data.wallJumpForce.x * addforce, data.wallJumpForce.y);
        force.x *= _dir;

        if (Mathf.Sign(rb.linearVelocity.x) != Mathf.Sign(force.x))
        {
            force.x -= rb.linearVelocity.x;
        }
        if (rb.linearVelocity.y < 0)
        {
            force.y -= rb.linearVelocity.y;
        }

        rb.AddForce(force, ForceMode2D.Impulse);
    }
    #endregion

    #region SLIDE METHODE
    private void Slide()
    {
        //Debug.Log("슬라이딩");

        /*
        // 위쪽으로 미끄러지는것 방지
        //if (rb.linearVelocity.y > 0)
        //{
        //    rb.AddForce(-rb.linearVelocity.y * Vector2.up, ForceMode2D.Impulse);
        //}

        //float speed_dif = data.slide_speed - rb.linearVelocity.y;
        //float movement = speed_dif * data.slide_accel;

        //movement = Mathf.Clamp(movement, -Mathf.Abs(speed_dif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speed_dif) * (1 / Time.fixedDeltaTime));

        //rb.AddForce(movement * Vector2.up);
        */

        float stamina_consume = 0f;

        if (0 < lastOnGrabTime &&        
            true == isWallGrabbing && false == isJumping && false == isWallJumping)
        {
            if (moveInput.y > 0 && false == isLookingOther && false == isJumping)
            {
                rb.linearVelocity = new Vector2(0, moveInput.y * data.climbUpSpeed);
                stamina_consume = 4.75f;
            }
            else if (moveInput.y < 0 && false == isLookingOther)
            {
                rb.linearVelocity = new Vector2(0, moveInput.y * data.slideSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, 0);
                stamina_consume = 1f;
            }            
        }      
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.slideSpeed);
        }

        lastOnGrabTime -= (stamina_consume * Time.deltaTime);
    }

    private void EdgeDetection()
    {
        frontDownRay = transform.position + Vector3.down * 0.53f;

        Vector3 dir = isFacingRight ? Vector2.right : Vector2.left;

        hit = Physics2D.Raycast(frontDownRay, dir, rayDistance, groundLayer);
        bool check = Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer);
        bool isMoving = 0.1f < Mathf.Abs(rb.linearVelocity.x) || moveInput.x != 0 || true == isWallGrabbing;

        Debug.DrawRay(frontDownRay, dir * rayDistance, Color.magenta);

        if (hit.collider && false == check && isMoving)
        {
            float offset_mult = (isWallGrabbing && moveInput.x == 0) ? 0.5f : 0.05f;
            dir += Vector3.up;

            if (true == isWallGrabbing && moveInput.x == 0)
            {
                transform.position += dir * offset_mult;

                Vector2 diagonalForce = new Vector2(dir.x, dir.y * 0.6f).normalized * climbForce;
                rb.AddForce(diagonalForce, ForceMode2D.Impulse);

                ControllSleep(0.2f);
            }
            else
            {
                transform.position += dir * offset_mult;
            }
        }
    }
    #endregion

    #region DASH METHODS
    private IEnumerator StartDash(Vector2 _dir)
    {
        lastOnGroundTime = 0;
        lastPressedDashTime = 0;

        isDashing = true;
        isJumping = false;
        isWallJumping = false;
        isJumpCut = false;

        float start_time = Time.time;

        dashesLeft--;
        isDashAttacking = true;

        SetGravityScale(0);

        while (Time.time - start_time <= data.dashAttackTime)
        {
            rb.linearVelocity = _dir.normalized * data.dashSpeed;

            if (rb.linearVelocity.y > 0 &&
                Physics2D.OverlapBox(headCheckPoint.position, headCheckSize, 0, groundLayer & ~onewayPlatform))
            {
                break;
            }

            yield return null;
        }

        start_time = Time.time;

        isDashAttacking = false;

        SetGravityScale(data.gravityScale);
        rb.linearVelocity = _dir.normalized * data.dashEndSpeed;

        while (Time.time - start_time <= data.dashEndTime)
        {
            yield return null;
        }

        isDashing = false;
    }
    private IEnumerator RefillDash()
    {
        dashRefilling = true;

        yield return new WaitForSeconds(data.dashRefillTime);

        dashRefilling = false;
        dashesLeft = Mathf.Min(data.dashAmount, dashesLeft + 1);
    }

    public void BonusDash()
    {
        dashesLeft = data.dashAmount;
    }
    #endregion

    #region KNOCKBACK METHODS
    public void ApplyKnockback(Vector2 _dir)
    {
        if (false == this.gameObject.activeInHierarchy)
        {
            return;
        }

        if (m_health != null)
        {
            if (false == m_health.postDamageInvulnerable)
            {
                StartCoroutine(nameof(StartKnockBack), _dir);
            }
        }

        //doKnockback = true;
        //isJumping = true;
        //isWallJumping = false;
        //isJumpCut = false;
        //isJumpFalling = false;

        //moveInput = Vector2.zero;
        //rb.linearVelocity = Vector2.zero;
        //SetGravityScale(0);

        //if (Mathf.Sign(rb.linearVelocity.x) != Mathf.Sign(_dir.x))
        //{
        //    _dir.x -= rb.linearVelocity.x;
        //}
        //if (rb.linearVelocity.y < 0)
        //{
        //    _dir.y -= rb.linearVelocity.y;
        //}

        //rb.AddForce(_dir.normalized * data.jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator StartKnockBack(Vector2 _dir)
    {
        lastOnGroundTime = 0;

        doKnockback = true;

        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;

        float startTime = Time.time;

        SetGravityScale(0);

        while (Time.time - startTime <= data.dashAttackTime)
        {
            rb.linearVelocity = _dir.normalized * data.dashSpeed;

            if (rb.linearVelocity.y > 0 &&
                Physics2D.OverlapBox((m_boundsTopLeftCorner + m_boundsBottomRightCorner) * 0.5f , new Vector2(m_boundsWidth, 0.3f), 0, groundLayer & ~onewayPlatform))
            {
                break;
            }

            yield return null;
        }

        startTime = Time.time;
        
        if (movementState.currentState == PlayerStates.MovementStates.Die)
        {
            doKnockback = false;
            yield break;
        }

        SetGravityScale(data.gravityScale);
        rb.linearVelocity = _dir.normalized * data.dashEndSpeed;

        while (Time.time - startTime <= data.dashEndTime)
        {
            yield return null;
        }

        doKnockback = false;
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool _isMovingRight)
    {     
        if (_isMovingRight != isFacingRight)
        {
            Turn();
        }
    }

    private bool CanJump()
    {
        if (false == isJumping &&
            false == isWallJumping &&
            lastOnGroundTime > 0)
        {
            jumpsLeft = Mathf.Min(data.jumpAmount, jumpsLeft + 1);
        }

        return jumpsLeft > 0;
    }

    private bool CanJumpCut()
    {
        return true == isJumping && rb.linearVelocity.y > 0;
    }

    private bool CanWallJump()
    {
        return CanWallJumping && lastPressedJumpTime > 0 && lastOnWallTime > 0 && lastOnGroundTime <= 0 && 
            (false == isWallJumping || (lastOnWallRightTime > 0 && lastWallJumpDirection == 1) || (lastOnWallLeftTime > 0 && lastWallJumpDirection == -1));
    }

    private bool CanWallJumpCut()
    {
        return true == isWallJumping && rb.linearVelocity.y > 0;
    }

    private bool CanSlide()
    {
        if (CanWallSliding &&
            lastOnWallTime > 0 && 
            false == isJumping &&
            false == isWallJumping &&
            lastOnGroundTime <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CanGrab()
    {     
        if (isJumping || isWallJumping || false == CanWallGrabbing)
        {
            return false;
        }

        if (Physics2D.OverlapBox(frontWallCheckPoint.position, grabCheckSize, 0, groundLayer & ~onewayPlatform))
        {
            return true;
        }
        else if (Physics2D.OverlapBox(backWallCheckPoint.position, grabCheckSize, 0, groundLayer & ~onewayPlatform) &&
            true == isWallGrabbing)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CanDash()
    {
        if (CanDasing &&
            false == isDashing &&
            dashesLeft < data.dashAmount &&
            lastOnGroundTime > 0 &&
            false == dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        if (false == isOnJumpPad)
        {
            return dashesLeft > 0;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR

#endif
    }
    #endregion
}
