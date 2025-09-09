using UnityEngine;

public class BossSniper : BossSkillBase
{
    [Header("타이머")]
    public float trackingTime = 3f;
    public float shotTime = 1f;


    private Vector2 m_velocity = Vector2.zero;
    private float m_trackingTimer;
    private float m_shotTimer;
    private float m_destroyTime;



    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Initialization();
    }

    public void Initialization()
    {
        m_collider2D.enabled = false;

        target = FindFirstObjectByType<PlayerMovement>().transform;
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

        m_collider2D.enabled = true;
        Invoke(nameof(OnDestroyObject), 0.1f);
    }

    private void OnDestroyObject()
    {
        this.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

    }
#endif
}
