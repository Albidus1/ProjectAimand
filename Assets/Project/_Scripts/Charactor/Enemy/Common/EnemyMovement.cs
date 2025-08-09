using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : EnemyMovementControl
{
    [Header("레이캐스트")]
    public Transform checkHoles;
    public Vector3 colliderSize => Vector3.Scale(transform.localScale, boxCollider.size);
    public Vector3 colliderCenterPosition => boxCollider.bounds.center;

    [Header("레이어")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    

    private bool hitObject;

    //private RaycastHit2D[] sideHitsStorage;
    //private RaycastHit2D[] belowHitsStorage;

    //private Vector2 horizontalRaycastFromBottom = Vector2.zero;
    //private Vector2 horizontalRaycastToTop = Vector2.zero;
    //private Vector2 verticalRaycastFromLeft = Vector2.zero;
    //private Vector2 verticalRaycastToRight = Vector2.zero;
    //private Vector2 raycastOrigin = Vector2.zero;

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

    protected override void Update()
    {
        if (base.patternController != null && base.patternController.patternCooldownTimer > 0)
        {
            Run(Vector2.zero);
            return;
        }

        facingDirection = isFacingRight ? 1 : -1;

        SetRaysParameters();

        CastRay();
        DetectPlayer();

        base.Update();
    }

    private void FixedUpdate()
    {
        Vector2 direction;

        if (isAttackingPlayer)
        {
            return;
        }

        if (isChasingPlayer)
        {
            if (hitObject)
            {
                Run(Vector2.zero);
            }
            else
            {
                if (Mathf.Abs(playerPosition.x - transform.position.x) > 0.1f)
                {
                    direction = playerPosition.x > transform.position.x ? Vector2.right : Vector2.left;
                    CheckDirectionToFace(playerPosition.x > transform.position.x);

                    Run(direction);
                }
                else
                {
                    Run(Vector2.zero);
                }
            }
        }
        else
        {
            direction = Vector2.right * facingDirection;
            Run(direction);
        }

        if (hitObject && false == isChasingPlayer)
        {
            Turn();
        }
    }

    #region INITIALIZATION
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

    #region RAYCAST METHODS
    private void CastRay()
    {
        hitObject = false;

        if (rb.linearVelocity.y == 0)
        {
            RaycastHit2D hitHole = MyDebug.Raycast(checkHoles.position, transform.up, 0.5f, groundLayer, Color.blue, true);

            if (false == hitHole)
            {
                hitObject = true;
            }
        }


        Vector2 dir = Vector2.zero;
        dir.x = facingDirection;

        int raysCount = 5;
        float raysDistance = boundsHeight / raysCount;
        for (int i = 0; i < raysCount; i++)
        {
            Vector2 position = facingDirection > 0 ?
                new Vector2((boundsCenter.x + boundsWidth * 0.51f), boundsCenter.y) :
                new Vector2((boundsCenter.x - boundsWidth * 0.51f), boundsCenter.y);

            position.y = boundsTopLeftCorner.y - (raysDistance * i);

            RaycastHit2D hitWall = MyDebug.Raycast(position, dir, 0.2f, LayerManager.obstacleLayerMask, Color.blue, true);
            if (hitWall)
            {
                hitObject = true;
                break;
            }
        }
    }

    private void DetectPlayer()
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
                target = player.gameObject;
                isChasingPlayer = true;
                playerPosition = player.transform.position;
            }
        }
        else
        {
            target = null;
            isChasingPlayer = false;
            playerPosition = Vector2.zero;
        }

        if (Physics2D.OverlapCircle(transform.position, attackRange, playerLayerMask))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }
    }
    #endregion

    #region MOVE METHODS
    private void Run(Vector2 _direction)
    {
        float speed = target != null ? base.chaseSpeed : normalSpeed;

        Vector2 newPosition = new Vector2(_direction.normalized.x * speed, _direction.normalized.y);
        newPosition *= Time.deltaTime;

        transform.Translate(newPosition, Space.Self);
    }
    #endregion

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
