using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public Vector2 spawnPosition;
}

[System.Serializable]
public class Wave
{
    public string WaveID;
    public List<EnemySpawnInfo> enemyInfo;
    public float spawnInterval;
    public float timeForNextWave;
}


public class EnemyWave : MonoBehaviour
{
    public Wave[] waves;

    public bool isWaveActive { get; private set; } = false;

    private int m_currentWave = -1;
    private int m_enemiesSpawned;



    private void Awake()
    {

    }

    private void Start()
    {
        //StartNextWave();
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

        for (int i = 0; i < _wave.enemyInfo.Count; i++)
        {
            _wave.enemyInfo[i].spawnPosition = GetSpawnPosition();
            m_enemiesSpawned++;
        }

        WaveManager.Instance.GetWavePool(_wave.enemyInfo);

        yield return new WaitForSeconds(_wave.spawnInterval);

        Debug.Log($"적 {m_enemiesSpawned}마리 생성");

        yield return new WaitUntil(() => WaveManager.Instance.enemiesRemaining <= 0);

        yield return new WaitForSeconds(_wave.timeForNextWave);

        isWaveActive = false;

        StartNextWave();
    }

    private Vector3 GetSpawnPosition()
    {
        float x = Random.Range(-10f, 10f);

        return new Vector3(transform.position.x + x, transform.position.y, 0);
    }

    private void AllWaveCompleted()
    {
        Debug.Log("웨이브 클리어");
    }
}
