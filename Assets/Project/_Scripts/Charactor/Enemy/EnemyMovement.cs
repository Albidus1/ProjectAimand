using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : MonoBehaviour
{
    private int facingDirection = 1;
    public bool isFacingRight { get; private set; }
    public bool isFalling { get; private set; }


    [Header("스피드")]
    public float speed;

    [Header("레이캐스트")]
    public Transform checkHoles;
    public Vector3 colliderSize => Vector3.Scale(transform.localScale, boxCollider.size);
    public Vector3 colliderCenterPosition => boxCollider.bounds.center;

    [Header("레이어")]
    [SerializeField] private LayerMask groundLayer;

    

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private bool AtEdge;
    private bool hitObject;

    //private RaycastHit2D[] sideHitsStorage;
    //private RaycastHit2D[] belowHitsStorage;

    //private Vector2 horizontalRaycastFromBottom = Vector2.zero;
    //private Vector2 horizontalRaycastToTop = Vector2.zero;
    //private Vector2 verticalRaycastFromLeft = Vector2.zero;
    //private Vector2 verticalRaycastToRight = Vector2.zero;
    //private Vector2 raycastOrigin = Vector2.zero;

    private Vector2 bounds;
    private Vector2 boundsCenter;
    private Vector2 boundsTopLeftCorner;
    private Vector2 boundsBottomLeftCorner;
    private Vector2 boundsTopRightCorner;
    private Vector2 boundsBottomRightCorner;
    private float boundsWidth;
    private float boundsHeight;

    private bool isChasingPlayer;
    private Vector2 playerPosition;
    private Vector2 previousPlayerPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        isFacingRight = true;
        CheckDirectionToFace(isFacingRight);
        SetRaysParameters();
    }

    private void Update()
    {
        CheckDirectionToFace(facingDirection > 0);

        SetRaysParameters();

        CastRay();
        DetectPlayer();
    }

    private void FixedUpdate()
    {
        Vector2 direction;

        if (isChasingPlayer && hitObject)
        {
            Run(Vector2.zero);
        }
        else if (isChasingPlayer && false == hitObject)
        {
            direction = playerPosition.x > transform.position.x ? Vector2.right : Vector2.left;
            CheckDirectionToFace(playerPosition.x > transform.position.x);

            Run(direction);
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

    private void CheckDirectionToFace(bool _isMovingRight)
    {
        if (_isMovingRight != isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
        facingDirection = -facingDirection;
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
            Vector2 position = boundsCenter;
            position.y = boundsTopLeftCorner.y - (raysDistance * i);

            RaycastHit2D hitWall = MyDebug.Raycast(position, dir, boundsWidth * 0.5f + 0.05f, groundLayer, Color.blue, true);
            if (hitWall)
            {
                hitObject = true;
                break;
            }
        }
    }

    public float sightRange = 10f;
    private void DetectPlayer()
    {
        Vector2 boxSize = new Vector2(boundsWidth, boundsHeight);

        RaycastHit2D hit = MyDebug.BoxCast(boundsCenter, boxSize, Vector2.Angle(transform.up, Vector2.up), transform.right * facingDirection, sightRange, LayerMask.GetMask("Player"), Color.cyan, true);

        if (hit)
        {
            Debug.Log("충돌");

            isChasingPlayer = true;

            playerPosition = hit.collider.transform.position;
        }
        else
        {
            isChasingPlayer = false;

            playerPosition = Vector2.zero;
        }
    }
    #endregion

    #region MOVE METHODS
    private void Run(Vector2 _direction)
    {
        Vector2 newPosition = _direction.normalized * speed * Time.deltaTime;

        transform.Translate(newPosition, Space.Self);
    }
    #endregion
}
