using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class PlatformMoving : MyPath, ISaveLoadManagerMethods
{
    [Header("플레이어 동기화")]
    public bool isPlayerSync = false;

    // 속도
    [Header("속도")]
    public float movementSpeed;

    [Header("가속")]
    [Tooltip("플레이어가 가속을 받는 최소 속도")]
    public bool isAccelerateAble = false;
    [MyConditionalHide("isAccelerateAble", true)]
    public float speedThreshold;


    public float jumpTime { get; private set; }
    public bool isMoving { get; private set; }
    private float m_waitTimer;
    private Vector3 m_lastPosition;
    private PlayerMovement m_player;


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

    }
    
    protected override void Start()
    {
        base.Start();
        base.canMove = true;
    }

    protected override void Update()
    {
        #region TIMERS
        jumpTime -= Time.deltaTime;
        #endregion

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

    private void FixedUpdate()
    {
        
    }

    private void ExecuteUpdate()
    {
        if (base.pathElements == null 
            || base.pathElements.Count < 1
            || base.m_endReached
            || false == canMove)
        {
            return;
        }

        if (isPlayerSync && false == PlatformCanMove())
        {
            return;
        }

        CheckAccelerateAble();
        Move();

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

    #region CHECK METHODES
    private bool PlatformCanMove()
    {
        return m_player != null && (0 < m_player.lastOnGroundTime || m_player.isWallGrabbing);
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

            if (PlatformCanMove())
            {
                m_player.isOnMovingPlatform = true;
                m_player.platformTransform = transform;
                m_player.lastPlatformPosition = transform.position;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && m_player != null)
        {
            m_player.isOnMovingPlatform = false;
            m_player.platformTransform = null;
            m_player.lastPlatformPosition = Vector3.zero;
        }
    }
    #endregion
}
