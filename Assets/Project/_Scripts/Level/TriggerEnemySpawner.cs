using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class SpawnerElement
{
    public int enemyIndex;
    public Vector3 spawnPosition;
}

public class TriggerEnemySpawner : TriggerEvent
{
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<SpawnerElement> enemyList = new List<SpawnerElement>();


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (base.m_triggered || base.m_triggerOnce)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Trigger();
        }
    }

    protected override void Trigger()
    {
        base.m_triggerOnce = base.triggerOnce;
        base.m_triggered = true;


        if (enemyPrefabs.Count < 1)
        {
            Debug.LogWarning("적 프리팹 확인");
            return;
        }

        foreach (var enemy in enemyList)
        {
            Vector2 spawnPosition = transform.position + enemy.spawnPosition;

            Instantiate
                (enemyPrefabs[enemy.enemyIndex],
                spawnPosition, 
                Quaternion.identity);
        }
    }
}
