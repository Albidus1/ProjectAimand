using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class WaveManager : MySingleton<WaveManager>
{
    [MyReadOnly]
    public string currentWaveID;
    [MyReadOnly]
    public EnemyWave currentWave;

    [Header("풀링")]
    public MyMultipleObjectPooler pool;


    public int enemiesRemaining => m_enemiesRemaining;

    public UnityEvent OnAllWavesCompleted;
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveCompleted;

    public bool isWaveActive { get; private set; } = false;
    private List<Wave> m_waves = new List<Wave>();
    private List<GameObject> m_enemies = new List<GameObject>();
    private int m_currentWaveIndex = -1;
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
            if (_list[i].enemyPrefab == null)
            {
                return;
            }

            GameObject nextGameObject = pool.GetPooledGameObjectOfName(_list[i].enemyPrefab.name);

            if (nextGameObject == null)
            {
                return;
            }
            //if (nextGameObject.GetComponent<MyPoolableObject>() == null)
            //{
            //    throw new Exception(_list[i].enemyPrefab.name + "PoolalbeObject 없음");
            //}

            nextGameObject.transform.position = _list[i].spawnPosition;

            nextGameObject.SetActive(true);

            m_enemies.Add(nextGameObject);
            m_enemiesRemaining++;
        }
    }

    public void SetWave(EnemyWave _wave)
    {
        if (isWaveActive)
        {
            return;
        }

        if (_wave == null)
        {
            Debug.LogError("Wave없음");
            return;
        }

        if (_wave.isWaveCompleted)
        {
            return;
        }

        currentWave = _wave;
        currentWaveID = _wave.waveEventID;
        m_waves = _wave.waves;

        ResetWaveIndex();
        StartWave();
    }

    private void StartWave()
    {
        m_currentWaveIndex++;

        if (m_currentWaveIndex >= m_waves.Count)
        {
            Debug.Log("모든 웨이브 완료");

            OnAllWavesCompleted?.Invoke();

            ResetWaveIndex();

            currentWave.isWaveCompleted = true;
            currentWave = null;
            currentWaveID = string.Empty;
            isWaveActive = false;

            return;
        }

        StartCoroutine(SpawnWave(m_waves[m_currentWaveIndex]));
    }

    private IEnumerator SpawnWave(Wave _wave)
    {
        isWaveActive = true;

        GetWavePool(_wave.enemyInfo);      
        yield return new WaitForSeconds(_wave.spawnInterval);

        if (_wave.waveEventType == WaveEventStartTypes.StartWave)
        {
            //Debug.Log($"시작 이벤트: {currentWaveID}");
            OnWaveStart?.Invoke(m_currentWaveIndex);
        }

        Debug.Log($"적 {m_enemiesRemaining}마리 생성");

        yield return new WaitUntil(() => WaveManager.Instance.enemiesRemaining <= 0);

        if (_wave.waveEventType == WaveEventStartTypes.EndWave)
        {
            //Debug.Log($"시작 이벤트: {currentWaveID}");
            OnWaveCompleted?.Invoke(m_currentWaveIndex);
        }

        yield return new WaitForSeconds(_wave.timeForNextWave);

        OnWaveCompleted?.Invoke(m_currentWaveIndex);
        StartWave();
    }

    public void AddEnemyToWave(GameObject _enemy, Vector3 _spawnPosition)
    {
        if (_enemy == null)
        {
            return;
        }

        GameObject nextGameObject = pool.GetPooledGameObjectOfName(_enemy.name);

        if (nextGameObject == null)
        {
            return;
        }
        //if (nextGameObject.GetComponent<MyPoolableObject>() == null)
        //{
        //    throw new Exception(enemyTransform.name + "PoolalbeObject 없음");
        //}

        nextGameObject.transform.position = _spawnPosition;

        nextGameObject.SetActive(true);

        m_enemies.Add(nextGameObject);
        m_enemiesRemaining++;
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

    private void ResetWaveIndex()
    {
        m_enemiesRemaining = 0;
        m_currentWaveIndex = -1;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        OnWaveStart.RemoveAllListeners();
        OnWaveCompleted.RemoveAllListeners();
        OnAllWavesCompleted.RemoveAllListeners();
    }
}
