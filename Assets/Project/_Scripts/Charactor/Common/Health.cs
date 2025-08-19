using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



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

    [MyReadOnly]
    public float currentHP;

    public EnemyWave enemyWave { get; set; }

    private ReSpawner m_respawner;

    private Collider2D m_collider;
    private PlayerMovement m_playerMovement;
    private EnemyMovementControl m_enemyMovement;
    private GameObject m_owner;

    public UnityEvent<float> OnDamageEvent;



    private void Start()
    {
        Initialization();
        InitializeCurrentHealth();
    }

    private void Initialization()
    {
        if (m_collider == null)
        {
            m_collider = GetComponent<Collider2D>();
        }

        m_collider.enabled = true;

        m_playerMovement = GetComponent<PlayerMovement>();
        if (m_playerMovement == null)
        {
            m_enemyMovement = GetComponent<EnemyMovement>();
            m_enemyMovement = m_enemyMovement == null ? GetComponent<EnemyMovementFly>() : m_enemyMovement;
        }

        m_owner = this.gameObject;

        m_respawner = FindFirstObjectByType<ReSpawner>();

        UpdateHealthBar();
    }

    public void InitializeCurrentHealth()
    {
        //Debug.Log($"현재 체력: {currentHP}");
        currentHP = maxHP;

        UpdateHealthBar();
    }

    public void Damage(float _damage, float _invincibilityDuration)
    {
        if (invincible)
            return; // ✅ 무적일 땐 체력 변경 안 함


        _damage = Mathf.Clamp(_damage, 0, maxHP);
        currentHP -= _damage;

        if (healthSlider != null)
        {
            healthSlider.maxValue = 1;
            healthSlider.value = currentHP / maxHP;
        }

        OnDamageEvent.Invoke(_damage); // 대미지 이벤트 실행

        if (Application.isPlaying)
        {
            Debug.Log($"현재 체력: {currentHP}");
        }

        if (_invincibilityDuration > 0 && gameObject.activeInHierarchy)
        {
            invincible = true;
            StartCoroutine(DisableInvincible(_invincibilityDuration));
        }

        UpdateHealthBar();

        if (currentHP <= 0)
        {
            Kill();
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
            return;
        }
        else if (m_enemyMovement != null)
        {
            WaveManager.Instance.RegisterEnemyDeath(this.gameObject);

            OnDeath();
            return;
        }

        OnDeath();
    }

    public void UpdateHealthBar()
    {
        if (m_playerMovement != null)
        {
            if (GUIManager.HasInstance)
            {
                Debug.Log($"플레이어 체력 업데이트: {currentHP}");
                GUIManager.Instance.UpdateHealthBar(currentHP, 0f, maxHP);
            }
        }
    }

    private IEnumerator DisableInvincible(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        invincible = false;
    }

    private void OnDeath()
    {
        if (m_collider != null)
        {
            m_collider.enabled = false;
        }

        this.gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        Initialization();
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
        //currentHP = m_HP;
    }
#endif
}
