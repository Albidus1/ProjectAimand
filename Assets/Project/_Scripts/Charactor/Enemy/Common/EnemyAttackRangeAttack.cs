using DG.Tweening;
using System;
using UnityEngine;

public class EnemyAttackRangeAttack : EnemyAttack
{
    [Header("원거리 공격")]
    public GameObject rangedWeapon;
    public GameObject bullet;
    public Transform bulletSpawn;
    public float bulletDuration;


    private float bulletAngle;
    private bool isWeaponReady = true;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        UpdateRangedWeapon();

        if (m_attackTime < Time.time && enemyMovement.isAttacking)
        {
            Attack();
        }
    }

    private void UpdateRangedWeapon()
    {
        if (enemyMovement.target == null)
        {
            ResetWeaponRotation();
            return;
        }

        var target = base.enemyMovement.target;

        Vector2 direction = rangedWeapon.transform.position - target.transform.position;
        bulletAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (transform.localScale.x > 0)
        {
            bulletAngle += 180f;
        }

        bulletAngle = Mathf.Clamp(bulletAngle, -89, 89);
        Quaternion targetRotation = Quaternion.Euler(0, 0, bulletAngle);

        rangedWeapon.transform.rotation = Quaternion.Lerp(
            rangedWeapon.transform.rotation,
            targetRotation,
            10f * Time.deltaTime
        );

        float angleDifference = Quaternion.Angle(rangedWeapon.transform.rotation, targetRotation);
    }

    private void ResetWeaponRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);

        rangedWeapon.transform.rotation = Quaternion.Lerp(
            rangedWeapon.transform.rotation,
            targetRotation,
            10f * Time.deltaTime
        );

        isWeaponReady = false;
    }

    protected override void Attack()
    {
        base.m_attackTime = Time.time + base.attackTime;
        Debug.Log("확인용");
        
        if (bullet != null && bulletSpawn != null)
        {
            Quaternion angle = Quaternion.Euler(0, 0, bulletAngle);
            GameObject b = Instantiate(bullet, bulletSpawn.position, angle);

            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            Vector3 moveDirection = angle * direction;
            Vector3 targetPosition = b.transform.position + moveDirection * 50f;

            b.transform.DOMove(targetPosition, bulletDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    if (b != null) 
                    {
                        Destroy(b);
                    }
                })
                .SetLink(b);
        }
    }
}
