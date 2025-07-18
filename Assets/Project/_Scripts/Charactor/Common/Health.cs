using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float maxHP = 100;
    public Slider healthSlider;
    public float currentHP
    {
        get
        {
            return m_HP;
        }
        set
        {
            m_HP = Mathf.Clamp(value, 0, maxHP);

            if (healthSlider != null)
            {
                healthSlider.maxValue = 1;
                healthSlider.value = m_HP / maxHP;
            }

            Debug.Log($"현재 체력: {currentHP}");

            if (m_HP <= 0)
            {
                m_HP = 0;
                Kill();
            }
        }
    }


    private ReSpawner m_respawner;

    public float m_HP;
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
        //Debug.Log($"현재 체력: {currentHP}");
        m_HP = maxHP;

        if(healthSlider != null)
        {
            healthSlider.value = currentHP / maxHP;
        }
    }

    public void Kill()
    {
        if (m_playerMovement != null)
        {
            LevelManager.Instance.PlayerDead(m_playerMovement);
            InitializeCurrentHealth();
        }
        else if (m_enemyMovement != null)
        {
            StartCoroutine(nameof(OnDeath));
        }       
    }

    private IEnumerator OnDeath()
    {
        m_collider.enabled = false;

        Destroy(this.gameObject);

        yield return new WaitForSeconds(3f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentHP = m_HP;
    }
#endif
}
