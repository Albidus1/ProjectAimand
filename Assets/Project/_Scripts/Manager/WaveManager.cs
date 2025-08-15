using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class WaveManager : MySingleton<WaveManager>
{
    [Header("풀링")]
    public MyMultipleObjectPooler pool;


    public int enemiesRemaining => m_enemiesRemaining;

    public UnityEvent OnAllWavesCompleted;
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveCompleted;

    private List<GameObject> m_enemies = new List<GameObject>();
    //private int m_currentWave = -1;
    private int m_enemiesRemaining;



    protected override void Awake()
    {
        base.Awake();

        pool = GetComponent<MyMultipleObjectPooler>();
    }

    public void GetWavePool(List<EnemySpawnInfo> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            GameObject nextGameObject = pool.GetPooledGameObjectOfName(_list[i].enemyPrefab.name);

            if (nextGameObject == null)
            {
                return;
            }
            //if (nextGameObject.GetComponent<MyPoolableObject>() == null)
            //{
            //    throw new Exception(enemyTransform.name + "PoolalbeObject 없음");
            //}

            nextGameObject.transform.position = _list[i].spawnPosition;

            nextGameObject.SetActive(true);

            m_enemies.Add(nextGameObject);
            m_enemiesRemaining++;
        }
    }

    public void RegisterEnemyDeath(GameObject _enemy)
    {
        if (m_enemies.Contains(_enemy))
        {
            m_enemies.Remove(_enemy);
            m_enemiesRemaining--;

            Debug.Log("몹 제거");
        }
    }
}
