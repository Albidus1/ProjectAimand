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
    public WaveEventStartTypes waveEventType;
    public ObjectTriggerTypes waveEventTriggerType;
    public List<EnemySpawnInfo> enemyInfo;
    public float spawnInterval;
    public float timeForNextWave;
}


public class EnemyWave : MonoBehaviour
{
    public string waveID = "defaultWave";
    public List<Wave> waves;

    public bool isWaveCompleted = false;


    public void StartWave()
    {
        WaveManager.Instance.SetWave(this);
    }

    private void OnEnable()
    {
        isWaveCompleted = false;
    }
}
