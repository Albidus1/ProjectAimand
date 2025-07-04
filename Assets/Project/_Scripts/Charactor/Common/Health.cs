using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float maxHP = 100;
    public Slider healthSlider;

    public bool invincible = false;
    public float currentHP
    {
        get { return m_HP; }
        set
        {
            if (invincible) return; // ✅ 무적일 땐 체력 변경 안 함

            m_HP = Mathf.Clamp(value, 0, maxHP);

            if (healthSlider != null)
                healthSlider.value = m_HP / maxHP;

            OnDamageEvent.Invoke(value); // 대미지 이벤트 실행

            Debug.Log($"현재 체력: {m_HP}");

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
    private EnemyMovement m_enemyMovement;
    private GameObject m_owner;

    public UnityEvent<float> OnDamageEvent;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_playerMovement = GetComponent<PlayerMovement>();
        m_enemyMovement = GetComponent<EnemyMovement>();
        m_owner = this.gameObject;

        m_respawner = FindFirstObjectByType<ReSpawner>();
    }

    private void Start()
    {
        currentHP = maxHP;

        InitializeCurrentHealth();
    }

    private void OnEnable()
    {
        InitializeCurrentHealth();
    }

    public void InitializeCurrentHealth()
    {
        Debug.Log($"현재 체력: {currentHP}");
        m_HP = maxHP;

        if(healthSlider != null)
        {
            healthSlider.value = currentHP / maxHP;
        }
    }

    private void Kill()
    {
        if (m_playerMovement != null)
        {
            m_playerMovement.movementState.StateChange(PlayerStates.MovementStates.Die);
            StartCoroutine(nameof(OnRespawn));
        }
        else if (m_enemyMovement != null)
        {
            StartCoroutine(nameof(OnDeath));
        }       
    }

    private IEnumerator OnRespawn()
    {
        m_collider.enabled = false;

        yield return new WaitForSeconds(3f);

        InitializeCurrentHealth();
        transform.position = m_respawner.respawnPosition.position;
        m_collider.enabled = true;

        if (m_playerMovement != null)
        {
            m_playerMovement.movementState.StateChange(PlayerStates.MovementStates.Idle);
        }
    }

    private IEnumerator OnDeath()
    {
        m_collider.enabled = false;

        Destroy(this.gameObject);

        yield return new WaitForSeconds(3f);

    }
}
