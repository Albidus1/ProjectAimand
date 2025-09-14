using TMPro;
using UnityEngine;

public class EnemyMovementFly : EnemyMovementControl
{
    public float minScanTick = 0.5f;
    public float maxScanTick = 2f;
    [MyReadOnly]
    public float currentScanTick = 0.5f;

    protected Vector2 m_moveDirection;
    protected Vector2 m_targetPosition;
    protected float m_chaseWaitTime = -0.1f;
    protected float m_scanTimer = 0f;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        GetRandomPosition();
        currentScanTick = GetPatrolTick();
    }

    protected override void Update()
    {
        base.Update();
        DetectPlayer();
        Move();
    }

    protected override void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, base.detectRange, playerLayerMask);

        if (player != null)
        {
            target = player.gameObject;
            m_targetPosition = target.transform.position;
            m_chaseWaitTime = Time.time + chaseWaitTime;
            base.currentSpeed = chaseSpeed;

            base.animator.SetBool("isChasing", true);
        }
        else if (false == isMoving && false == isAttacking &&
            m_scanTimer + currentScanTick < Time.time)
        {
            currentScanTick = GetPatrolTick();
            m_scanTimer = Time.time;
            base.currentSpeed = Random.Range(minSpeed, maxSpeed);

            GetRandomPosition();

            target = null;

            base.animator.SetBool("isChasing", false);
        }

        isAttacking = Physics2D.OverlapCircle(transform.position, base.attackRange, playerLayerMask);
    }
    protected override void Move()
    {
        if (base.isStunned || base.m_doKnockback)
        {
            return;
        }

        if (Vector2.Distance(transform.position, m_targetPosition) > 0.1)
        {
            base.m_previousPosition = base.m_currentPosition;

            m_moveDirection = (m_targetPosition - (Vector2)transform.position).normalized;
            Vector2 newPosition = m_moveDirection * currentSpeed;
            transform.Translate(newPosition * Time.deltaTime);

            base.m_currentPosition = transform.position;

            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private void CalculateDirection()
    {
/*        if (target != null && false == isStunned)
        {
            m_targetPosition = (Vector2)target.transform.position;
            m_moveDirection = (m_targetPosition - (Vector2)transform.position).normalized;
            m_currentSpeed = chaseSpeed;
        }
        else if (m_chaseWaitTime < Time.time && false == isStunned)
        {
            m_targetPosition = m_initializePosition;
            m_moveDirection = (m_targetPosition - (Vector2)transform.position).normalized;
            m_currentSpeed = currentSpeed;
        }
        else
        {
            m_targetPosition = transform.position;
            m_moveDirection = Vector2.zero;
            m_currentSpeed = 0;
        }*/
    }

    private float GetPatrolTick()
    {
        return Random.Range(minScanTick, maxScanTick);
    }


    private void GetRandomPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(0f, base.activityRange);

            RaycastHit2D hit = Physics2D.Raycast(base.m_initializePosition, randomDirection, randomDistance, LayerManager.obstacleLayerMask);

            if (false == hit)
            {
                m_targetPosition = base.m_initializePosition + randomDirection * randomDistance;
                //Debug.Log(m_targetPosition);
                break;
            }
            else
            {
                m_targetPosition = transform.position;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        base.m_initializePosition = transform.position;
        detectRange = Mathf.Clamp(detectRange, detectRange, activityRange);
        attackRange = Mathf.Clamp(attackRange, attackRange, detectRange);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(base.m_initializePosition, base.activityRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, base.detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, base.attackRange);
    }
#endif
}
