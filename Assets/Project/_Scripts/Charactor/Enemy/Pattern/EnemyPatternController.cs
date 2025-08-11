using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPatternController : MonoBehaviour
{
    public List<EnemyPattern> availablePatterns = new List<EnemyPattern>();
    public bool isSequentialPattern = true;

    [MyReadOnly]
    public Transform target;
    [MyReadOnly]
    public string currentPatternID;
    [MyReadOnly]
    public float patternCooldownTimer;
    [MyReadOnly]
    public float patternExecutionTimer;

    public bool isPatternActive { get; private set; }
    protected Dictionary<PatternType, EnemyPattern> m_patternDictionary;
    protected EnemyPattern m_currentPattern;
    protected int m_currentPatternIndex = 0;
    protected IEnemyPattern m_currentPatternInstance;
    protected Animator m_animator;
    protected EnemyMovement m_enemyMovement;
    


    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start()
    {
        m_patternDictionary = new Dictionary<PatternType, EnemyPattern>();
        foreach (var pattern in availablePatterns)
        {
            if (false == m_patternDictionary.ContainsKey(pattern.patternType))
            {
                m_patternDictionary.Add(pattern.patternType, pattern);
            }
        }

        ResetPatternTimers();
    }

    private void Update()
    {
        UpdatePatternTimers();

        if (false == isPatternActive && patternCooldownTimer <= 0f)
        {
            m_enemyMovement.enabled = true;

            SelectNextPattern();
        }

        if (isPatternActive)
        {
            ExecutePattern();

            if (m_currentPatternInstance != null && m_currentPatternInstance.isFinished())
            {
                EndPatternExecution();
            }
        }     
    }

    #region TIMERS
    private void ResetPatternTimers()
    {
        patternCooldownTimer = 0f;
        patternExecutionTimer = 0f;
        isPatternActive = false;
    }

    private void UpdatePatternTimers()
    {
        if (patternCooldownTimer > 0f)
        {
            patternCooldownTimer -= Time.deltaTime;
        }

        if (isPatternActive)
        {
            patternExecutionTimer -= Time.deltaTime;
        }
    }
    #endregion

    private void SelectNextPattern()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        List<EnemyPattern> patternsToCheck = new List<EnemyPattern>(availablePatterns);
        List<EnemyPattern> candidatePatterns = new List<EnemyPattern>();
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        foreach (var pattern in patternsToCheck)
        {
            bool distanceCondition = distanceToTarget >= pattern.minTriggerDistance && distanceToTarget <= pattern.maxTriggerDistance;
            bool cooldownCondition = patternCooldownTimer <= 0f;

            if (distanceCondition && cooldownCondition)
            {
                candidatePatterns.Add(pattern);
            }
        }

        if (candidatePatterns.Count > 0)
        {
            candidatePatterns.Sort((a, b) => b.priority.CompareTo(a.priority));

            m_currentPatternIndex = Mathf.Min(3, candidatePatterns.Count);
            m_currentPattern = candidatePatterns[Random.Range(0, m_currentPatternIndex)];

            Debug.Log($"선택된 패턴: {m_currentPattern.patternID}");

            StartPatternExecution();
        }
    }

    private void StartPatternExecution()
    {
        m_enemyMovement.enabled = false;
        int direction = (int)Mathf.Sign(target.position.x - transform.position.x);
        m_enemyMovement.CheckDirectionToFace(direction > 0);

        m_currentPatternInstance = PatternFactory.CreatePattern(m_currentPattern.patternType, m_currentPattern);

        if (m_currentPatternInstance != null)
        {
            m_currentPatternInstance.Initialization(this, m_currentPattern);
            isPatternActive = true;
            patternExecutionTimer = m_currentPattern.executionTime;
            currentPatternID = m_currentPattern.patternID;

            Debug.Log($"패턴 실행 시간: {patternExecutionTimer}");

            m_currentPatternInstance.Execute();

            if (false == string.IsNullOrEmpty(m_currentPattern.animationTrigger))
            {
                //Debug.Log("애니메이션 트리거 설정: " + m_currentPattern.animationTrigger);
                m_animator.SetBool(m_currentPattern.animationTrigger, true);
            }
        }
    }

    private void ExecutePattern()
    {
        if (m_currentPatternInstance != null)
        {
            m_currentPatternInstance.Update();
        }
    }

    private void EndPatternExecution()
    {
        if (m_currentPatternInstance != null)
        {
            m_currentPatternInstance.Finish();
        }

        if (false == string.IsNullOrEmpty(m_currentPattern.animationTrigger))
        {
            m_animator.SetBool(m_currentPattern.animationTrigger, false);
        }

        isPatternActive = false;
        patternCooldownTimer = m_currentPattern.cooldown;
        m_currentPattern = null;
        m_currentPatternInstance = null;
        currentPatternID = "None";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_currentPattern != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_currentPattern.maxTriggerDistance);
            Gizmos.DrawWireSphere(transform.position, m_currentPattern.minTriggerDistance);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}
