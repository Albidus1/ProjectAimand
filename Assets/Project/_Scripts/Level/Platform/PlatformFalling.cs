using UnityEngine;




public class PlatformFalling : MonoBehaviour, Respawnable, IEventListener<TriggerEvent>
{
    public Transform spriteTransform;

    [Header("낙하 설정")]
    public bool fallingOnStart;
    public float fallSpeed = 3f;
    [Tooltip("가속 적용")]
    public bool applyAcceleration;
    [MyConditionalHide("applyAcceleration", true)]
    [Tooltip("초기 속도")]
    public float initialSpeed = 1f;
    [MyConditionalHide("applyAcceleration", true)]
    public float acceleration = 9.8f;

    [Header("쉐이킹 효과")]
    public bool applyShaking;
    [MyConditionalHide("applyShaking", true)]
    [Tooltip("흔들림 폭")]
    public float shakeIntensity = 0.05f;
    [MyConditionalHide("applyShaking", true)]
    [Tooltip("흔들림 속도")]
    public float shakeSpeed = 50f;

    [Header("레이캐스트")]
    public LayerMask groundLayer;
    public int numberOfVerticalRays = 5;
    public float rayDistance = 0.5f;

    [Header("이벤트")]
    public bool useEvent = false;
    [MyConditionalHide("useEvent", true)]
    public string eventID = "default";




    private bool m_initialized = false;
    private bool isFalling = false;

    private float m_currentFallSpeed;

    private Vector2 m_initialPosition;
    private Vector3 m_spritePosition;
    private Vector3 m_endPosition;

    private RaycastHit2D[] m_belowHitsStorage;
    private BoxCollider2D m_boxCollider;
    private Vector2 m_boundsCenter;
    private Vector2 m_boundsTopLeftCorner;
    private Vector2 m_boundsBottomLeftCorner;
    private Vector2 m_boundsTopRightCorner;
    private Vector2 m_boundsBottomRightCorner;
    private float m_boundsWidth;
    private float m_boundsHeight;
    private Vector2 m_virticalRaycastFromLeft;
    private Vector2 m_virticalRaycastToRight;



    private void Awake()
    {
        m_boxCollider = GetComponent<BoxCollider2D>();

        m_belowHitsStorage = new RaycastHit2D[numberOfVerticalRays];
    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (false ==  m_initialized)
        {
            m_initialPosition = transform.position;
            m_endPosition = new Vector2(0, int.MinValue);

            m_initialized = true;
        }

        transform.position = m_initialPosition;

        isFalling = (false == useEvent) && fallingOnStart;
    }

    private void Update()
    {
        if (false == isFalling)
        {
            return;
        }

        SetRaysParameters();
        RaycastBelow();

        if (applyAcceleration &&
            m_currentFallSpeed < fallSpeed)
        {
            m_currentFallSpeed += acceleration * Time.deltaTime;

        }
        else
        {
            m_currentFallSpeed = fallSpeed;
        }

        transform.Translate(Vector3.down * m_currentFallSpeed * Time.deltaTime, Space.World);
        //transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);

        if (spriteTransform != null && applyShaking)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            spriteTransform.localPosition = m_spritePosition + new Vector3(offsetX, 0f, 0f);
        }

        if (m_endPosition.y >= transform.position.y)
        {
            isFalling = false;
            CameraShakeEvent.Trigger("MainCamera", 0.2f, 1f, 30f);
        }
    }

    private void SetRaysParameters()
    {
        //Debug.Log(boxCollider.size);
        float x = m_boxCollider.size.x;
        float y = m_boxCollider.size.y;

        float right = x * 0.5f;
        float left = -x * 0.5f;
        float top = y * 0.5f;
        float bottom = -y * 0.5f;


        m_boundsCenter = m_boxCollider.bounds.center;

        m_boundsTopLeftCorner.x = left;
        m_boundsTopLeftCorner.y = top;

        m_boundsBottomLeftCorner.x = left;
        m_boundsBottomLeftCorner.y = bottom;

        m_boundsTopRightCorner.x = right;
        m_boundsTopRightCorner.y = top;

        m_boundsBottomRightCorner.x = right;
        m_boundsBottomRightCorner.y = bottom;

        m_boundsTopLeftCorner = transform.TransformPoint(m_boundsTopLeftCorner);
        m_boundsBottomLeftCorner = transform.TransformPoint(m_boundsBottomLeftCorner);
        m_boundsTopRightCorner = transform.TransformPoint(m_boundsTopRightCorner);
        m_boundsBottomRightCorner = transform.TransformPoint(m_boundsBottomRightCorner);

        m_boundsWidth = Vector2.Distance(m_boundsTopLeftCorner, m_boundsTopRightCorner);
        m_boundsHeight = Vector2.Distance(m_boundsTopLeftCorner, m_boundsBottomLeftCorner);
    }

    private void RaycastBelow()
    {
        m_virticalRaycastFromLeft = (m_boundsBottomLeftCorner + m_boundsTopLeftCorner) * 0.5f;
        m_virticalRaycastToRight = (m_boundsBottomRightCorner + m_boundsTopRightCorner) * 0.5f;
        m_virticalRaycastFromLeft += (Vector2)transform.up * 0.01f;
        m_virticalRaycastToRight += (Vector2)transform.up * 0.01f;

        if (m_belowHitsStorage.Length != numberOfVerticalRays)
        {
            m_belowHitsStorage = new RaycastHit2D[numberOfVerticalRays];
        }

        float smallestDistance = float.MaxValue;
        int smallestDistanceIndex = 0;
        bool hitConnected = false;
        for (int i = 0; i < numberOfVerticalRays; i++)
        {
            Vector2 rayOriginPoint = Vector2.Lerp(m_virticalRaycastFromLeft, m_virticalRaycastToRight, (float)i / (numberOfVerticalRays - 1));
            rayOriginPoint.y = m_boundsBottomLeftCorner.y - 0.001f;

            m_belowHitsStorage[i] = MyDebug.Raycast(rayOriginPoint, Vector2.down, rayDistance, groundLayer, Color.blue, true);

            float distance = MyMaths.DistanceBetweenPointAndLine(m_belowHitsStorage[i].point, m_virticalRaycastFromLeft, m_virticalRaycastToRight);

            if (m_belowHitsStorage[i])
            {
                hitConnected = true;

                if (m_belowHitsStorage[i].distance < smallestDistance)
                {
                    smallestDistance = m_belowHitsStorage[i].distance;
                    smallestDistanceIndex = i;
                }
            }

            if (distance < 0.0001f)
            {
                break;
            }
        }

        if (hitConnected)
        {
            m_endPosition.y = m_belowHitsStorage[smallestDistanceIndex].point.y + m_boundsHeight * 0.5f;
            //Debug.Log(m_belowHitsStorage[smallestDistanceIndex].collider.name);
        }
    }

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Initialization();
    }

    public void OnEvent(TriggerEvent _eventType)
    {
        if (false ==  useEvent)
        {
            return;
        }

        if (_eventType.eventID != eventID)
        {
            return;
        }

        isFalling = true;
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<TriggerEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<TriggerEvent>();
    }
}
