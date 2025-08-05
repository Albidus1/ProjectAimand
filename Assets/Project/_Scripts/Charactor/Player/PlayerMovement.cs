using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;



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
    public Collider2D col { get; private set; }

    public Animator animator;

    [Header("움직임 제어")]
    public bool CanWallJumping = true;
    public bool CanWallSliding = true;
    public bool CanWallGrabbing = true;
    public bool CanDasing = true;

    public bool isFacingRight { get; private set; }
    public bool isJumping { get; private set; }
    public bool isWallJumping { get; private set; }
    public bool isSliding { get; private set; }
    public bool isWallGrabbing { get; private set; }
    public bool isDashing { get; private set; }
    public bool checkOneWayPlatformBelow { get; private set; }
    public bool doKnockback { get; private set; }
    public bool isControlSleep { get; private set; }
    public bool ApplyGravityOnDeath;

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
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]

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
    public LayerMask magnetPlatform;

    [SerializeField] private LayerMask groundLayer;
    [MyReadOnly] public LayerMask m_currentPlatform;

    [Header("이벤트")]
    public bool SendStateChangeEvents = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        animator = GetComponent<Animator>();

        movementState = new MyStateManager<PlayerStates.MovementStates>(this.gameObject, SendStateChangeEvents);
        movementState.StateChange(PlayerStates.MovementStates.Idle);

        playerState = new PlayerStates();

        //임시
        //CinemachineCamera cam = FindAnyObjectByType<CinemachineCamera>();
        //cam.Target.TrackingTarget = transform;

        groundLayer |= platform;
        groundLayer |= movingPlatform;
        groundLayer |= onewayPlatform;
        groundLayer |= magnetPlatform;
    }
    private void Start()
    {
        SetGravityScale(data.gravityScale);

        isFacingRight = true;   
    }

    private void OnEnable()
    {
        movementState.StateChange(PlayerStates.MovementStates.Idle);
        moveInput = Vector2.zero;
    }

    private void Update()
    {
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
        #endregion

        #region COLLISION CHECKS
        if (false == isDashing)
        {
            if (Time.time > m_jumpEndIgnoreGroundUntil &&
                Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer))
            {
                lastOnGroundTime = data.coyoteTime;
                lastOnGrabTime = data.grabStamina;
                dashesLeft = data.dashAmount;

                if (checkOneWayPlatformBelow)
                {
                    Collider2D oneway = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, onewayPlatform);

                    if (oneway != null)
                    {
                        StartCoroutine(DownJump(oneway));
                    }
                }
            }

            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight)
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight)) &&
                false == isJumping && false == isWallJumping)
            {
                //Debug.Log("오른쪽 벽 확인");
                lastOnWallRightTime = data.coyoteTime;
            }

            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight)
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight)) &&
                false == isJumping && false == isWallJumping)
            {
                //Debug.Log("왼쪽 벽 확인");
                lastOnWallLeftTime = data.coyoteTime;
            }

            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }

        checkOneWayPlatformBelow = false;

        EdgeDetection();

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

            Vector2 newRb = new Vector2(rb.linearVelocity.x, 0);
            rb.linearVelocity = newRb;

            if (padDirection == Vector2.up)
            {
                StartCoroutine(nameof(RefillDash), 1);

                lastOnJumpPadTime = data.jumpInputBufferTime;

                rb.linearVelocity = moveInput;

                Mathf.Clamp(data.jumpForce * padForce, 20, 80);
                Jump(data.jumpForce * padForce);
            }
            else
            {
                StartCoroutine(nameof(RefillDash), 1);

                lastOnJumpPadTime = data.jumpInputBufferTime;

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
            if (true == isSliding)
            {
                SetGravityScale(0);
            }
            else if (true == isWallGrabbing && false == isJumping && false == isWallJumping)
            {
                SetGravityScale(0);
            }
            else if (rb.linearVelocity.y < 0 && moveInput.y < 0)
            {
                SetGravityScale(data.gravityScale * data.fastFallGravityMult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFastFallSpeed));
            }
            else if (true == isJumpCut)
            {
                SetGravityScale(data.gravityScale * data.jumpCutGravityMult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFallSpeed));
            }
            else if ((true == isJumping || true == isWallJumping || true == isJumpFalling) &&
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
        if (isDashing)
        {
            movementState.StateChange(PlayerStates.MovementStates.Dashing);
        }
        else if (isJumping)
        {
            movementState.StateChange(PlayerStates.MovementStates.Jumping);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (isJumpFalling)
        {
            movementState.StateChange(PlayerStates.MovementStates.Falling);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
        else if (moveInput.x != 0 && lastOnGroundTime > 0)
        {       
            movementState.StateChange(PlayerStates.MovementStates.Running);
            animator.SetBool("isRunning", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else
        {
            movementState.StateChange(PlayerStates.MovementStates.Idle);
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (movementState.currentState == PlayerStates.MovementStates.Die)
        {
            return;
        }

        if (false == isDashing)
        {
            if (true == isWallJumping)
            {
                Run(data.wallJumpRunLerp);
            }
            else if (true == isWallGrabbing && 0 < lastOnGrabTime)
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

        if (true == isOnMovingPlatform)
        {
            OnMovingPlatform();
        }

        if (true == isSliding || true == isWallGrabbing)
        {
            Slide();
        }
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
        Physics2D.IgnoreCollision(col, _col, true);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(col, _col, false);
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

        int direction = _facingDirection ? 1 : -1;

        Vector3 scale = transform.localScale;
        scale.x = direction * scale.x;
        transform.localScale = scale;

        isFacingRight = _facingDirection;

        transform.position = _spawnPoint.position;

        moveInput = Vector2.zero;
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
        // 감속 포함
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
        if ((true == isJumping || true == isWallJumping || true == isJumpFalling) &&
            Mathf.Abs(rb.linearVelocity.y) < data.jumpHangTimeThreshold)
        {
            accelerate *= data.jumpHangAccelerationMult;
            targetSpeed *= data.jumpHangMaxSpeedMult;
        }

        // 속도 제어
        if (true == data.doConserveMomentum &&
            Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) &&
            Mathf.Abs(targetSpeed) > 0.01f &&
            lastOnGroundTime < 0)
        {
            // 감속이 발생하지 않도록 방지
            accelerate = 0;
        }

        // 현재 속도와 원하는 속도 간의 차이 계산
        // 플레이어에게 적용할 X축을 따라 힘 계산
        float speed_dif = targetSpeed - rb.linearVelocity.x;

        float movement = speed_dif * accelerate;

        // 벡터로 변환
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void OnMovingPlatform()
    {
        // 움직이는 플렛폼 보정
        if (true == isOnMovingPlatform)
        {
            Vector3 platform_velocity = (platformTransform.position - lastPlatformPosition) / Time.deltaTime;

            rb.position += (Vector2)(platform_velocity * Time.deltaTime);

            lastPlatformPosition = platformTransform.position;
        }
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
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

        m_jumpEndIgnoreGroundUntil = Time.time + m_jumpDisableGroundCheckTime;

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
    public void Knockback(Vector2 _dir)
    {
        StartCoroutine(nameof(StartKnockBack), _dir);
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
                Physics2D.OverlapBox(headCheckPoint.position, headCheckSize, 0, groundLayer & ~onewayPlatform))
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
        return (lastOnGroundTime > 0) && false == isJumping;
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
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 size = new Vector2 (col.size.x * transform.localScale.x, col.size.y * transform.localScale.y);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(headCheckPoint.position, headCheckSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);

        if (true == isWallGrabbing)
        {
            Gizmos.color = Color.green;
            if (true == isLookingOther)
            {
                Gizmos.DrawWireCube(backWallCheckPoint.position, grabCheckSize);
            }
            else
            {
                Gizmos.DrawWireCube(frontWallCheckPoint.position, grabCheckSize);
            }
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(frontWallCheckPoint.position, grabCheckSize);
        }
#endif
    }
    #endregion
}
