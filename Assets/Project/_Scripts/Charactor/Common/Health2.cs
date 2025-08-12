using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



// 나중에 Health 대체
public class Health2 : MonoBehaviour, IEventListener<HealthDeathEvent>
{
    [Header("상태")]
    [MyReadOnly]
    public float currentHP;
    [MyReadOnly] [Tooltip("일시적 무적 상태")]
    public bool temporarilyInvulneralble = false;
    [MyReadOnly] [Tooltip("피격 후 무적 상태 여부")]
    public bool postDamageInvulnerable = false;

    [Header("체력 설정")]
    [Tooltip("초기 체력")]
    public float initialHP = 10f;
    [Tooltip("최대 체력")]
    public float maxHP = 10f;
    [Tooltip("일시적인 무적 상태 여부")]
    public bool invulnerable = false;

    [Header("피해")]
    [Tooltip("영구적인 무적 상태 여부")]
    public bool immuneToDamage = false;

    [Header("넉백")]
    [Tooltip("넉백 면역 여부")]
    public bool immuneToKnockback = false;

    [Header("사망")]
    public bool destroyOnDeath = true;
    public float delayBeforeDestroy = 0f;
    public bool gravityOffOnDeath = false;
    public bool respawnAtInitialPosition = false;

    [Header("사망 시 힘")]
    [Tooltip("사망 시 힘을 적용할지 여부")]
    public bool applyForceOnDeath = true;
    [Tooltip("사망 시 힘의 크기")]
    public Vector2 deathForce = new Vector2(0, 10);
    public bool resetForceOnDeath = false;

    public delegate void OnHitDelegate();
    public delegate void OnReviveDelegate();
    public delegate void OnDeathDelegate();

    public OnHitDelegate OnHit;
    public OnReviveDelegate OnRevive;
    public OnDeathDelegate OnDeath;

    public float lastDamage { get; set; }
    public Vector3 lastDamageDirection { get; set; }
    public bool initialized => m_initialized;
    public CharacterMovement characterController => m_characterMovement;

    private GameObject m_owner;
    private Collider2D m_collider;
    private CharacterMovement m_characterMovement;

    private Vector3 m_initialPosition;
    private bool m_initialized = false;
    private ReSpawner m_respawner;


    private void Start()
    {
        Initialization();
        InitializeCurrentHealth();
    }

    private void Initialization()
    {
        m_owner = this.gameObject;

        m_respawner = FindFirstObjectByType<ReSpawner>();
        m_collider = GetComponent<Collider2D>();
        m_characterMovement = GetComponent<PlayerMovement>();
        if (m_characterMovement == null)
        {
            m_characterMovement = GetComponent<EnemyMovement>();
            m_characterMovement = m_characterMovement == null ? GetComponent<EnemyMovementFly>() : m_characterMovement;
        }

        SetInitialPosition();
        m_initialized = true;

        DamageEnabled();
        DisablePostDamageInvulnerability();
        UpdateHPBar();
    }

    public void InitializeCurrentHealth()
    {
        SetHealth(initialHP, m_owner);
    }

    public void SetInitialPosition()
    {
        m_initialPosition = transform.position;
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

    public void Damage(float _damage, GameObject _instigator, float _invincibilityDuration, Vector3 _damageDirection)
    {
        if (false == gameObject.activeInHierarchy)
        {
            return;
        }

        if (temporarilyInvulneralble || invulnerable
            || postDamageInvulnerable || immuneToDamage)
        {
            Debug.Log("데미지를 받지 않음.");
            return;
        }

        if (false == CanTakeDamage())
        {
            return;
        }

        if (_damage <= 0)
        {
            return;
        }

        float previousHP = currentHP;
        SetHealth(currentHP - _damage, _instigator);

        lastDamage = _damage;
        lastDamageDirection = _damageDirection;
        OnHit?.Invoke();

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        if (_invincibilityDuration > 0 && gameObject.activeInHierarchy)
        {
            EnablePostDamageInvulnerability();
            StartCoroutine(DisablePostDamageInvulnerability(_invincibilityDuration));
        }

        UpdateHPBar();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Kill();
        }
    }

    public void Kill()
    {
        if (immuneToDamage)
        {
            return;
        }
        
        if (m_characterMovement != null)
        {
            if (m_characterMovement is PlayerMovement playerMovement)
            {
                Debug.Log("플레이어 사망");
                LevelManager.Instance.PlayerDead(playerMovement);
            }

            if (m_characterMovement is EnemyMovementControl enemyMovement)
            {
                Debug.Log("적 사망");
                //enemyMovement.RegisterEnemyDeath(this.gameObject);
            }

        }
        
        SetHealth(0f, m_owner);

        DamageDisabled();

        OnDeath?.Invoke();

        //HealthDeathEvent.Trigger(this);

        if (m_characterMovement != null)
        {
            if (m_collider != null)
            {
                m_collider.enabled = false;
            }

            if (gravityOffOnDeath)
            {
                
            }

            if (resetForceOnDeath)
            {

            }

            if (applyForceOnDeath)
            {
                
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

        if (respawnAtInitialPosition)
        {
            transform.position = m_initialPosition;
        }

        Initialization();
        InitializeCurrentHealth();

        OnRevive?.Invoke();
    }

    private void DestroyObject()
    {
        if (false == destroyOnDeath)
        {
            return;
        }

        if (m_respawner != null)
        {
            //m_respawner.Respawn(m_owner, m_initialPosition);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void GetHealth(float _health, GameObject _instigator)
    {
        SetHealth(Mathf.Min(currentHP + _health, maxHP), _instigator);
        UpdateHPBar();
    }

    public void SetHealth(float _newHealth, GameObject _instigator)
    {
        currentHP = Mathf.Min(_newHealth, maxHP);
        UpdateHPBar();
        //HealthChangeEvent.Trigger(this, _newHealth);
    }

    public void UpdateHPBar()
    {
        
    }

    public void DamageEnabled()
    {
        temporarilyInvulneralble = false;
    }

    public void DamageDisabled()
    {
        temporarilyInvulneralble = true;
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

    public IEnumerator DamageEnabled(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        temporarilyInvulneralble = false;
    }

    protected virtual void OnEnable()
    {
        InitializeCurrentHealth();
        DamageEnabled();
        DisablePostDamageInvulnerability();
        UpdateHPBar();
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
