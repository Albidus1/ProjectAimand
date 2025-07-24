using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public enum DamageSource
{
    UNKNOWN,
    PLATFORM,
    MONSTER,
    BULLET
}

public class Health : MonoBehaviour
{
    [SerializeField]
    private float maxHP = 100;
    public float MaxHP { get { return maxHP; } }
    private float m_HP;
    public float CurrentHP { get { return m_HP; } }
    [SerializeField]
    private Slider healthSlider;

    private bool invincible = false;
    public bool IsInvincible {  get { return invincible; } }

    private Collider2D m_collider;
    private GameObject m_owner;

    public UnityEvent<float, DamageSource> OnDamageEvent;
    public UnityEvent OnDeathEvent;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_owner = this.gameObject;
    }

    private void Start()
    {
        m_HP = maxHP;

        InitializeCurrentHealth();
    }

    private void OnEnable()
    {
        InitializeCurrentHealth();
    }

    public void InitializeCurrentHealth()
    {
        Debug.Log($"현재 체력: {CurrentHP}");
        m_HP = maxHP;

        if(healthSlider != null)
        {
            healthSlider.value = CurrentHP / maxHP;
        }
    }

    public int Damaged(float damage, DamageSource source = DamageSource.UNKNOWN)
    {
        if (damage < 0) return -1; // 데미지는 음수가 될 수 없음
        if (m_HP <= 0) return -2; // 이미 죽은 상태에서 데미지를 받을 수 없음 (사망 이벤트 중복 발생 방지)
        if (invincible) return 1; // 무적일 때

        m_HP -= damage;
        m_HP = Mathf.Max(m_HP, 0);

        // 체력바 표시
        if (healthSlider != null)
            healthSlider.value = m_HP / maxHP;

        OnDamageEvent.Invoke(damage, source); // 데미지 이벤트 실행

        if (m_HP <= 0)
        {
            OnDeathEvent.Invoke();
            return 2; // 죽었을 때
        }

        return 0; // 데미지를 받았지만 살아남았을 때
    }

    public void SetInvincible(bool invincible)
    {
        this.invincible = invincible;
    }

    [ContextMenu("KillTest")]
    private void KillTest()
    {
        Damaged(1000f);
    }
}
