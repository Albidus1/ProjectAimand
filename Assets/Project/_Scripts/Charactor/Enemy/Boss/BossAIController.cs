using UnityEngine;
using System.Collections.Generic;
using TMPro;



public class BossAIController : MonoBehaviour
{
    [Header("보스 능력 설정")]
    public bool randomAbilityUsage = false;
    public List<BossAbility> abilities = new List<BossAbility>();

    [Header("보스 페이즈 설정")]
    public float[] phaseThresholds = { 0.75f, 0.5f, 0.25f };

    [Header("디버그용 텍스트")]
    public TextMeshProUGUI debugText;


    private Health m_health;
    private float m_hpRatio;
    private int m_currentPhase = 0;

    private BossAbility m_currentAbility = null;
    private bool m_isAbilityActive = false;
    private float m_waitTime = 1f;
    private float m_waitTimer;



    private void Awake()
    {
        m_health = GetComponent<Health>();     
    }

    private void Start()
    {
        Initialization();

        m_waitTimer = m_waitTime;
        m_isAbilityActive = true;
    }

    private void Initialization()
    {
        m_health.currentHP = m_health.maxHP;
        m_hpRatio = m_health.currentHP / m_health.maxHP;
        m_currentPhase = 0;


    }

    private void Update()
    {
        if (m_health.currentHP <= 0)
        {
            return;
        }

        if (m_currentAbility != null)
        {
            debugText.text = $"Skill : {m_currentAbility.name}  ";
        }

        if (m_currentAbility != null && false == m_currentAbility.isAbilityActive)
        {
            m_waitTimer = m_waitTime;
            m_currentAbility = null;
        }

        if (m_waitTimer > 0)
        {
            m_waitTimer -= Time.deltaTime;

            if (m_waitTimer <= 0)
            {
                m_isAbilityActive = false;
            }
        }

        UseAbility();
        UpdateCooldownTimers();
        UpdateHealth();
    }

    private void UseAbility()
    {
        if (abilities.Count == 0)
        {
            Debug.LogWarning("보스 능력 없음");
            return;
        }

        if (m_isAbilityActive)
        {
            return;
        }

        foreach (BossAbility ab in abilities)
        {
            if (false == ab.isOnCooldown && false == ab.isAbilityActive)
            {
                Debug.Log($"보스 능력 사용: {ab.name}");

                StartCoroutine(ab.UseAbility());

                m_currentAbility = ab;
                m_isAbilityActive = true;
                break;
            }
        }
    }

    private void UpdateCooldownTimers()
    {
        foreach (BossAbility ab in abilities)
        {
            if (ab.isOnCooldown)
            {
                ab.cooldownTimer -= Time.deltaTime;

                if (ab.cooldownTimer <= 0)
                {
                    ab.Initialization();
                }
            }
        }
    }

    private void UpdateHealth()
    {
        m_hpRatio = m_health.currentHP / m_health.maxHP;
        PhaseChange();
    }

    private void PhaseChange()
    {
        if (m_hpRatio < phaseThresholds[m_currentPhase])
        {
            m_currentPhase++;

            if (m_currentPhase >= phaseThresholds.Length)
            {
                m_currentPhase = phaseThresholds.Length - 1;
            }
        }
    }
}