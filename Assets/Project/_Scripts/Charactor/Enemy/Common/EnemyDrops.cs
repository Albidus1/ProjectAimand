using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class DropProbability
{
    public int dropCount;
    [Range(0f, 1f)] public float probability;
}

public class EnemyDrops : MonoBehaviour, IEventListener<HealthDeathEvent>
{
    [Header("확률")]
    [SerializeField]
    private List<DropProbability> drops = new List<DropProbability>
    {
        new DropProbability { dropCount = 1, probability = 0.75f },
        new DropProbability { dropCount = 2, probability = 0.25f }
    };

    //[SerializeField]
    //private List<DropProbability> eliteDrops = new List<DropProbability>
    //{
    //    new DropProbability { dropCount = 1, probability = 0.45f },
    //    new DropProbability { dropCount = 2, probability = 0.3f },
    //    new DropProbability { dropCount = 3, probability = 0.15f },
    //    new DropProbability { dropCount = 4, probability = 0.10f },
    //};

    [Header ("드롭 아이템")]
    [SerializeField] private GameObject dropObjectPrefabs;


    private Health m_health;



    private void Awake()
    {
        m_health = GetComponent<Health>();
    }

    public void DropItems(Vector3 _dropPosition)
    {
        int dropCount = GetDropCount(drops);

        Debug.Log($"{gameObject.name} : 아이템 {dropCount}개 생성");

        for (int i = 0; i < dropCount; i++)
        {
            SpawnItem(_dropPosition);
        }
    }

    private int GetDropCount(List<DropProbability> _dropTable)
    {
        float totalProbability = 0f;
        foreach (var drop in _dropTable)
        {
            totalProbability += drop.probability;
        }

        float randomPoint = Random.value * totalProbability;

        float cumulative = 0f;
        foreach (var drop in _dropTable)
        {
            cumulative += drop.probability;

            if (randomPoint <= cumulative)
            {
                return drop.dropCount;
            }
        }

        return _dropTable[^1].dropCount;
    }

    private void SpawnItem(Vector3 position)
    {
        if (dropObjectPrefabs != null)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0,
                Random.Range(-0.5f, 0.5f)
            );

            Instantiate(dropObjectPrefabs, position + randomOffset, Quaternion.identity);
        }
    }

    public void Drop()
    {
        Debug.Log("사망 감지");
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<HealthDeathEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<HealthDeathEvent>();
    }

    public void OnEvent(HealthDeathEvent _deathEvent)
    {
        if (m_health != _deathEvent.affectedHealth)
        {
            return;
        }

        DropItems(transform.position);
    }
}
