using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyMovement enemyMovement;

    [Header("범위")]
    public Vector2 chaseRange = new Vector2(12, 12);
    public Transform chaseRangeCenter;
    public Vector2 attackRange = new Vector2(6, 6);
    public Transform attackRangeCenter;

    [Header("타이머")]
    public float attackTime;

    private float m_attackTime;



    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();

        Initialize();
    }

    private void Initialize()
    {
        enemyMovement.chaseRange = chaseRange;
        enemyMovement.attackRange = attackRange;
        //enemyMovement.chaseRangePosition = chaseRangeCenter.position;
        //enemyMovement.attackRangePosition = attackRangeCenter.position;
    }

    private void Update()
    {
        m_attackTime -= Time.deltaTime;

        if (m_attackTime < 0 && enemyMovement.isAttacking)
        {
            Attack();
        }
    }

    private void Attack()
    {
        m_attackTime = attackTime;

        Debug.Log("적 공격");
    }

    private void OnValidate()
    {
        attackRange.x = Mathf.Clamp(attackRange.x, 0, chaseRange.x - 0.5f);
        attackRange.y = Mathf.Clamp(attackRange.y, 0, chaseRange.y - 0.5f);

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
}
