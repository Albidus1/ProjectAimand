using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyMovementControl enemyMovement;

    public Health health;

    [Header("공격 설정")]
    public float damage = 10f;
    public float invulnerabilityTime = 0.5f;
    public Collider2D attackCollider2D;

    [Header("범위")]
    public float activityRange = 13f;
    public float detectRange = 12f;
    public float attackRange = 6f;

    [Header("자폭")]
    public bool isSelfDestruct = false;
    public float selfDestructWaitTime = 2f;
    public float selfDestructRange = 8f;
    public float selfDestructDamage = 80f;
    public GameObject selfDestructEffect;

    [Header("타이머")]
    public float attackTime;

    protected float m_attackTime;
    protected bool doSelfDestruct;
    protected float m_selfDestructTimer = 0.1f;
    protected bool isAttacking = false;
    protected bool isAttackEnabled = true;


    protected virtual void Awake()
    {
        health = GetComponent<Health>();

        enemyMovement = GetComponent<EnemyMovement>();
        enemyMovement = enemyMovement == null ? GetComponent<EnemyMovementFly>() : enemyMovement;

        if (attackCollider2D == null)
        {
            //attackCollider2D = transform.Find("AttackCollider2D").GetComponent<Collider2D>();
        }

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

        if (attackCollider2D != null)
        {
            attackCollider2D.enabled = false;
        }

        isAttackEnabled = true;
        isAttacking = false;
        m_attackTime = 0f;
        doSelfDestruct = false;
        m_selfDestructTimer = 0f;
    }

    protected virtual void Update()
    {
        if (false == isAttackEnabled)
        {
            return;
        }

        if (SelfDestruct())
        {
            isAttackEnabled = false;
            return;
        }

        if (m_attackTime < Time.time && enemyMovement.isAttacking && false == isAttacking)
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

        if (enemyMovement != null)
        {
            enemyMovement.isStunned = true;
        }

        Debug.Log("자폭");

        ShowExplosion();

        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, selfDestructRange);

        foreach (Collider2D obj in col)
        {
            Health health = obj.GetComponent<Health>();

            if (health != null)
            {
                health.Damage(selfDestructDamage, invulnerabilityTime);
                Debug.Log($"{obj.name}에게 {selfDestructDamage}의 피해");
            }
        }

        health.Kill();

        return true;
    }

    private void ShowExplosion()
    {
        GameObject explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = transform.position;
        explosionSphere.transform.localScale = Vector3.one * selfDestructRange * 2;
        explosionSphere.GetComponent<Renderer>().material.color = Color.red;
        explosionSphere.GetComponent<Collider>().enabled = false;

        Destroy(explosionSphere, 0.1f);
    }

    protected virtual void Attack()
    {
        m_attackTime = Time.time + attackTime;

        if (false == isSelfDestruct)
        {
            //Debug.Log("적 공격");
            StartCoroutine(StartAttack());
        }
        else if (false == doSelfDestruct)
        {
            //Debug.Log("자폭 시작");

            doSelfDestruct = true;
            m_selfDestructTimer = Time.time + selfDestructWaitTime;

            enemyMovement.isStunned = true;
        }
    }
    
    public void AttackColliderEnable()
    {
        attackCollider2D.enabled = true;
    }

    public void AttackColliderDisable()
    {
        attackCollider2D.enabled = false;
    }

    private IEnumerator StartAttack()
    {
        isAttacking = true;
        enemyMovement.isAttackingPlayer = true;

        yield return new WaitForSeconds(1f);

        var target = enemyMovement.target;

        if (target == null)
        {
            AttackEnd();
            yield break;
        }

        enemyMovement.animator.SetTrigger("isAttacking");
        yield return new WaitUntil(() => enemyMovement.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        yield return new WaitForSeconds(0.5f);

        /*Vector3 position = transform.localScale.x > 0 ?
            new Vector3(transform.position.x + attackRange * 0.3f, transform.position.y, 0) :
            new Vector3(transform.position.x - attackRange * 0.3f, transform.position.y, 0);

        Vector2 direction = target.transform.position - transform.transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (transform.localScale.x > 0)
        {
            angle += 180f;
        }
        angle = Mathf.Clamp(angle, -89, 89);
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        GameObject attackBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        attackBox.transform.position = position;
        attackBox.transform.rotation = targetRotation;
        attackBox.transform.localScale = new Vector2(3, attackRange);
        attackBox.GetComponent<Renderer>().material.color = Color.red;
        attackBox.GetComponent<Collider>().enabled = false;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackBox.transform.position,      
            attackBox.transform.localScale,    
            attackBox.transform.eulerAngles.z, 
            enemyMovement.playerLayerMask
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) 
                continue;


            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<Health>(out var playerHealth))
                {
                    playerHealth.Damage(damage, invulnerabilityTime);
                    Debug.Log($"{hit.name}에게 {damage}의 피해");
                }
            }
        }

        Destroy(attackBox, 0.1f);*/

        AttackEnd();
    }

    private void AttackEnd()
    {
        isAttacking = false;
        enemyMovement.isAttackingPlayer = false;
    }

    private void OnEnable()
    {
        Initialization();
    }

    private void OnValidate()
    {
        //if (enemyMovement == null)
        //{
        //    enemyMovement = GetComponent<EnemyMovement>();

        //    Initialization();
        //}
        //else
        //{
        //    Initialization();
        //}
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
