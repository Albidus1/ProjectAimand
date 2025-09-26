using UnityEngine;
using DG.Tweening;



[SelectionBase]
public class PlatformMoving : MyPath, Respawnable, IEventListener<TriggerEvent>, IEventListener<ColorInvertEvent>, ISaveLoadManagerMethods
{
    public enum RotateDirection
    {
        None,
        Left,
        Right
    }

    [Header("플레이어 동기화")]
    public bool isPlayerSync = false;

    [Header("속도")]
    public float movementSpeed;

    [Header("회전")]
    public bool isRotateAble;
    [MyConditionalHide("isRotateAble", true)]
    public float rotateSpeed = 0f;
    [MyConditionalHide("isRotateAble", true)]
    public RotateDirection rotateDirection = RotateDirection.None;

    [Header("가속")]
    [Tooltip("플레이어가 가속을 받는 최소 속도")]
    public bool isAccelerateAble = false;
    [MyConditionalHide("isAccelerateAble", true)]
    public float speedThreshold;

    [Header("이벤트")]
    public bool invertMoveOnInput = false;
    public bool useTriggerEvent = false;
    [MyConditionalHide("useTriggerEvent", true)]
    public string eventID;


    public Rigidbody2D rb { get; private set; }
    public float jumpTime { get; private set; }
    public bool isMoving { get; private set; }

    private Quaternion m_initialRotation;
    private float m_waitTimer;
    private Vector3 m_lastPosition;
    private PlayerMovement m_player;
    private bool m_playerSync = false;
    private TriggerEventSetting m_eventSetting;
    private DetectColorInversion m_detectColorInversion;


    #region SAVELOAD
    public virtual string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public virtual void Load(string _json)
    {
        JsonUtility.FromJsonOverwrite(_json, this);
    }
    #endregion

    private void Awake()
    {
        m_detectColorInversion = GetComponent<DetectColorInversion>();
    }
    
    protected override void Start()
    {
        m_lastPosition = transform.position;
        m_initialRotation = transform.rotation;

        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
        base.canMove = true;   

        m_playerSync = false;
        m_eventSetting = null;
        isMoving = false;
        transform.rotation = m_initialRotation;
    }

    protected override void Update()
    {
        #region TIMERS
        jumpTime -= Time.deltaTime;
        #endregion
    }

    private void FixedUpdate()
    {
        #region HANDLE MOVEMENT
        ExecuteUpdate();

        if (m_player != null)
        {
            if (jumpTime <= 0 && true == isAccelerateAble)
            {
                //player.platformDirection = direction;
            }
            else
            {
                m_player.platformDirection = Vector3.zero;
            }
        }
        #endregion
    }

    private void ExecuteUpdate()
    {
        //Debug.Log(m_direction);

        if (base.pathElements == null 
            || base.pathElements.Count < 1
            || base.m_endReached
            || false == canMove)
        {
            if ((m_eventSetting != null && m_eventSetting.isTrigger) ||
                false == useTriggerEvent)
            {
                RotatePlatform();
            }

            return;
        }

        if (PlatformCanMove())
        {
            CheckAccelerateAble();
            RotatePlatform();
            Move();
        }

        m_lastPosition = transform.position;
    }

    #region MOVE
    private void Move()
    {
        m_waitTimer -= Time.deltaTime;

        if (m_waitTimer > 0)
        {
            return;
        }

        Vector3 position = base.originalTransformPosition + base.m_currentPoint.Current;
        transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * movementSpeed);

        base.m_distanceToNextPoint = (transform.position - position).magnitude;
        if (base.m_distanceToNextPoint < base.minDistanceToGoal)
        {
            if (base.pathElements.Count > base.currentIndex)
            {
                m_waitTimer = base.pathElements[base.currentIndex].delay;
            }

            base.m_previousPoint = base.m_currentPoint.Current;
            m_currentPoint.MoveNext();

            transform.position = position;
        }
    }

    private void UpdateSpeed()
    {
/*        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (speed >= speedThreshold)
        {
            jumpTime = inputDelay;
        }*/
    }
    #endregion

    #region ROTATE
    private void RotatePlatform()
    {
        //Debug.Log(isRotateAble);

        if (false == isRotateAble)
        {
            return;
        }

        if (pathElements.Count > 1)
        {
            Vector3 position = base.originalTransformPosition + base.m_currentPoint.Current;
            Vector3 moveDirection = position - transform.position;
            //float rotationSmoothing = 0.1f;

            if (base.CycleOption == CycleOptions.PingPong && base.m_direction < 0)
            {
                moveDirection = transform.position - position;
            }

            if (moveDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime);
            }
        }
        else
        {
            //Debug.Log(rotateDirection);

            Quaternion rotation = transform.rotation;
            switch (rotateDirection)
            {
                case RotateDirection.None:
                    return;
                case RotateDirection.Left:
                    rotation *= Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);
                    break;
                case RotateDirection.Right:
                    rotation *= Quaternion.Euler(0, 0, -rotateSpeed * Time.deltaTime);
                    break;
            }

            transform.rotation = rotation;
        }
    }
    #endregion

    #region CHECK METHODES
    private bool PlatformCanMove()
    {
        if (m_detectColorInversion != null)
        {
            if (useTriggerEvent)
            {
                if (IsTriggerEventActive())
                {
                    if (invertMoveOnInput)
                    {
                        return true;
                    }
                    else
                    {
                        return false == m_detectColorInversion.enableSetting;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (invertMoveOnInput)
            {
                return true;
            }
            else
            {
                return false == m_detectColorInversion.enableSetting;
            }
        }

        if (useTriggerEvent)
        {
            return IsTriggerEventActive();
        }

        if (false == isPlayerSync)
        {
            return true;
        }

        return IsPlayerOnPlatform();
    }

    private bool IsTriggerEventActive()
    {
        return m_eventSetting != null && m_eventSetting.isTrigger;
    }

    private bool IsPlayerOnPlatform()
    {
        return m_player != null && m_playerSync &&
               (m_player.lastOnGroundTime > 0 || m_player.isWallGrabbing);
    }

    private void CheckAccelerateAble()
    {
        if (m_player == null)
        {
            return;
        }

        if (0 < jumpTime)
        {
            m_player.isJumpingOnMovingPlatform = true;
        }
        else
        {
            m_player.isJumpingOnMovingPlatform = false;
        }
    }
    #endregion

    #region ON COLLISION
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (m_player == null)
            {
                m_player = collision.gameObject.GetComponent<PlayerMovement>();
            }

            m_playerSync = isPlayerSync;
            //m_player.isOnMovingPlatform = true;
            //m_player.platformTransform = transform;
            //m_player.lastPlatformPosition = transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && m_player != null)
        {
            //m_player.isOnMovingPlatform = false;
            //m_player.platformTransform = null;
            //m_player.lastPlatformPosition = Vector3.zero;

            //m_player = null;
        }
    }
    #endregion

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Debug.Log("확인용");

        Initialization();
        base.canMove = true;
    }

    public void OnEvent(TriggerEvent e)
    {
        if (e.eventID != this.eventID)
        {
            return;
        }

        m_eventSetting = e.setting;

        if (m_eventSetting != null)
        {
            if (m_eventSetting.isTrigger)
            {
                return;
            }           
        }
    }

    public void OnEvent(ColorInvertEvent e)
    {     
        if (base.CycleOption == CycleOptions.Single)
        {
            base.ChangeDirection(e.isInvert ? -1 : 1);
            base.canMove = true;

            Debug.Log(m_direction);
        }
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<TriggerEvent>();
        this.EventStartListening<ColorInvertEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<TriggerEvent>();
        this.EventStartListening<ColorInvertEvent>();
    }
}
