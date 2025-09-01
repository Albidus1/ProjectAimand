using System.Collections;
using UnityEngine;
using UnityEngine.Events;




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
    [MyReadOnly]
    public float currentHP;
    [MyReadOnly]
    public bool temporarilyInvulnerable = false;
    [MyReadOnly]
    public bool postDamageInvulnerable = false;

    [Header("체력 설정")]
    public float initialHP = 100f;
    public float maxHP = 100f;
    public bool invulnerable = false;

    [Header("데미지")]
    public bool immuneToDamage = false;

    [Header("넉백")]
    public bool immuneToKnockback = false;

    [Header("사망")]
    public bool destroyOnDeath = true;
    public float delayBeforeDestroy = 0f;
    public bool CollisionsOffOnDeath = true;
    public bool grabityOffOnDeath = false;
    public bool respawnAtInitialLocation = false;

    //[Header("사망 시 힘")]
    //public bool applyDeathForce = true;
    //public Vector2 deathForce = new Vector2(0f, 10f);
    //public bool resetForcesOnDeath = false;

    public UnityEvent<float> OnDamageEvent;

    public float lastDamage { get; set; }
    public Vector2 lastDamageDirection { get; set; }
    public bool initialized => m_initialized;

    private bool m_initialized = false;
    private Vector3 m_initialPosition;

    private Collider2D m_collider;
    private PlayerMovement m_playerMovement;
    private EnemyMovementControl m_enemyMovement;
    private HealthBar m_healthBar;
    private AutoRespawn m_autoRespawn;



    private void Start()
    {
        Initialization();
        InitializeCurrentHealth();
    }

    private void Initialization()
    {
        m_collider = GetComponent<Collider2D>();
        m_playerMovement = GetComponent<PlayerMovement>();
        m_enemyMovement = GetComponent<EnemyMovementControl>();
        m_healthBar = GetComponentInChildren<HealthBar>();
        m_autoRespawn = GetComponent<AutoRespawn>();

        m_initialPosition = transform.position;
        DamageEnabled();
        DisablePostDamageInvulnerability();
        UpdateHealthBar();

        m_initialized = true;
    }

    public void InitializeCurrentHealth()
    {
        SetHealth(initialHP, this.gameObject);
    }

    public bool CanTakeDamage()
    {
        if (invulnerable || immuneToDamage)
        {
            return false;
        }

        if (false == this.enabled)
        {
            return false;
        }

        if (currentHP <= 0 && initialHP != 0)
        {
            return false;
        }

        return true;
    }

    public void Damage(float _damage, float _invincibilityDuration)
    {
        Damage(_damage, this.gameObject, _invincibilityDuration, Vector2.zero);
    }

    public void Damage(float _damage, GameObject _instigator, float _invincibilityDuration, Vector2 _damageDirection)
    {
        if (false == this.gameObject.activeInHierarchy)
        {
            return;
        }

        if (temporarilyInvulnerable || invulnerable || immuneToDamage || postDamageInvulnerable)
        {
            return;
        }

        if (false == CanTakeDamage())
        {
            return;
        }

        _damage = Mathf.Clamp(_damage, 0, maxHP);
        
        if (_damage <= 0)
        {
            return;
        }

        OnDamageEvent?.Invoke(_damage);

        SetHealth(currentHP - _damage, _instigator);

        lastDamage = _damage;
        lastDamageDirection = _damageDirection;

        currentHP = Mathf.Max(currentHP, currentHP, 0f);

        if (_invincibilityDuration > 0 && this.gameObject.activeInHierarchy)
        {
            EnablePostDamageInvulnerability();
            StartCoroutine(DisablePostDamageInvulnerability(_invincibilityDuration));
        }

        UpdateHealthBar();

        if (currentHP <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        if (immuneToDamage)
        {
            return;
        }

        if (m_playerMovement != null)
        {
            Debug.Log("플레이어 사망");
            LevelManager.Instance.PlayerDead(m_playerMovement);
        }
        else if (m_enemyMovement != null)
        {
            WaveManager.Instance.RegisterEnemyDeath(this.gameObject);
        }

        SetHealth(0f, this.gameObject);

        DamageDisabled();
        HealthDeathEvent.Trigger(this);

        if (m_playerMovement != null)
        {
            if (m_collider != null)
            {
                m_collider.enabled = false;
            }
        }

        if (delayBeforeDestroy > 0f)
        {
            Invoke(nameof(DestroyObject), delayBeforeDestroy);
        }
        else
        {
            DestroyObject();
        }
    }

    public void Revive()
    {
        if (false == m_initialized)
        {
            return;
        }

        if (m_collider != null)
        {
            m_collider.enabled = true;
        }

        if (m_playerMovement != null)
        {
            m_playerMovement.movementState.StateChange(PlayerStates.MovementStates.Idle);
        }

        if (respawnAtInitialLocation)
        {
            transform.position = m_initialPosition;
        }

        Initialization();
        InitializeCurrentHealth();

        UpdateHealthBar();
    }

    private void DestroyObject()
    {
        if (false == destroyOnDeath)
        {
            return;
        }

        if (m_autoRespawn == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            m_autoRespawn.Kill();
        }
    }

    public void ApplyKnockback(GameObject _instigator, Vector2 _dir)
    {
        if (immuneToKnockback)
        {
            return;
        }

        Vector2 relativePosition = transform.position - _instigator.transform.position;
        Vector2 knockbackForce = new Vector2(_dir.x, _dir.y);
        knockbackForce.x *= Mathf.Sign(relativePosition.x);

        if (m_playerMovement != null)
        {
            m_playerMovement.ApplyKnockback(knockbackForce);
        }
        else if (m_enemyMovement != null)
        {
            m_enemyMovement.ApplyKnockback(knockbackForce);
        }
    }

    public void GetHealth(float _health, GameObject _instigator)
    {
        SetHealth(Mathf.Min(currentHP + _health, maxHP), _instigator);
        UpdateHealthBar();
    }

    public void SetHealth(float _newHealth, GameObject _instigator)
    {
        currentHP = Mathf.Min(_newHealth, maxHP);
        UpdateHealthBar();
    }

    public void ResetHealthToMaxHealth()
    {
        currentHP = maxHP;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (m_healthBar != null)
        {
            //m_healthBar.UpdateBar(currentHP, 0f, maxHP, _show);
        }

        if (m_playerMovement != null)
        {
            if (GUIManager.HasInstance)
            {
                Debug.Log($"플레이어 체력 업데이트: {currentHP}");
                GUIManager.Instance.UpdateHealthBar(currentHP, 0f, maxHP);
            }
        }
    }

    public void DamageEnabled()
    {
        temporarilyInvulnerable = false;
    }

    public void DamageDisabled()
    {
        temporarilyInvulnerable = true;
    }

    public void EnablePostDamageInvulnerability()
    {
        postDamageInvulnerable = true;
    }

    public void DisablePostDamageInvulnerability()
    {
        postDamageInvulnerable = false;
    }

    public IEnumerator DisablePostDamageInvulnerability(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        postDamageInvulnerable = false;
    }

    protected virtual void OnEnable()
    {
        InitializeCurrentHealth();
        DamageEnabled();
        DisablePostDamageInvulnerability();
        UpdateHealthBar();
        this.EventStartListening<HealthDeathEvent>();
    }

    protected virtual void OnDisable()
    {
        CancelInvoke();
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
