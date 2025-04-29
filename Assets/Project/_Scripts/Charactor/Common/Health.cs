using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHP = 30;
    public float currentHP
    {
        get
        {
            return m_HP;
        }
        set
        {
            m_HP = value;

            Debug.Log($"현재 체력: {currentHP}");

            if (m_HP <= 0)
            {
                m_HP = 0;
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
