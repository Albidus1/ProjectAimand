using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class PlatformMoving : MonoBehaviour, ISaveLoadManagerMethods
{
    public Rigidbody2D rb { get; private set; }
    public PlayerMovement player { get; private set; }

    // 위치
    public Vector2 pointA;
    public Vector2 pointB;
    public Vector3 next { get; private set; }
    public Vector3 direction { get; private set; }
    private Vector3 lastPosition;

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
    [Space(5)]

    [Tooltip("플레이어가 가속을 받는 최소 속도")]
    public float speedThreshold;

    private float waitTime;
    public float jumpTime { get; private set; }

    public bool isMovingStart { get; private set; }
    public bool isMoving { get; private set; }
    public bool isPaused { get; private set; }
    public bool isReturning { get; private set; }



    #region SAVELOAD
    public string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public void Load(string _json)
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

        lastPosition = transform.position;
    }

    [HideInInspector] public bool isInitialized = false;
    public void InitializePoints()
    {
        if (false == isInitialized)
        {
            pointA = transform.position;
            pointB = transform.position + Vector3.right * 5f;
        }

        isInitialized = true;
    }

    private void Update()
    {
        #region TIMERS
        waitTime -= Time.deltaTime;

        jumpTime -= Time.deltaTime;
        #endregion

        if (player != null)
        {
            if (true == CanMove())
            {
                waitTime = startDelay;

                isMovingStart = true;
            }

            CheckAccelerateAble();

            if (jumpTime <= 0)
            {
                player.platformDirection = direction;
            }
        }

        if (waitTime <= 0)
        {
            isPaused = false;
        }
        else
        {
            isPaused = true;
        }
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

    public void DirectionCalculate(bool _flag = false)
    {
        if (true == _flag)
        {
            direction = (pointB - pointA).normalized;
        }
        else
        {
            direction = (next - transform.position).normalized;
        }
    }

    #region MOVE
    private void Move()
    {
        float distance_to_target = (next - transform.position).magnitude;

        float current_speed = (false == isReturning ? speedToDestination : speedToHome) * Time.deltaTime;

        if (current_speed >= distance_to_target)
        {
            transform.position = next;

            if (false == isReturning)
            {
                // B지점 도착
                next = pointA;
                isReturning = true;
                
                waitTime = homeWaitTime;
            }
            else
            {
                // A지점 복귀
                next = pointB;
                isReturning = false;
                isMoving = false;

                waitTime = startWaitTime;
            }

            DirectionCalculate();
        }
        else
        {
            transform.Translate(direction * current_speed, Space.World);
        }

        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (speed >= speedThreshold)
        {
            jumpTime = inputDelay;
        }        
    }
    #endregion

    #region CHECK METHODES
    private bool CanMove()
    {
        return false == isMoving && (0 < player.lastOnGroundTime || player.isWallGrabbing);
    }

    private void CheckAccelerateAble()
    {
        if (player == null)
        {
            return;
        }

        if (0 < jumpTime)
        {
            player.isJumpingOnMovingPlatform = true;
        }
        else
        {
            player.isJumpingOnMovingPlatform = false;
        }
    }
    #endregion

    #region ON COLLISION
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<PlayerMovement>();

            player.isOnMovingPlatform = true;
            player.platformTransform = transform;
            player.lastPlatformPosition = transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && player != null)
        {
            player.isOnMovingPlatform = false;
            player.platformTransform = null;
            player.lastPlatformPosition = Vector3.zero;

            player = null;
        }
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
