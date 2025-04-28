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

    public ReSpawner respawner;

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

        respawner = FindFirstObjectByType<ReSpawner>();
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
        Debug.Log("사망");
        StartCoroutine(nameof(OnRespawn));
    }

    private IEnumerator OnRespawn()
    {
        m_collider.enabled = false;

        yield return new WaitForSeconds(3f);

        InitializeCurrentHealth();
        transform.position = respawner.respawnPosition.position;
        m_collider.enabled = true;
    }
}
