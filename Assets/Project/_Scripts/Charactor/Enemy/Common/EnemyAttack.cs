using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyMovementControl enemyMovement;

    public Health health;

    [Header("범위")]
    public float activityRange = 13f;
    public float detectRange = 12f;
    public float attackRange = 6f;

    [Header("자폭")]
    public bool isSelfDestruct = false;
    public float selfDestructWaitTime = 2f;
    public float selfDestructRange = 8f;
    public float selfDestructDamage = 80f;

    [Header("타이머")]
    public float attackTime;

    protected float m_attackTime;
    protected bool doSelfDestruct;
    protected float m_selfDestructTimer = 0.1f;


    protected virtual void Awake()
    {
        health = GetComponent<Health>();

        enemyMovement = GetComponent<EnemyMovement>();
        enemyMovement = enemyMovement == null ? GetComponent<EnemyMovementFly>() : enemyMovement;

        Initialization();
    }

    protected virtual void Initialization()
    {
        if (enemyMovement != null)
        {
            enemyMovement.detectRange = detectRange;
            enemyMovement.attackRange = attackRange;
            //enemyMovement.chaseRangePosition = chaseRangeCenter.position;
            //enemyMovement.attackRangePosition = attackRangeCenter.position;
        }
    }

    protected virtual void Update()
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

    protected virtual void Attack()
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
        if (enemyMovement == null)
        {
            enemyMovement = GetComponent<EnemyMovement>();

            Initialization();
        }
        else
        {
            Initialization();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (isSelfDestruct)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, selfDestructRange);
        }

        if (TryGetComponent<EnemyMovementFly>(out var e))
        {
            e.activityRange = activityRange;
            e.detectRange = detectRange;
            e.attackRange = attackRange;
        }
    }
#endif
}
