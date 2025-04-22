using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private int facingDirection = 1;
    public bool isFacingRight { get; private set; } = true;
    public bool isFalling { get; private set; }


    [Header("스피드")]
    public float speed;

    [Header("레이캐스트")]
    public Transform checkHoles;

    [Header("레이어")]
    [SerializeField] private LayerMask groundLayer;

    public Vector3 colliderSize => Vector3.Scale(transform.localScale, boxCollider.size);
    public Vector3 colliderCenterPosition => boxCollider.bounds.center;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

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


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        SetRaysParameters();
    }

    private void Update()
    {
        CheckDirectionToFace(facingDirection > 0);

        SetRaysParameters();

        CastRay();
    }

    private void FixedUpdate()
    {
        Vector2 moveDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 newPosition = moveDirection * speed * Time.deltaTime;

        transform.Translate(newPosition, Space.Self);
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
        if (rb.linearVelocity.y == 0)
        {
            RaycastHit2D hitHole = MyDebug.Raycast(checkHoles.position, transform.up, 0.5f, groundLayer, Color.blue, true);
            if (false == hitHole)
            {
                Turn();
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

            RaycastHit2D hitWall = MyDebug.Raycast(position, dir, boundsWidth * 0.5f + 0.5f, groundLayer, Color.blue, true);
            if (true == hitWall)
            {
                Turn();
                break;
            }
        }
    }

    private void CastPlayer()
    {
        //MyDebug.BoxCast(boundsCenter, new Vector2(boundsWidth, boundsHeight), Vector2.Angle(transform.up, Vector2.up), -transform.up, 10f, LayerMask.GetMask("Player"), Color.);
    }
    #endregion
}
