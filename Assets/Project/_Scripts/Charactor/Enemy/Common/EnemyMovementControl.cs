using UnityEngine;
using UnityEngine.PlayerLoop;

public class EnemyMovementControl : CharacterMovement
{
    [Header("플레이어")]
    public LayerMask playerLayerMask;

    [Header("속도")]
    public float minSpeed;
    public float maxSpeed;
    public float chaseSpeed;
    [MyReadOnly]
    public float currentSpeed;

    [Header("추격")]
    public float chaseWaitTime;


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

    public int facingDirection = 1;
    protected Vector2 m_initializePosition;
    protected Vector2 m_currentPosition;
    protected Vector2 m_previousPosition;


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
        m_currentPosition = transform.position;
    }

    protected virtual void Initiailization()
    {
        isFacingRight = true;
        CheckDirectionToFace(isFacingRight);

        isAttackingPlayer = false;
        isFalling = false;
        isStunned = false;

        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    protected virtual void Update()
    {
        facingDirection = isFacingRight ? 1 : -1;

        if (target != null)
        {
            int direction = (int)Mathf.Sign(target.transform.position.x - transform.position.x);

            CheckDirectionToFace(direction > 0);
        }
        else
        {
            facingDirection = m_currentPosition.x - m_previousPosition.x > 0 ? 1 : -1;

            CheckDirectionToFace(facingDirection > 0);
        }
    }

    public virtual void DirectionToFace(bool _direction)
    {
        isFacingRight = _direction;
        facingDirection = isFacingRight ? 1 : -1;

        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public virtual void CheckDirectionToFace(bool _isMovingRight)
    {
        if (_isMovingRight != isFacingRight)
        {
            Turn();
        }
    }

    protected virtual void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;

        transform.localScale = scale;

        isFacingRight = !isFacingRight;
        facingDirection = isFacingRight ? 1 : -1;
    }

    protected virtual void OnEnable()
    {
        Initiailization();
    }

    protected virtual void OnDisable()
    {

    }
}
