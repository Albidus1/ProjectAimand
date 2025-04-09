using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public bool isFacingRight = true;
    public int facingDirection = 1;

    [Header("스피드")]
    public float speed;

    [Header("레이캐스트")]
    public int numOfHorizontalRay = 4;
    public int numOfVerticalRay = 4;
    public float RayOffsetHorizontal = 0.05f;
    public float RayOffsetVertical = 0.05f;

    [Header("레이어")]
    [SerializeField] private LayerMask groundLayer;

    public Vector3 colliderSize => Vector3.Scale(transform.localScale, boxCollider.size);
    public Vector3 colliderCenterPosition => boxCollider.bounds.center;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private RaycastHit2D[] sideHitsStorage;
    private RaycastHit2D[] belowHitsStorage;

    private Vector2 horizontalRaycastFromBottom = Vector2.zero;
    private Vector2 horizontalRaycastToTop = Vector2.zero;
    private Vector2 verticalRaycastFromLeft = Vector2.zero;
    private Vector2 verticalRaycastToRight = Vector2.zero;
    private Vector2 raycastOrigin = Vector2.zero;

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

        sideHitsStorage = new RaycastHit2D[numOfHorizontalRay];
        belowHitsStorage = new RaycastHit2D[numOfVerticalRay];

        SetRaysParameters();
    }

    private void Update()
    {
        CheckDirectionToFace(facingDirection > 0);

        SetRaysParameters();

        CastRaysLeft();
        CastRaysRight();
        CastRaysBelow();
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
        float right = boxCollider.size.x * 0.5f;
        float left = -boxCollider.size.x * 0.5f;
        float top = boxCollider.size.y * 0.5f;
        float bottom = -boxCollider.size.y * 0.5f;

        boundsCenter = boxCollider.bounds.center;

        boundsTopLeftCorner.x = left;
        boundsTopLeftCorner.y = top;

        boundsBottomLeftCorner.x = left;
        boundsBottomLeftCorner.y = bottom;

        boundsTopRightCorner.x = right;
        boundsTopRightCorner.y = top;

        boundsBottomRightCorner.x = right;
        boundsBottomRightCorner.y = bottom;

        boundsCenter = boxCollider.bounds.center;
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
    private void CastRaysLeft()
    {
        CastRaysToTheSides(-1);
    }

    private void CastRaysRight()
    {
        CastRaysToTheSides(1);
    }

    private void CastRaysToTheSides(int _rayDirection)
    {
        
    }

    private void CastRaysBelow()
    {
        float rayLenth = boundsHeight * 0.5f + RayOffsetVertical;

        verticalRaycastFromLeft = (boundsTopLeftCorner + boundsBottomLeftCorner) * 0.5f;
        verticalRaycastToRight = (boundsTopRightCorner + boundsBottomRightCorner) * 0.5f;
        verticalRaycastFromLeft += (Vector2)transform.up * RayOffsetVertical;
        verticalRaycastToRight += (Vector2)transform.up * RayOffsetVertical;
        verticalRaycastFromLeft += (Vector2)transform.right * rb.linearVelocity;
        verticalRaycastToRight += (Vector2)transform.right * rb.linearVelocity;

        if (belowHitsStorage.Length != numOfVerticalRay)
        {
            belowHitsStorage = new RaycastHit2D[numOfVerticalRay];
        }

        for (int i = 0; i < numOfVerticalRay; i++)
        {
            Vector2 rayOriginPoint = Vector2.Lerp(verticalRaycastFromLeft, verticalRaycastToRight, (float)i / (float)(numOfVerticalRay - 1));

            belowHitsStorage[i] = MyDebug.Raycast(rayOriginPoint, -transform.up, rayLenth, groundLayer, Color.blue, true);
        }
    }
    #endregion
}
