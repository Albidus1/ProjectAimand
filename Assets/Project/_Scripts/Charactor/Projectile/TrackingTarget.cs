using System;
using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    public LayerMask targetLayerMask = LayerManager.playerLayerMask;
    public float detectionRadius = 10f;
    public float trackingStartTime = 0;
    public float trackingTime = 3f;
    public bool lockedLocation;

    protected Projectile m_projectile;
    protected Transform m_target;
    protected Vector3 m_direction;
    protected Quaternion m_rotation;
    protected float m_trackingStartTimer;
    protected float m_trackingTimer;
    protected bool isTracking;
    protected bool isLocked = false;

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
        m_trackingStartTimer = trackingStartTime;
        m_trackingTimer = trackingTime;
        m_target = null;
        isTracking = false;
        isLocked = false;
    }

    private void Update()
    {
        m_trackingStartTimer -= Time.deltaTime;

        if (false == isTracking && m_trackingStartTimer < 0f)
        {
            isTracking = true;
        }

        if (isTracking && m_trackingTimer > 0f)
        {
            m_trackingTimer -= Time.deltaTime;
            FindTarget();
        }
    }

    private void FindTarget()
    {     
        if (false == isLocked)
        {
            Collider2D target = Physics2D.OverlapCircle(transform.position, detectionRadius, targetLayerMask);

            if (target != null)
            {
                m_target = target.transform;
            }

            if (m_target != null)
            {
                m_direction = (m_target.transform.position - transform.position).normalized;
                m_rotation = Quaternion.LookRotation(Vector3.forward, m_direction);
            }

            isLocked = lockedLocation;
        }

        m_projectile.SetDirection(m_direction, m_rotation);
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
