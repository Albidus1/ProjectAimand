using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



public struct HealthChangeEvent
{
    static HealthChangeEvent e;

    public Health affectedHealth;
    public float newHealth;

    public HealthChangeEvent(Health _affectedHealth, float _newHealth)
    {
        affectedHealth = _affectedHealth;
        newHealth = _newHealth;
    }

    public static void Trigger(Health _affectedHealth, float _newHealth)
    {
        e.affectedHealth = _affectedHealth;
        e.newHealth = _newHealth;
        EventManager.TriggerEvent(e);
    }
}

public struct HealthDeathEvent
{
    static HealthDeathEvent e;

    public Health affectedHealth;

    public HealthDeathEvent(Health _affectedHealth)
    {
        affectedHealth = _affectedHealth;
    }

    public static void Trigger(Health _affectedHealth)
    {
        e.affectedHealth = _affectedHealth;
        EventManager.TriggerEvent(e);
    }
}


public class Health : MonoBehaviour, IEventListener<HealthDeathEvent>
{
    //테스트
    public float maxHP = 100;
    public Slider healthSlider;

    public bool invincible = false;
    public float currentHP
    {
        get { return m_HP; }
        set
        {
            if (invincible) 
                return; // ✅ 무적일 땐 체력 변경 안 함


            m_HP = Mathf.Clamp(value, 0, maxHP);

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1;
                healthSlider.value = m_HP / maxHP;
            }

            OnDamageEvent.Invoke(value); // 대미지 이벤트 실행

            if (Application.isPlaying)
            {
                Debug.Log($"현재 체력: {m_HP}");
            }

            if (m_HP <= 0)
            {
                Kill();
            }
        }
    }

    private ReSpawner m_respawner;

    private float m_HP;
    private Collider2D m_collider;
    private PlayerMovement m_playerMovement;
    private EnemyMovementControl m_enemyMovement;
    private GameObject m_owner;

    public UnityEvent<float> OnDamageEvent;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_playerMovement = GetComponent<PlayerMovement>();

        if (m_playerMovement == null)
        {
            m_enemyMovement = GetComponent<EnemyMovement>();
            m_enemyMovement = m_enemyMovement == null ? GetComponent<EnemyMovementFly>() : m_enemyMovement;
        }

        m_owner = this.gameObject;

        m_respawner = FindFirstObjectByType<ReSpawner>();
    }

    private void Start()
    {
        m_collider.enabled = true;
        InitializeCurrentHealth();
    }

    public void InitializeCurrentHealth()
    {
        //Debug.Log($"현재 체력: {currentHP}");
        m_HP = maxHP;

        if(healthSlider != null)
        {
            healthSlider.value = currentHP / maxHP;
        }
    }

    public void Kill()
    {
        HealthDeathEvent.Trigger(this);

        if (m_playerMovement != null)
        {
            Debug.Log("플레이어 사망");
            LevelManager.Instance.PlayerDead(m_playerMovement);
            InitializeCurrentHealth();
        }
        else if (m_enemyMovement != null)
        {
            StartCoroutine(OnDeath());
        }       
    }

    private IEnumerator OnDeath()
    {
        m_collider.enabled = false;

        gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);
    }

    protected virtual void OnEnable()
    {
        InitializeCurrentHealth();
        this.EventStartListening<HealthDeathEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<HealthDeathEvent>();
    }

    public void OnEvent(HealthDeathEvent _deathEvent)
    {
        //Kill();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHP = m_HP;
    }
#endif
}
