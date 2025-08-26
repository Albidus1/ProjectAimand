using DG.Tweening;
using System;
using UnityEngine;

public class EnemyAttackRangeAttack : EnemyAttack
{
    [MyReadOnly]
    public GameObject target;

    [Header("원거리 공격")]
    public MySimpleObjectPooler objectPooler;
    public GameObject rangedWeapon;
    public Transform bulletSpawn;
    public float bulletDuration;


    private float bulletAngle;
    private Vector2 m_direction;
    private LineRenderer m_lineRenderer;



    protected override void Awake()
    {
        objectPooler = GetComponent<MySimpleObjectPooler>();
        m_lineRenderer = GetComponentInChildren<LineRenderer>();

        base.Awake();
    }

    protected void Start()
    {
        m_lineRenderer.startColor = Color.red;
        m_lineRenderer.endColor = Color.white;
        m_lineRenderer.startWidth = 0.05f;
    }

    protected override void Update()
    {
        target = base.enemyMovement.target;

        if (target == null)
        {
            m_lineRenderer.enabled = false;
        }
        else
        {
            m_lineRenderer.enabled = true;
        }

        UpdateRangedWeapon();

        if (target != null)
        {
            if (m_attackTime < Time.time && false == isAttacking)
            {
                //Attack();
                base.isAttacking = true;

                Debug.Log("확인용");
                enemyMovement.animator.SetTrigger("isAttacking");
            }

            if (m_lineRenderer != null)
            {
                m_lineRenderer.SetPosition(0, bulletSpawn.transform.position);
                m_lineRenderer.SetPosition(1, target.transform.position);
            }
        }
    }

    private void UpdateRangedWeapon()
    {
        if (target != null)
        {
            m_direction = target.transform.position - rangedWeapon.transform.position;
            bulletAngle = Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg;

            if (transform.localScale.x < 0f)
            {
                bulletAngle += 180f;
            }

            Quaternion targetRotation = Quaternion.Euler(0, 0, bulletAngle);

            rangedWeapon.transform.rotation = Quaternion.Lerp(
                rangedWeapon.transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

            rangedWeapon.transform.rotation = Quaternion.Lerp(
                rangedWeapon.transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }

    protected override void Attack()
    {
        base.m_attackTime = Time.time + base.attackTime;
        GameObject nextGameObject = objectPooler.GetPooledGameObject();

        if (nextGameObject == null)
        {
            return;
        }
        if (nextGameObject.GetComponent<MyPoolableObject>() == null)
        {
            throw new Exception(gameObject.name + "PoolalbeObject 없음");
        }

        nextGameObject.transform.position = bulletSpawn.transform.position;

        nextGameObject.SetActive(true);

        Projectile projectile = nextGameObject.GetComponent<Projectile>();

        projectile.SetDirection(m_direction.normalized, transform.rotation);
    }

    public void AttackAnimationEnd()
    {
        base.isAttacking = false;
    }
}
