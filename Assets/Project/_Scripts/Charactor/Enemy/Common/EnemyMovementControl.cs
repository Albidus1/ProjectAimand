using UnityEngine;




public abstract class EnemyMovementControl : CharacterMovement
{
    public bool initialFacingRight = true;

    [Header("속도")]
    public float minSpeed;
    public float maxSpeed;
    public float chaseSpeed;
    [MyReadOnly]
    public float currentSpeed;

    [Header("추격")]
    public float chaseWaitTime;

    [Header("레이어")]
    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected LayerMask groundLayer = LayerManager.platformsLayerMask;
    [SerializeField] protected LayerMask obstacleLayer = LayerManager.obstacleLayerMask;

    public bool isFacingRight { get; set; }
    public bool isMoving { get; protected set; }
    public bool isFalling { get; protected set; }
    public bool isAttacking { get; protected set; }
    public bool isAttackingPlayer { get; set; }
    public bool isStunned { get; set; }

    public float activityRange { get; set; }
    public float detectRange { get; set; }
    public float attackRange { get; set; }


    public GameObject target { get; protected set; }
    public Rigidbody2D rb { get; protected set; }
    public BoxCollider2D boxCollider { get; protected set; }
    public EnemyPatternController patternController { get; protected set; }
    public Animator animator { get; protected set; }

    protected int m_facingDirection = 1;
    protected Vector2 m_initializePosition;
    protected Vector2 m_currentPosition;
    protected Vector2 m_previousPosition;
    protected Vector2 m_speed;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        patternController = GetComponent<EnemyPatternController>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        Initiailization();

        m_initializePosition = transform.position;
    }

    protected virtual void Initiailization()
    {
        m_currentPosition = transform.position;

        isFacingRight = initialFacingRight;
        DirectionToFace(isFacingRight);

        isAttacking = false;
        isAttackingPlayer = false;
        isFalling = false;
        isStunned = false;

        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    protected virtual void Update()
    {
        DetectPlayer();
        HandleFacing();
        UpdateSpeed();
    }

    protected void HandleFacing()
    {
        if (target != null)
        {
            m_facingDirection = (int)Mathf.Sign(target.transform.position.x - transform.position.x);
        }
        else
        {
            m_facingDirection = (int)Mathf.Sign(m_currentPosition.x - m_previousPosition.x);
        }

        DirectionToFace(m_facingDirection > 0);
    }

    protected void UpdateSpeed()
    {
        m_speed = m_currentPosition - m_previousPosition;
    }

    protected abstract void Move();
    protected abstract void DetectPlayer();

    public virtual void DirectionToFace(bool _direction)
    {
        isFacingRight = _direction;
        m_facingDirection = isFacingRight ? 1 : -1;

        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    protected virtual void Turn()
    {
        isFacingRight = !isFacingRight;
        m_facingDirection *= -1; 

        Vector3 scale = transform.localScale;
        scale.x *= -1; 
        transform.localScale = scale;
    }

    protected virtual void OnEnable()
    {
        Initiailization();
    }

    protected virtual void OnDisable()
    {

    }
}
