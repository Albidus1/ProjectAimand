using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class Wave
{
    public string WaveID;
    public GameObject[] enemyTypes;
    public int enemyCount;
    public float spawnInterval;
    public float timeForNextWave;
}


public class EnemyWave : MonoBehaviour
{
    public Wave[] waves;
    public UnityEvent OnAllWavesCompleted;
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveCompleted;


    public bool isWaveActive { get; private set; }
    private int m_currentWave = -1;
    private int m_enemiesSpawned;
    private int m_enemiesRemaining;
    private List<GameObject> m_enemies = new List<GameObject>();


    private void Awake()
    {
        
    }

    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (isWaveActive)
        {
            return;
        }

        m_currentWave++;

        if (m_currentWave >= waves.Length)
        {
            AllWaveCompleted();
            return;
        }

        StartCoroutine(SpawnWave(waves[m_currentWave]));    
    }

    private IEnumerator SpawnWave(Wave _wave)
    {
        isWaveActive = true;
        m_enemiesSpawned = 0;
        m_enemiesRemaining = _wave.enemyCount;

        OnWaveStart?.Invoke(m_currentWave + 1);

        for (int i = 0; i < _wave.enemyCount; i++)
        {
            GameObject enemy = _wave.enemyTypes[Random.Range(0, _wave.enemyTypes.Length)];
            SpawnEnemy(enemy);

            m_enemiesSpawned++;
            yield return new WaitForSeconds(_wave.spawnInterval);
        }

        Debug.Log($"적 {m_enemiesSpawned}마리 생성");

        yield return new WaitUntil(() => m_enemiesRemaining <= 0);
        
        OnWaveCompleted?.Invoke(m_currentWave + 1);

        yield return new WaitForSeconds(_wave.timeForNextWave);

        isWaveActive = false;

        StartNextWave();
    }

    private void SpawnEnemy(GameObject _enemy)
    {
        Vector2 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(_enemy, spawnPosition, Quaternion.identity);
        Health h = enemy.GetComponent<Health>();
        h.enemyWave = this;

        m_enemies.Add(enemy);
    }

    private Vector3 GetSpawnPosition()
    {
        float x = Random.Range(-10f, 10f);

        return new Vector3(transform.position.x + x, transform.position.y, 0);
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

    private void AllWaveCompleted()
    {
        Debug.Log("웨이브 클리어");
        OnAllWavesCompleted?.Invoke();
    }
}
