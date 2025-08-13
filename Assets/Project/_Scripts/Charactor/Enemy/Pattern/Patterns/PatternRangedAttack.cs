using System;
using UnityEngine;


public class PatternRangedAttack : EnemyPatternBase
{
    private MyObjectPooler[] m_objectPooler;
    private Transform m_projecttileSpawnTransform;
    private Vector3 m_projecttileSpawnOffset = Vector3.zero;
    private Vector3 m_spawnPosition = Vector2.zero;
    private Vector2 m_direction = Vector2.right;
    private Vector2 m_randomSpreadDirection;
    private Vector3 m_spawnPositionCenter;
    private Vector2 m_offset;
    private float m_lastAttackTime;
    private float m_shootInterval;
    private int m_projecttilePerShot = 1;
    private int m_selectedIndex;

    public override void Execute()
    {
        m_lastAttackTime = Time.time;

        if (m_objectPooler == null)
        {
            m_objectPooler = enemyTransform.GetComponentsInChildren<MyObjectPooler>();

            if (m_objectPooler == null)
            {
                Debug.LogError("MyObjectPooler 없음 > " + enemyTransform.name);
                return;
            }
        }

        m_direction = patternData.spawnFaceDirection.normalized;
        m_projecttilePerShot = patternData.projectilePerShot;
        m_shootInterval = patternData.attackInterval;
        m_lastAttackTime = Time.time;

        m_selectedIndex = -1;
        for (int i = 0; i < m_objectPooler.Length; i++)
        {
            if (m_objectPooler[i].gameObject.name == patternData.poolName)
            {
                m_selectedIndex = i;
                //Debug.Log(patternData.poolName + " >> " + i);
                break;
            }
        }
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

            if (m_selectedIndex >= 0)
            {
                DetermineSpawnPosition();

                for (int i = 0; i < m_projecttilePerShot; i++)
                {
                    SpawnProjectile(m_spawnPosition, m_selectedIndex, i, m_projecttilePerShot);
                }
            }
        }
    }

    public GameObject SpawnProjectile(Vector3 _spawnPosition, int _poolIndex,int projectileIndex, int totalProjectiles)
    {
        GameObject nextGameObject = m_objectPooler[_poolIndex].GetPooledGameObject();

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

        if (totalProjectiles > 1)
        {
            Vector2 spread = new Vector2(10, 10);

            m_randomSpreadDirection.x = MyMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -spread.x, spread.x);
            m_randomSpreadDirection.y = MyMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -spread.y, spread.y);
        }
        else
        {
            m_randomSpreadDirection = Vector2.up;
        }

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, m_randomSpreadDirection);
        projectile.SetDirection(m_randomSpreadDirection.normalized, rotation);

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
