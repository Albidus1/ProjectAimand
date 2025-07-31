using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



[System.Serializable]
public class DropProbability
{
    public int dropCount;
    [Range(0f, 1f)] public float probability;
}

public class EnemyDrops : MonoBehaviour, IEventListener<HealthDeathEvent>
{
    [SerializeField]
    private List<DropProbability> normalDrops = new List<DropProbability>
    {
        new DropProbability { dropCount = 1, probability = 0.75f },
        new DropProbability { dropCount = 2, probability = 0.25f }
    };

    [SerializeField]
    private List<DropProbability> eliteDrops = new List<DropProbability>
    {
        new DropProbability { dropCount = 1, probability = 0.45f },
        new DropProbability { dropCount = 2, probability = 0.3f },
        new DropProbability { dropCount = 3, probability = 0.15f },
        new DropProbability { dropCount = 4, probability = 0.10f },
    };

    [SerializeField] private GameObject dropObjectPrefabs;


    public void DropItems(bool _isElite, Vector3 _dropPosition)
    {
        int dropCount = _isElite ? GetDropCount(eliteDrops) : GetDropCount(normalDrops);

        Debug.Log($"타입 {(_isElite ? "엘리트" : "노말")} + {dropCount}");


        for (int i = 0; i < dropCount; i++)
        {
            SpawnItem(_dropPosition);
        }
    }

    private int GetDropCount(List<DropProbability> dropTable)
    {
        float totalProbability = 0f;
        foreach (var drop in dropTable)
        {
            totalProbability += drop.probability;
        }

        float randomPoint = Random.value * totalProbability;

        float cumulative = 0f;
        foreach (var drop in dropTable)
        {
            cumulative += drop.probability;

            if (randomPoint <= cumulative)
            {
                return drop.dropCount;
            }
        }

        return dropTable[dropTable.Count - 1].dropCount;
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
        DropItems(false, transform.position);
    }
}
