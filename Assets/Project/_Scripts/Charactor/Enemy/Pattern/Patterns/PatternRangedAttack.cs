using System;
using UnityEngine;


public class PatternRangedAttack : EnemyPatternBase
{
    protected MyObjectPooler m_objectPooler;
    protected Transform m_projecttileSpawnTransform;
    protected Vector3 m_projecttileSpawnOffset = Vector3.zero;
    protected int m_projecttilePerShot = 1;
    protected Vector3 m_spawnPosition = Vector3.zero;
    protected Vector2 m_direction = Vector2.right;
    protected Vector2 m_randomSpreadDirection;

    private Vector3 m_spawnPositionCenter;
    private Vector2 m_offset;

    protected float m_lastAttackTime;
    protected float m_shootInterval;

    public override void Execute()
    {
        m_lastAttackTime = Time.time;

        if (m_objectPooler == null)
        {
            m_objectPooler = enemyTransform.GetComponent<MyObjectPooler>();

            if (m_objectPooler == null)
            {
                Debug.LogError("MyObjectPooler 없음 > " + enemyTransform.name);
                return;
            }
        }

        m_direction = patternData.spawnFaceDirection.normalized;
        m_projecttilePerShot = patternData.projectilePerShot;
        m_shootInterval = patternData.attackInterval;
    }

    public override void Update()
    {
        float desiredDistace = patternData.attackRange * 0.8f;
        Vector2 retreatDirection = (enemyTransform.position - target.position).normalized;
        Vector2 desiredPosition = (Vector2)target.position + retreatDirection * desiredDistace;

        base.MoveTowards(desiredPosition, patternData.moveSpeed * 0.7f);

        if (Time.time - m_lastAttackTime > m_shootInterval)
        {
            m_lastAttackTime = Time.time;

            DetermineSpawnPosition();

            for (int i = 0; i < m_projecttilePerShot; i++)
            {
                SpawnProjectile(m_spawnPosition);
            }
        }
    }

    public GameObject SpawnProjectile(Vector3 _spawnPosition)
    {
        GameObject nextGameObject = m_objectPooler.GetPooledGameObject();

        if (nextGameObject == null)
        {
            return null;
        }
        if (nextGameObject.GetComponent<MyPoolableObject>() == null)
        {
            throw new Exception(enemyTransform.name + "PoolalbeObject 없음");
        }

        nextGameObject.transform.position = _spawnPosition;

        nextGameObject.SetActive(true);

        Projectile projectile = nextGameObject.GetComponent<Projectile>();

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, m_direction);
        projectile.SetDirection(m_direction, rotation, true);

        if (nextGameObject.TryGetComponent<TrackingTarget>(out TrackingTarget trackingTarget))
        {
            trackingTarget.detectionRadius = patternData.attackRange;
        }

        return nextGameObject;
    }

    public void DetermineSpawnPosition()
    {
        m_spawnPositionCenter = m_projecttileSpawnTransform == null ?
            enemyTransform.position : m_projecttileSpawnTransform.position;

        m_offset = m_projecttileSpawnOffset;

        m_spawnPosition = m_spawnPositionCenter + enemyTransform.rotation * m_offset;
    }

    public override bool isFinished()
    {
        return controller.patternExecutionTimer <= 0;
    }
}
