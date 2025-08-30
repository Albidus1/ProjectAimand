using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : EnemyMovementControl
{
    [Header("공격 콜라이더")]
    public Collider2D attackCollider2D;

    [Header("레이캐스트")]
    public int raysCount = 5;
    public Vector3 colliderSize => Vector3.Scale(transform.localScale, boxCollider.size);
    public Vector3 colliderCenterPosition => boxCollider.bounds.center;

    //private RaycastHit2D[] sideHitsStorage;
    //private RaycastHit2D[] belowHitsStorage;

    //private Vector2 horizontalRaycastFromBottom = Vector2.zero;
    //private Vector2 horizontalRaycastToTop = Vector2.zero;
    //private Vector2 verticalRaycastFromLeft = Vector2.zero;
    //private Vector2 verticalRaycastToRight = Vector2.zero;
    //private Vector2 raycastOrigin = Vector2.zero;

    public bool isPatternActive => base.patternController != null && patternController.isPatternActive;

    private bool m_hitObject;

    protected Vector2 bounds;
    protected Vector2 boundsCenter;
    protected Vector2 boundsTopLeftCorner;
    protected Vector2 boundsBottomLeftCorner;
    protected Vector2 boundsTopRightCorner;
    protected Vector2 boundsBottomRightCorner;
    protected float boundsWidth;
    protected float boundsHeight;

    private bool isChasingPlayer;
    private Vector2 playerPosition;



    protected override void Awake()
    {
        base.Awake();
        SetRaysParameters();
    }

    #region INITIALIZATION
    protected override void Initiailization()
    {
        base.Initiailization();
        SetRaysParameters();
    }

    private void SetRaysParameters()
    {
        float x = boxCollider.size.x;
        float y = boxCollider.size.y;

        float right = x * 0.5f;
        float left = -x * 0.5f;
        float top = y * 0.5f;
        float bottom = -y * 0.5f;


        boundsCenter = boxCollider.bounds.center;

        boundsTopLeftCorner.x = left;
        boundsTopLeftCorner.y = top;

        boundsBottomLeftCorner.x = left;
        boundsBottomLeftCorner.y = bottom;

        boundsTopRightCorner.x = right;
        boundsTopRightCorner.y = top;

        boundsBottomRightCorner.x = right;
        boundsBottomRightCorner.y = bottom;

        boundsTopLeftCorner = transform.TransformPoint(boundsTopLeftCorner);
        boundsBottomLeftCorner = transform.TransformPoint(boundsBottomLeftCorner);
        boundsTopRightCorner = transform.TransformPoint(boundsTopRightCorner);
        boundsBottomRightCorner = transform.TransformPoint(boundsBottomRightCorner);

        boundsWidth = Vector2.Distance(boundsTopLeftCorner, boundsTopRightCorner);
        boundsHeight = Vector2.Distance(boundsTopLeftCorner, boundsBottomLeftCorner);
    }
    #endregion

    protected override void Update()
    {
        base.Update();
        SetRaysParameters();
        CastRay();
        DetectPlayer();
        Move();
    }

    protected override void Move()
    {
        Vector2 direction = Vector2.zero;

        base.animator.SetBool("isMoving", false);

        if (isAttackingPlayer)
        {
            return;
        }

        if (isChasingPlayer)
        {
            if (m_hitObject)
            {
                direction = Vector2.zero;
            }
            else
            {
                if (Mathf.Abs(playerPosition.x - transform.position.x) > 0.1f)
                {
                    direction = playerPosition.x > transform.position.x ? Vector2.right : Vector2.left;
                }
            }
        }
        else
        {
            direction = Vector2.right * m_facingDirection;
        }

        if (m_hitObject && false == isChasingPlayer)
        {
            base.Turn();
            direction *= -1;
        }

        if (base.animator != null && false == isPatternActive)
        {
            if (direction.x != 0 && base.currentSpeed > 0f)
            {
                base.animator.SetBool("isMoving", true);
            }
        }

        Run(direction);
    }

    #region RAYCAST METHODS
    private void CastRay()
    {
        m_hitObject = false;

        CliffCastRay();
        WallRayCast();
    }

    private void CliffCastRay()
    {
        Vector3 groundCheckPosition = transform.position - new Vector3(0, boundsHeight * 0.5f);
        RaycastHit2D hitGround = MyDebug.Raycast(groundCheckPosition, -transform.up, 0.15f, groundLayer, Color.blue, true);
        if (false == hitGround)
        {
            return;
        }

        Vector2 position = m_facingDirection > 0 ?
            new Vector2(boundsCenter.x + (boundsWidth * 0.51f), boundsCenter.y) :
            new Vector2(boundsCenter.x - (boundsWidth * 0.51f), boundsCenter.y);

        position.y = boundsCenter.y - boundsHeight * 0.51f;

        RaycastHit2D hitCliff = MyDebug.Raycast(position, -transform.up, 0.3f, groundLayer, Color.cyan, true);

        if (false == hitCliff)
        {
            m_hitObject = true;
            return;
        }
    }

    private void WallRayCast()
    { 
        Vector2 dir = Vector2.zero;
        dir.x = m_facingDirection;

        float raysDistance = boundsHeight / raysCount;
        for (int i = 0; i < raysCount; i++)
        {
            Vector2 position = m_facingDirection > 0 ?
                new Vector2(boundsCenter.x + (boundsWidth * 0.51f), boundsCenter.y) :
                new Vector2(boundsCenter.x - (boundsWidth * 0.51f), boundsCenter.y);

            position.y = boundsTopLeftCorner.y - (raysDistance * i);

            RaycastHit2D hitWall = MyDebug.Raycast(position, dir, 0.2f, obstacleLayer, Color.blue, true);
            if (hitWall)
            {
                m_hitObject = true;
                return;
            }
        }
    }

    protected override void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectRange, playerLayerMask);
        if (player != null)
        {
            //Debug.Log("충돌");
            Vector2 direction = player.transform.position - transform.position;
            float distance = Vector2.Distance(transform.position, player.transform.position);

            RaycastHit2D hit = MyDebug.Raycast(transform.position, direction.normalized, distance, obstacleLayer, Color.white, true);
            if (false == hit)
            {
                base.target = player.gameObject;
                isChasingPlayer = true;
                playerPosition = player.transform.position;

                base.isAttacking = Physics2D.OverlapCircle(transform.position, attackRange, playerLayerMask);
            }
        }
        else
        {
            base.target = null;
            isChasingPlayer = false;
            playerPosition = Vector2.zero;

            base.isAttacking = false;
        }
    }
    #endregion

    #region MOVE METHODS
    private void Run(Vector2 _direction)
    {
        base.m_previousPosition = m_currentPosition;

        base.currentSpeed = target != null ?
            base.chaseSpeed :
            Random.Range(base.minSpeed, base.maxSpeed);

        Vector2 newPosition = new Vector2(_direction.normalized.x * base.currentSpeed, _direction.normalized.y);
        newPosition *= Time.deltaTime;

        transform.Translate(newPosition, Space.Self);

        base.m_currentPosition = transform.position;
    }
    #endregion

    protected override void OnEnable()
    {
        Initiailization();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_initializePosition, activityRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
