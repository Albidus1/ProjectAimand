using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


[System.Serializable]
public class SpawnerPhase
{
    public int spawnCount;
    public float disalbeTime;
}


public class BossAbilitySpawner : BossAbility
{
    [Header("랜덤 스폰")]
    public bool randomSpawnPoint = false;

    [Header("스폰 위치")]
    public List<Vector2> spawnPoint = new List<Vector2>();
    public List<SpawnerPhase> spawnerPhases = new List<SpawnerPhase>();
    [MyReadOnly]
    public int currentPhaseIndex = 0;

    protected bool[] m_spawnedPoint;
    protected List<BossSkillBase> m_spawnedObjects = new List<BossSkillBase>();
    protected int m_spawnedObjectCount = 1;
    protected Collider2D m_collider2D;



    private void Awake()
    {
        m_collider2D = GetComponent<BoxCollider2D>();
        m_spawnedPoint = new bool[spawnPoint.Count];
    }

    protected override void Start()
    {
        base.Start();

        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();

        currentPhaseIndex = 0;

        m_spawnedObjectCount = base.abilityActCount;
        if (spawnerPhases.Count > 0)
        {
            m_spawnedObjectCount = spawnerPhases[0].spawnCount;

        }
    }

    public override void SkillReset()
    {
        base.SkillReset();
    }

    public override IEnumerator UseAbility()
    {
        if (base.abilityPrefab == null)
        {
            Debug.LogWarning("능력 오브젝트 없음");
            yield break;
        }

        base.SetAbilityActive(true);
        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);

        base.SetAbilityActive(false);
        ObjectsActivate();

        yield return new WaitForSeconds(0.1f);
    }

    private Vector2 SetSpawnPoint()
    {
        if (spawnPoint.Count == 0)
        {
            float posX = m_collider2D.bounds.center.x;
            float posY = m_collider2D.bounds.center.y;
            if (randomSpawnPoint)
            {
                posX = Random.Range(m_collider2D.bounds.min.x, m_collider2D.bounds.max.x);
                posY = Random.Range(m_collider2D.bounds.min.y, m_collider2D.bounds.max.y);
            }

            return new Vector2(posX, posY);
        }

        int index;
        while (true)
        {
            index = Random.Range(0, spawnPoint.Count);

            if (m_spawnedPoint[index] == false)
            {
                m_spawnedPoint[index] = true;
                break;
            }
        }

        return spawnPoint[index];
    }

    protected override void AbilityRangeVisualizer()
    {
        if (m_spawnedObjectCount <= 1)
        {
            m_spawnedObjectCount = 1;
        }

        m_spawnedObjects.Clear();
        m_spawnedPoint = new bool[spawnPoint.Count];

        for (int i = 0; i < m_spawnedObjectCount; i++)
        {
            m_spawnPoint = SetSpawnPoint();

            if (base.abilityRangePrefab != null)
            {
                base.AbilityRangeVisualizer();
            }

            BossSkillBase obj = Instantiate(base.abilityPrefab, m_spawnPoint, Quaternion.identity).GetComponent<BossSkillBase>();
            obj.gameObject.SetActive(false);
            m_spawnedObjects.Add(obj);
        }
    }

    private void ObjectsActivate()
    {
        foreach (BossSkillBase obj in m_spawnedObjects)
        {
            obj.gameObject.SetActive(true);

            if (spawnerPhases.Count > 0)
            {
                obj.disableTime = spawnerPhases[currentPhaseIndex].disalbeTime;
            }

            obj.UseSkill();
        }
    }

    public override void PhaseChange()
    {
        if (currentPhaseIndex >= spawnerPhases.Count - 1)
        {
            return;
        }

        Debug.Log($"스폰 능력 페이즈 변경: {currentPhaseIndex} -> {currentPhaseIndex + 1}");

        currentPhaseIndex++;
        m_spawnedObjectCount = spawnerPhases[currentPhaseIndex].spawnCount;
    }
}
