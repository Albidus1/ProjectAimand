using UnityEngine;

public class BossSkillBase : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target;
    public LayerMask playerMask;

    [Header("기본 스킬 설정")]
    public float disableTime;

    protected Collider2D m_collider2D;



    protected virtual void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
        playerMask = LayerMask.GetMask("Player");
    }

    protected virtual void OnEnable()
    {

    }

    public virtual void UseSkill()
    {

    }
}
