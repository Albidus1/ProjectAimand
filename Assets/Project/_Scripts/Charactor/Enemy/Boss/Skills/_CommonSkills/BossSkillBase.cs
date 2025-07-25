using UnityEngine;

public class BossSkillBase : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target;
    public LayerMask playerMask;

    [Header("기본 스킬 설정")]
    public float damage;
    public bool isDoT = false;
    [MyConditionalHide("isDoT", true)]
    public float damageTickTime = 0.5f;
    public float disableTime;

    protected Collider2D m_col;
    protected float m_damageTimer = -0.1f;



    protected virtual void Awake()
    {
        m_col = GetComponent<Collider2D>();
        playerMask = LayerMask.GetMask("Player");
    }

    protected virtual void OnEnable()
    {

    }

    public virtual void UseSkill()
    {

    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDoT)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} - {collision.gameObject.name}에게 {damage} 피해");
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (!isDoT)
        {
            return;
        }

        if (Time.time < m_damageTimer)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            m_damageTimer = Time.time + damageTickTime;
            Debug.Log($"{gameObject.name} - {collision.gameObject.name}에게 {damage} 피해 (DoT)");
        }
    }
}
