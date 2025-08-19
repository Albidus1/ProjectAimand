using System;
using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    public LayerMask targetLayerMask = LayerManager.playerLayerMask;
    public float detectionRadius = 10f;
    public float trackingStartTime = 0;
    public float trackingTime = 3f;
    public float trackingEndTime = 1f;
    public bool lockedLocation;

    protected Health m_health;
    protected Projectile m_projectile;
    protected Vector3 m_targetPosition;
    protected Vector3 m_direction;
    protected Quaternion m_rotation;
    protected float m_trackingStartTimer;
    protected float m_trackingTimer;
    protected float m_trackingEndTimer;
    protected bool isTracking;
    protected bool isLocked;
    protected bool m_targetPointArrival;

    private void Awake()
    {
        m_projectile = GetComponent<Projectile>();
        m_health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        m_trackingStartTimer = trackingStartTime;
        m_trackingTimer = trackingTime;
        m_trackingEndTimer = trackingEndTime;
        m_targetPosition = Vector3.zero;
        isTracking = false;
        isLocked = false;
        m_targetPointArrival = false;
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

        if (m_targetPointArrival)
        {
            m_trackingEndTimer -= Time.deltaTime;

            if (m_trackingEndTimer < 0f)
            {
                m_health.Kill();
            }
        }
    }

    private void FindTarget()
    {      
        if (false == isLocked || m_targetPosition == null)
        {
            Collider2D target = Physics2D.OverlapCircle(transform.position, detectionRadius, targetLayerMask);

            if (target != null)
            {
                m_targetPosition = target.transform.position;
            }

            if (m_targetPosition != null)
            {
                m_direction = (m_targetPosition - transform.position).normalized;
                m_rotation = Quaternion.LookRotation(Vector3.forward, m_direction);
            }

            isLocked = lockedLocation;
        }

        if (Vector2.Distance(transform.position, m_targetPosition) < 0.1f)
        {
            m_projectile.SetDirection(Vector2.zero, m_rotation);
            m_targetPointArrival = true;
        }
        else
        {
            m_projectile.SetDirection(m_direction, m_rotation);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (m_targetPosition != null && m_trackingTimer > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_targetPosition);
        }
    }
#endif
}
