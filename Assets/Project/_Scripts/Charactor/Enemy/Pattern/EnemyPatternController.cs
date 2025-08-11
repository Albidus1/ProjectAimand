using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;




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
    protected Dictionary<string, EnemyPattern> m_patternDictionary = new Dictionary<string, EnemyPattern>();
    protected EnemyPattern m_currentPattern;
    protected int m_currentPatternIndex = 0;
    protected IEnemyPattern m_currentPatternInstance;
    protected Animator m_animator;
    protected EnemyMovement m_enemyMovement;
    protected PatternCooldownTracker m_patternCooldownTracker = new PatternCooldownTracker();

    protected float m_globalTime = 0.5f;
    protected float m_globalCooldownTimer = 0f;


    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start()
    {
        m_patternDictionary = new Dictionary<string, EnemyPattern>();
        m_patternCooldownTracker = new PatternCooldownTracker();
        foreach (var pattern in availablePatterns)
        {
            if (false == m_patternDictionary.ContainsKey(pattern.patternID))
            {
                m_patternDictionary.Add(pattern.patternID, pattern);
                m_patternCooldownTracker.InitializeCooldowns(pattern);
            }
        }

        ResetPatternTimers();
    }

    private void Update()
    {
        UpdatePatternTimers();

        if (IsAnyPatternReady())
        {
            m_enemyMovement.enabled = true;

            SelectNextPattern();
        }

        if (isPatternActive)
        {
            ExecutePattern();

            if (m_currentPatternInstance != null && 
                (m_currentPatternInstance.isFinished() || patternExecutionTimer <= 0))
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
        m_globalCooldownTimer = 0f;
        isPatternActive = false;
    }

    private void UpdatePatternTimers()
    {
        m_patternCooldownTracker.UpdateCooldowns(Time.deltaTime);

        if (false == isPatternActive && m_globalCooldownTimer > 0)
        {
            m_globalCooldownTimer -= Time.deltaTime;
        }

        if (patternExecutionTimer > 0)
        {
            patternExecutionTimer -= Time.deltaTime;
        }
    }
    #endregion

    private bool IsAnyPatternReady()
    {
        if (m_globalCooldownTimer > 0)
        {
            return false;
        }

        if (isPatternActive)
        {
            return false;
        }

        foreach (var pattern in availablePatterns)
        {
            if (m_patternCooldownTracker.IsCooldownReady(pattern.patternID))
            {
                Debug.Log($"패턴 {pattern.patternID} 준비 완료");
                return true;
            }
        }

        return false;
    }

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
            bool cooldownCondition = m_patternCooldownTracker.IsCooldownReady(pattern.patternID);

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

            //Debug.Log($"패턴 실행 시간: {patternExecutionTimer}");

            m_patternCooldownTracker.StartCooldown(m_currentPattern.patternID, m_currentPattern.cooldown);
            m_globalCooldownTimer = m_globalTime;

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


        Vector3 position = transform.position + (Vector3.up * 3.5f + Vector3.right * 3f);

        foreach (var pattern in availablePatterns)
        {
            float remaining = m_patternCooldownTracker.GetRemainingCooldown(pattern.patternID);
            string status = remaining > 0 ? $"{remaining:F1}s" : "READY";

            UnityEditor.Handles.Label(
                position,
                $"{pattern.patternID}: {status}",
                new GUIStyle { normal = new GUIStyleState { textColor = new Color(1, 0, 0, 0.7f) } }
            );

            position += Vector3.down * 0.8f;
        }
    }
#endif
}
