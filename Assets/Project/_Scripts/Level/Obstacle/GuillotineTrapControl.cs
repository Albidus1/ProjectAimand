using UnityEngine;



public class GuillotineTrapControl : MonoBehaviour, IEventListener<TriggerEvent>, Respawnable
{
    public enum BladeState {Preparation, BeforeFalling, Falling, WaitAtBottom, Resetting, Cooldown }
    [MyReadOnly]
    public BladeState currentState;

    [MyReadOnly]
    public bool isActive = true;

    [Header("사이클 시간 설정")]
    public float initialCooldownTime;
    public float preparationTime;
    public float waitAtBottomTime;
    public float cooldownTime;

    [Header("속도 설정")]
    public float fallSpeed;
    public float resetSpeed;

    [Header("레이캐스트 설정")]
    public int numberOfVerticalRays = 4;
    public float rayDistance = 10f;
    public LayerMask groundLayer;

    [Header("이벤트 설정")]
    public bool useTriggerEvent = false;
    [MyConditionalHide("useTriggerEvent", true)]
    public string eventID = "default";


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

    private float m_initialCooldownTimer;
    private float m_preparationTimer;
    private float m_waitAtBottomTimer;
    private float m_cooldownTimer;
    private Vector2 m_initialPosition;
    private Vector2 m_targetPosition;



    private void Awake()
    {
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_belowHitsStorage = new RaycastHit2D[numberOfVerticalRays];
    }

    private void Start()
    {
        groundLayer |= LayerManager.platformsLayerMask;
        m_initialPosition = transform.position;
        
        Initialization();
    }

    private void Initialization()
    {
        SetRaysParameters();

        m_boxCollider.enabled = false;

        transform.position = m_initialPosition;
        m_targetPosition = m_initialPosition;
        m_initialCooldownTimer = initialCooldownTime;
        m_preparationTimer = preparationTime;
        m_waitAtBottomTimer = waitAtBottomTime;
        m_cooldownTimer = cooldownTime;

        currentState = BladeState.Preparation;

        isActive = false == useTriggerEvent;
    }

    private void Update()
    {
        m_initialCooldownTimer -= Time.deltaTime;
        if (m_initialCooldownTimer > 0)
        {
            return;
        }

        BladeMovement();
    }

    private void BladeMovement()
    {
        switch (currentState)
        {
            case BladeState.Preparation:
                Preparation();
                break;

            case BladeState.BeforeFalling:
                BeforeFalling();
                break;

            case BladeState.Falling:
                Falling();
                break;

            case BladeState.WaitAtBottom:
                WaitAtBottom();
                break;

            case BladeState.Resetting:
                Resetting();
                break;

            case BladeState.Cooldown:
                Cooldown();
                break;

            default:
                break;
        }
    }

    private void Preparation()
    {
        if (false == isActive)
        {
            return;
        }

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
            rayOriginPoint.y = m_boundsBottomLeftCorner.y + 0.001f;

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
            m_targetPosition.y = m_belowHitsStorage[smallestDistanceIndex].point.y + m_boundsHeight * 0.5f;
            m_boxCollider.enabled = false;
            currentState = BladeState.BeforeFalling;
        }
    }

    private void BeforeFalling()
    {
        transform.Translate(Vector2.up * Time.deltaTime);

        if (transform.position.y >= m_initialPosition.y + 0.5f)
        {
            transform.position = new Vector2(transform.position.x, m_initialPosition.y + 0.5f);
        }

        m_preparationTimer -= Time.deltaTime;

        if (m_preparationTimer <= 0f)
        {
            m_preparationTimer = preparationTime;
            currentState = BladeState.Falling;
        }
    }

    private void Falling()
    {
        m_boxCollider.enabled = true;
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        if (m_targetPosition.y >= transform.position.y)
        {
            transform.position = m_targetPosition;
            m_boxCollider.enabled = false;

            currentState = BladeState.WaitAtBottom;
        }
    }

    private void WaitAtBottom()
    {
        m_waitAtBottomTimer -= Time.deltaTime;

        if (m_waitAtBottomTimer <= 0f)
        {
            m_waitAtBottomTimer = waitAtBottomTime;
            currentState = BladeState.Resetting;
        }
    }

    private void Resetting()
    {
        transform.Translate(Vector2.up * resetSpeed * Time.deltaTime);

        if (m_initialPosition.y <= transform.position.y)
        {
            transform.position = m_initialPosition;
            currentState = BladeState.Cooldown;
        }
    }

    private void Cooldown()
    {
        m_cooldownTimer -= Time.deltaTime;

        if (m_cooldownTimer <= 0f)
        {
            m_cooldownTimer = cooldownTime;
            currentState = BladeState.Preparation;
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

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Initialization();
    }

    public void OnEvent(TriggerEvent e)
    {
        if (e.eventID != this.eventID)
        {
            return;
        }

        isActive = true;
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
