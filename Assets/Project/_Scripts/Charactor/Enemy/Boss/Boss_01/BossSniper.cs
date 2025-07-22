using UnityEngine;

public class BossSniper : MonoBehaviour
{
    [Header("타겟 - 플레이어")]
    public Transform target;
    public LayerMask playerMask;

    [Header("타이머")]
    public float trackingTime = 3f;
    public float shotTime = 1f;

    [Header("범위")]
    public float circleRadius = 1.5f;

    private Vector2 m_velocity = Vector2.zero;
    private float m_trackingTimer;
    private float m_shotTimer;
    private float m_destroyTime;



    private void Awake()
    {

    }

    private void OnEnable()
    {
        Initialization();
    }

    public void Initialization()
    {
        target = null;
        m_trackingTimer = trackingTime;
        m_shotTimer = shotTime;
        m_destroyTime = 0.1f;
    }

    private void LateUpdate()
    {
        TargetTracking();
    }

    private void TargetTracking()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<PlayerMovement>().transform;
        }

        if (m_trackingTimer > 0)
        {
            m_trackingTimer -= Time.deltaTime;

            transform.position = Vector2.SmoothDamp(
                transform.position, 
                target.position,
                ref m_velocity,
                0.3f);

            return;
        }

        if (m_shotTimer > 0)
        {
            m_shotTimer -= Time.deltaTime;
            return;
        }

        if (m_destroyTime > 0)
        {
            m_destroyTime -= Time.deltaTime;
            return;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll
            (transform.position,
            circleRadius,
            Vector2.zero,
            0,
            playerMask);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("맞음");
                break;
            }
        }

        Object.Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, circleRadius);
    }
#endif
}
