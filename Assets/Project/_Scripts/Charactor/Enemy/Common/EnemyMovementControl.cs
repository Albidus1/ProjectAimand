using UnityEngine;

public class EnemyMovementControl : CharacterMovement
{
    [Header("플레이어")]
    public LayerMask playerLayerMask;

    [Header("속도")]
    public float normalSpeed;
    public float chaseSpeed;

    [Header("추격")]
    public float chaseWaitTime;


    public bool isFacingRight { get; set; }
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

    protected int facingDirection = 1;
    protected Vector2 m_initializePosition;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        patternController = GetComponent<EnemyPatternController>();

        isFacingRight = true;
        CheckDirectionToFace(isFacingRight);
    }

    protected virtual void Update()
    {
        if (target != null)
        {
            CheckDirectionToFace(target.transform.position.x > transform.position.x);
        }
        else
        {
            CheckDirectionToFace(facingDirection > 0);
        }
    }


    protected virtual void CheckDirectionToFace(bool _isMovingRight)
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
        facingDirection = -facingDirection;
    }
}
