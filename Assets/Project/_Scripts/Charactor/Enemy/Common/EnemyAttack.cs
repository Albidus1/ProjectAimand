using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Enemy enemyMovement;

    public Health health;

    [Header("범위 - 지상")]
    public float chaseRange = 12f;
    public Transform chaseRangeCenter;
    public float attackRange = 6f;
    public Transform attackRangeCenter;

    [Header("자폭")]
    public bool isSelfDestruct = false;
    public float selfDestructWaitTime = 2f;
    public float selfDestructRange = 8f;
    public float selfDestructDamage = 80f;

    [Header("타이머")]
    public float attackTime;

    private float m_attackTime;
    private bool doSelfDestruct;
    private float m_selfDestructTimer = 0.1f;


    private void Awake()
    {
        health = GetComponent<Health>();

        enemyMovement = GetComponent<EnemyMovement>();
        enemyMovement = enemyMovement == null ? GetComponent<EnemyMovementFly>() : enemyMovement;

        Initialize();
    }

    private void Initialize()
    {
        if (enemyMovement != null)
        {
            enemyMovement.detectRange = chaseRange;
            enemyMovement.attackRange = attackRange;
            //enemyMovement.chaseRangePosition = chaseRangeCenter.position;
            //enemyMovement.attackRangePosition = attackRangeCenter.position;
        }
    }

    private void Update()
    {
        if (SelfDestruct())
        {
            enabled = false;
            return;
        }

        if (m_attackTime < Time.time && enemyMovement.isAttacking)
        {
            Attack();
        }
    }

    private bool SelfDestruct()
    {
        if (false == doSelfDestruct)
        {
            return false;
        }

        if (m_selfDestructTimer > Time.time)
        {
            return false;
        }

        Debug.Log("자폭");

        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, selfDestructRange);

        foreach (Collider2D obj in col)
        {
            Health health = obj.GetComponent<Health>();

            if (health != null)
            {
                health.currentHP -= selfDestructDamage;
                Debug.Log($"{obj.name}에게 {selfDestructDamage}의 피해");
            }
        }

        health.Kill();

        return true;
    }

    private void Attack()
    {
        m_attackTime = Time.time + attackTime;

        if (false == isSelfDestruct)
        {
            Debug.Log("적 공격");
        }
        else if (false == doSelfDestruct)
        {
            Debug.Log("자폭 시작");

            doSelfDestruct = true;
            m_selfDestructTimer = Time.time + selfDestructWaitTime;

            enemyMovement.isStunned = true;
        }
    }

    private void OnValidate()
    {
        attackRange = Mathf.Clamp(attackRange, 0, chaseRange - 0.5f);

        if (enemyMovement == null)
        {
            enemyMovement = GetComponent<EnemyMovement>();

            Initialize();
        }
        else
        {
            Initialize();
        }
    }

    private void OnDrawGizmos()
    {
        if (isSelfDestruct)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, selfDestructRange);
        }
    }
}
