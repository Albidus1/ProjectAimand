using System;
using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    public LayerMask targetLayerMask = LayerManager.playerLayerMask;
    public float detectionRadius = 10f;
    public float trackingTime = 3f;

    protected Projectile m_projectile;
    protected Transform m_target;
    protected float m_trackingTimer;


    private void Awake()
    {
        m_projectile = GetComponent<Projectile>();

        if (m_projectile == null)
        {
            Debug.LogError("Projectile 컴포넌트 없음");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        m_trackingTimer = trackingTime;
        m_target = null;
    }

    private void Update()
    {
        m_trackingTimer -= Time.deltaTime;

        if (m_trackingTimer > 0f)
        {
            FindTarget();
        }
    }

    private void FindTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayerMask);

        if (targets.Length > 0)
        {
            m_target = targets[0].transform;
            float closestDistance = Vector2.Distance(transform.position, m_target.position);

            foreach (var target in targets)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    m_target = target.transform;
                }
            }

            Vector2 direction = (m_target.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            m_projectile.SetDirection(direction, rotation);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (m_target != null && m_trackingTimer > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_target.position);
        }
    }
#endif
}
