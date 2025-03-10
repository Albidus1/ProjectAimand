using System.Collections;
using System.ComponentModel;
using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { Idle, Run, Jumping, Falling, Sliding, Grabbing, Dashing }
    [MyReadOnly] 
    public PlayerState State;
    [Space(10)]

    public PlayerData data;

    public Rigidbody2D rb { get; private set; }

    public bool isFacingRight { get; private set; }
    public bool isJumping { get; private set; }
    public bool isWallJumping { get; private set; }
    public bool isSliding { get; private set; }
    public bool isWallGrabbing { get; private set; }
    public bool isDashing { get; private set; }
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

    // 벽 점프
    private float wallJumpStartTime;
    private int lastWallJumpDirection;

    // 벽 이동
    private int lastGrabDirection;
    private bool isLookingOther;

    // 대쉬
    public int dashesLeft { get; private set; }
    private bool dashRefilling;
    private Vector2 lastDashDirection;
    private bool isDashAttacking;

    // 입력
    private Vector2 moveInput;
    public float lastPressedJumpTime { get; private set; }
    public float lastPressedGrabTime { get; private set; }
    public float lastPressedDashTime { get; private set; }

    // 확인
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

    // 레이어
    [Header("레이어")]
    public LayerMask platform;
    public LayerMask movingPlatform;
    public LayerMask onewayPlatform;
    [SerializeField] private LayerMask groundLayer;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        groundLayer |= platform;
        groundLayer |= movingPlatform;
        groundLayer |= onewayPlatform;
    }

    private void Start()
    {
        SetGravityScale(data.gravity_scale);

        isFacingRight = true;        
    }

    private void Update()
    {
        #region TIMERS
        lastOnGroundTime -= Time.deltaTime;
        lastOnWallTime -= Time.deltaTime;
        lastOnWallRightTime -= Time.deltaTime;
        lastOnWallLeftTime -= Time.deltaTime;

        lastOnJumpPadTime -= Time.deltaTime;

        lastPressedJumpTime -= Time.deltaTime;
        lastPressedDashTime -= Time.deltaTime;
        lastPressedGrabTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        if (false == isControlSleep)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            if (moveInput.x != 0)
            {
                CheckDirectionToFace(moveInput.x > 0);
            }

            if (true == isWallGrabbing && false == isLookingOther)
            {
                CheckDirectionToFace(lastGrabDirection == 1);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                OnJumpInput();
            }

            if (Input.GetKeyUp(KeyCode.Z))
            {
                OnJumpUpInput();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                OnDashInput();
            }

            if (Input.GetKey(KeyCode.C))
            {
                OnGrabInput();
            }
        }      
        #endregion

        #region COLLISION CHECKS
        if (false == isJumping && false == isDashing)
        {         
            if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer))
            {
                lastOnGroundTime = data.coyote_time;
                lastOnGrabTime = data.grab_stamina;
            }

            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight) 
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight)) &&
                false == isWallJumping)
            {
                //Debug.Log("오른쪽 벽 확인");
                lastOnWallRightTime = data.coyote_time;
            }

            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && false == isFacingRight) 
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer & ~onewayPlatform) && true == isFacingRight)) &&
                false == isWallJumping)
            {
                //Debug.Log("왼쪽 벽 확인");
                lastOnWallLeftTime = data.coyote_time;
            }

            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);    
        }

        EdgeDetection();

        #endregion

        #region JUMP CHECK
        if (true == isJumping && rb.linearVelocity.y < 0)
        {
            isJumping = false;

            isJumpFalling = true;
        }

        if (true == isWallJumping && 
            Time.time - wallJumpStartTime > data.wall_jump_time)
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

        if (false == isDashing)
        {
            // 점프
            if ((true == CanJump() || (true == isWallGrabbing && 0 < lastOnGrabTime && false == isLookingOther)) && 
                lastPressedJumpTime > 0)
            {
                if (true == isWallGrabbing)
                {
                    lastOnGrabTime -= (data.grab_stamina * 0.25f);
                }

                if (true == isJumpingOnMovingPlatform)
                {
                    Jump(data.jump_force, platformDirection);
                }
                else
                {
                    Jump(data.jump_force);
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
            if (padDirection == Vector2.up)
            {
                StartCoroutine(nameof(RefillDash), 1);

                lastOnJumpPadTime = data.jump_input_buffer_time;

                rb.linearVelocity = moveInput;

                Jump(data.jump_force * padForce);
            }
            else
            {
                StartCoroutine(nameof(RefillDash), 1);

                lastOnJumpPadTime = data.jump_input_buffer_time;

                lastWallJumpDirection = (padDirection == Vector2.right) ? 1 : -1;

                rb.linearVelocity = padDirection;
                WallJump(lastWallJumpDirection);
            }
        }

        if (lastOnJumpPadTime < 0)
        {
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

            Sleep(data.dash_sleep_time);

            if (moveInput != Vector2.zero)
            {
                lastDashDirection = moveInput;
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
                SetGravityScale(data.gravity_scale * data.fast_fall_gravity_mult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.max_fast_fall_speed));
            }
            else if (true == isJumpCut)
            {
                SetGravityScale(data.gravity_scale * data.jump_cut_gravity_mult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.max_fall_speed));
            }
            else if ((true == isJumping || true == isWallJumping || true == isJumpFalling) &&
                Mathf.Abs(rb.linearVelocity.y) < data.jump_hang_time_threshold)
            {
                SetGravityScale(data.gravity_scale * data.jump_hang_gravity_mult);
            }
            else if (rb.linearVelocity.y < 0)
            {
                SetGravityScale(data.gravity_scale * data.fall_gravity_mult);

                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.max_fall_speed));
            }
            else
            {
                SetGravityScale(data.gravity_scale);
            }
        }
        else
        {
            SetGravityScale(0);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (false == isDashing)
        {
            if (true == isWallJumping)
            {
                Run(data.wall_jump_run_lerp);
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
            Run(data.dash_end_run_lerp);
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
        lastPressedJumpTime = data.jump_input_buffer_time;
    }

    public void OnJumpUpInput()
    {
        if (true == CanJumpCut() || true == CanWallJumpCut())
        {
            isJumpCut = true;
        }
    }

    public void OnDashInput()
    {
        lastPressedDashTime = data.dash_input_buffer_time;
    }

    public void OnGrabInput()
    {
        lastPressedGrabTime = data.grab_input_buffer_time;
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

    private IEnumerator PerformControllSleep(float _duration)
    {
        isControlSleep = true;

        yield return new WaitForSecondsRealtime(_duration);

        isControlSleep = false;
    }

    public void RespawnAt(Transform _spawnPoint, bool _facingDirection)
    {
        ControllSleep(0.5f);

        CheckDirectionToFace(_facingDirection);

        transform.position = _spawnPoint.position;
    }
    #endregion

    #region RUN METHODS
    private void Run(float _lerp_amount)
    {
        // 이동하고자 하는 방향과 원하는 속도 계산
        float target_speed = moveInput.x * data.run_max_speed;

        // 방향과 속도로 부드럽게 조절
        target_speed = Mathf.Lerp(rb.linearVelocity.x, target_speed, _lerp_amount);

        // 가속도 값 계산
        float accelerate;

        // 가속 중인지 여부(회전 포함)에 따라 가속도 값 계산
        // 감속 포함
        if (lastOnGroundTime > 0)
        {
            accelerate = (Mathf.Abs(target_speed) > 0.01f) ? data.run_accel_amount : data.run_deccel_amount;
        }
        else
        {
            accelerate = (Mathf.Abs(target_speed) > 0.01f) ? 
                data.run_accel_amount * data.accel_in_air : data.run_deccel_amount * data.deccel_in_air;
        }

        // 점프의 정점에 도달하면 가속도와 최대 속도가 증가
        if ((true == isJumping || true == isWallJumping || true == isJumpFalling) &&
            Mathf.Abs(rb.linearVelocity.y) < data.jump_hang_time_threshold)
        {
            accelerate *= data.jump_hang_acceleration_mult;
            target_speed *= data.jump_hang_max_speed_mult;
        }

        // 속도 제어
        if (true == data.doConserveMomentum &&
            Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(target_speed) &&
            Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(target_speed) &&
            Mathf.Abs(target_speed) > 0.01f &&
            lastOnGroundTime < 0)
        {
            // 감속이 발생하지 않도록 방지
            accelerate = 0;
        }

        // 현재 속도와 원하는 속도 간의 차이 계산
        // 플레이어에게 적용할 X축을 따라 힘 계산
        float speed_dif = target_speed - rb.linearVelocity.x;

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
        Debug.Log("점프");
      
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        isJumping = true;
        isWallJumping = false;
        isJumpCut = false;
        isJumpFalling = false;

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
            _dir.y += 0.5f;
        }

        rb.AddForce(_dir * _force, ForceMode2D.Impulse);
    }

    private void WallJump(int _dir)
    {
        Debug.Log("벽 점프");

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

        Vector2 force = new Vector2(data.wall_jump_force.x * addforce, data.wall_jump_force.y);
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
                rb.linearVelocity = new Vector2(0, moveInput.y * data.climb_up_speed);
                stamina_consume = 4.75f;
            }
            else if (moveInput.y < 0 && false == isLookingOther)
            {
                rb.linearVelocity = new Vector2(0, moveInput.y * data.slide_speed);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, 0);
                stamina_consume = 1f;
            }            
        }      
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.slide_speed);
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

        while (Time.time - start_time <= data.dash_attack_time)
        {
            rb.linearVelocity = _dir.normalized * data.dash_speed;

            if (rb.linearVelocity.y > 0 &&
                Physics2D.OverlapBox(headCheckPoint.position, headCheckSize, 0, groundLayer & ~onewayPlatform))
            {
                break;
            }

            yield return null;
        }

        start_time = Time.time;

        isDashAttacking = false;

        SetGravityScale(data.gravity_scale);
        rb.linearVelocity = _dir.normalized * data.dash_end_speed;

        while (Time.time - start_time <= data.dash_end_time)
        {
            yield return null;
        }

        isDashing = false;
    }
    private IEnumerator RefillDash()
    {
        dashRefilling = true;

        yield return new WaitForSeconds(data.dash_refill_time);

        dashRefilling = false;
        dashesLeft = Mathf.Min(data.dash_amount, dashesLeft + 1);
    }

    public void BonusDash()
    {
        dashesLeft = data.dash_amount;
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
        return lastPressedJumpTime > 0 && lastOnWallTime > 0 && lastOnGroundTime <= 0 && 
            (false == isWallJumping || (lastOnWallRightTime > 0 && lastWallJumpDirection == 1) || (lastOnWallLeftTime > 0 && lastWallJumpDirection == -1));
    }

    private bool CanWallJumpCut()
    {
        return true == isWallJumping && rb.linearVelocity.y > 0;
    }

    private bool CanSlide()
    {
        if (lastOnWallTime > 0 && 
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
        if (true == isJumping || true == isWallJumping)
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
        if (false == isDashing &&
            dashesLeft < data.dash_amount &&
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
        Vector2 size = new Vector2 (col.size.x, col.size.y * 1.2f);

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
