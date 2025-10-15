using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class PlatformDoor : ButtonObjectActivate, IEventListener<WaveEvent>, ISaveLoadManagerMethods
{
    public Rigidbody2D rb { get; private set; }

    [MyReadOnly]
    public bool isTrigger = false;

    // 위치
    public Vector2 pointA;
    public Vector2 pointB;
    public Vector3 next { get; private set; }
    public Vector3 direction { get; private set; }

    // 이동 속도
    [Header("딜레이")]
    public float startDelay;
    public float inputDelay;
    [Space(5)]

    [Header("목표 데이터")]
    public float speedToDestination;
    public float startWaitTime;
    [Space(5)]

    [Header("복귀 데이터")]
    public float speedToHome;
    public float homeWaitTime;

    [Header("이벤트")]
    public string triggerWaveID = "defaultWave";

    public bool isMovingStart { get; private set; }
    public bool isMoving { get; private set; }
    public bool isPaused { get; private set; }
    public bool isReturning { get; private set; }

    private float m_waitTime;



    #region SAVELOAD
    public override string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public override void Load(string _json)
    {
        JsonUtility.FromJsonOverwrite(_json, this);
    }
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        transform.position = pointA;
        next = pointB;
        DirectionCalculate();
    }

    [HideInInspector] public bool Initialized = false;
    public void InitializePoints()
    {
        if (false == Initialized)
        {
            pointA = transform.position;
            pointB = transform.position + Vector3.right * 5f;
        }

        Initialized = true;
    }

    private void Update()
    {
        #region TIMERS
        m_waitTime -= Time.deltaTime;
        #endregion

        #region HANDLE MOVEMENT
        if (base.button == null && base.objectID == "")
        {
            if (true == CanMove())
            {
                m_waitTime = startDelay;

                isMovingStart = true;
            }
        }
        #endregion

        #region HANDLE PAUSE
        if (m_waitTime <= 0)
        {
            isPaused = false;
        }
        else
        {
            isPaused = true;
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (true == isMovingStart)
        {
            isMovingStart = false;
            isMoving = true;
        }

        if (true == isMoving && false == isPaused)
        {
            Move();
        }
    }

    public override void ObjectActivate(ButtonObject _button)
    {
        base.ObjectActivate(_button);

        if (true == isMoving)
        {
            return;
        }

        isMoving = true;
        isReturning = false;
        m_waitTime = startWaitTime;
        DirectionCalculate();
    }

    public void DirectionCalculate(bool _flag = false)
    {
        direction = _flag ? (pointB - pointA).normalized : (next - transform.position).normalized;
    }

    #region MOVE
    private void Move()
    {
        float distanceToTarget = (next - transform.position).magnitude;
        float currentSpeed = (isReturning ? speedToHome : speedToDestination) * Time.deltaTime;

        if (currentSpeed >= distanceToTarget)
        {
            transform.position = next;
            HandleArrival();
        }
        else
        {
            transform.Translate(direction * currentSpeed, Space.World);
        }
    }

    private void HandleArrival()
    {
        if (!isReturning)
        {
            next = pointA;
            isReturning = true;
            m_waitTime = homeWaitTime;
        }
        else
        {
            next = pointB;
            isReturning = false;
            isMoving = false;
            m_waitTime = startWaitTime;
        }

        DirectionCalculate();
    }
    #endregion

    #region CHECK METHODES
    private bool CanMove()
    {
        return false == isMoving && isTrigger;
    }
    #endregion

    #region ON COLLISION
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (base.button != null && base.objectID != "")
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            isTrigger = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (base.button != null && base.objectID != "")
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {

        }
    }
    #endregion

    #region EVENT METHODS
    public void OnEvent(WaveEvent _event)
    {
        if (triggerWaveID != _event.waveID)
        {
            return;
        }

        Debug.Log("웨이브 감지됨");

        isMoving = true;
        isReturning = false;
        m_waitTime = startWaitTime;
        DirectionCalculate();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.EventStartListening<WaveEvent>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.EventStopListening<WaveEvent>();
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR     
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointB, 0.5f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pointA, pointB);
#endif
    }
    #endregion
}
