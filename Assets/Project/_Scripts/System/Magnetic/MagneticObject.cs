using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagneticObject : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;

    public enum MagneticFlag
    {
        NONE = 0,
        PULL = 1 << 0,
        PUSH = 1 << 1,
        HOLD = 1 << 2,
        ALL = PULL | PUSH | HOLD
    }

    [SerializeField]
    public MagneticFlag MagneticType;

    public bool IsPullable => (MagneticType & MagneticFlag.PULL) > 0;
    public bool IsPushable => (MagneticType & MagneticFlag.PUSH) > 0;
    public bool IsHoldable => (MagneticType & MagneticFlag.HOLD) > 0;

    private void Start()
    {
        m_rigidbody = this.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (m_isFired)
        {
            if (Time.time - m_fireStartTime >= m_fireLimitTime)
            {
                m_isFired = false;
                m_rigidbody.gravityScale = 1f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_isFired)
        {
            m_isFired = false;
            m_rigidbody.gravityScale = 1f;
        }
    }

    private bool m_isFired = false;
    private float m_fireStartTime;
    private float m_fireLimitTime;

    public void FireMagneticObject(float fireTimeLimit = 5f)
    {
        m_isFired = true;
        m_fireStartTime = Time.time;
        m_fireLimitTime = fireTimeLimit;
    }
}
