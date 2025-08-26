using UnityEngine;

public class EnemyMovementFly : EnemyMovementControl
{
    public float minScanTick = 0.5f;
    public float maxScanTick = 2f;
    [MyReadOnly]
    public float currentScanTick = 0.5f;

    protected Vector2 m_moveDirection;
    protected Vector2 m_targetPosition;
    protected float m_currentSpeed;
    protected float m_chaseWaitTime = -0.1f;
    protected float m_scanTimer = 0f;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        m_initializePosition = transform.position;
        currentScanTick = GetPatrolTick();
    }

    protected override void Update()
    {
        if (target != null && Mathf.Abs(target.transform.position.x - transform.position.x) > 0.1f)
        {
            facingDirection = target.transform.position.x < transform.position.x ? 1 : -1;
        }
        else
        {
            facingDirection = m_moveDirection.x < 0 ? 1 : -1;
        }

        DetectPlayer();
        base.Update();
    }

    private void FixedUpdate()
    {
        if (false == isAttacking && m_scanTimer + currentScanTick < Time.time)
        {
            currentScanTick = GetPatrolTick();
            m_scanTimer = Time.time;

            while (true)
            {
                m_targetPosition = MyMaths.GetRandomPointInCircle(activityRange);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, m_targetPosition.normalized, m_targetPosition.magnitude, LayerManager.obstacleLayerMask);

                if (false == hit)
                {
                    Debug.Log(hit.point);
                    break;
                }
            }
        }

        Move();
    }

    private float GetPatrolTick()
    {
        return Random.Range(minScanTick, maxScanTick);
    }

    private void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectRange, playerLayerMask);

        if (player != null)
        {
            target = player.gameObject;
            m_chaseWaitTime = Time.time + chaseWaitTime;

            base.animator.SetBool("isChasing", true);
        }
        else
        {
            target = null;

            base.animator.SetBool("isChasing", false);
        }

        CalculateDirection();

        if (Physics2D.OverlapCircle(transform.position, attackRange, playerLayerMask))
        {
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }
    }

    private void Move()
    {
        if (Vector2.Distance(transform.position, m_targetPosition) > 0.1)
        {
            Vector2 newPosition = m_moveDirection * m_currentSpeed;
            transform.Translate(newPosition * Time.deltaTime);
        }
    }

    private void CalculateDirection()
    {
        if (target != null && false == isStunned)
        {
            m_targetPosition = (Vector2)target.transform.position;
            m_moveDirection = (m_targetPosition - (Vector2)transform.position).normalized;
            m_currentSpeed = chaseSpeed;
        }
        else if (m_chaseWaitTime < Time.time && false == isStunned)
        {
            m_targetPosition = m_initializePosition;
            m_moveDirection = (m_targetPosition - (Vector2)transform.position).normalized;
            m_currentSpeed = normalSpeed;
        }
        else
        {
            m_targetPosition = transform.position;
            m_moveDirection = Vector2.zero;
            m_currentSpeed = 0;
        }
    }


    protected override void OnEnable()
    {
        base.OnEnable();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        m_initializePosition = transform.position;
        detectRange = Mathf.Clamp(detectRange, detectRange, activityRange);
        attackRange = Mathf.Clamp(attackRange, attackRange, detectRange);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_initializePosition, activityRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
