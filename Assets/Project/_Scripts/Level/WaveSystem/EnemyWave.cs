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
    public string waveEventID;
    public WaveEventStartTypes waveEventType;
    public ObjectTriggerTypes waveEventTriggerType;
    public List<EnemySpawnInfo> enemyInfo;
    public float spawnInterval;
    public float timeForNextWave;
}


public class EnemyWave : MonoBehaviour
{
    public List<Wave> waves;

    public string waveEventID = "defaultWave";
    public bool waveStartEvent;
    [MyConditionalHide(nameof(waveStartEvent), true)]
    public string startWaveEventID = "defaultStartWave";

    public bool waveEndEvent;
    [MyConditionalHide(nameof(waveEndEvent), true)]
    public string endWaveEventID = "defaultEndWave";

    [MyReadOnly]
    public bool isWaveCompleted = false;


    public void StartWave()
    {
        if (false == string.IsNullOrEmpty(waveEventID))
        {
            WaveEvent.TriggerEvent(this, waveEventID);
        }

        if (waveStartEvent && false == string.IsNullOrEmpty(startWaveEventID))
        {
            WaveEvent.TriggerEvent(this, startWaveEventID);
        }

        WaveManager.Instance.SetWave(this);
    }

    protected virtual void OnEnable()
    {
        isWaveCompleted = false;

        WaveManager.Instance.OnAllWavesCompleted.AddListener(AllWavesCompleted);
        WaveManager.Instance.OnWaveStart.AddListener(WaveStart);
        WaveManager.Instance.OnWaveCompleted.AddListener(WaveCompleted);
    }

    private void AllWavesCompleted()
    {
        if (false == string.IsNullOrEmpty(waveEventID))
        {
            WaveEvent.TriggerEvent(this, waveEventID);
        }

        if (waveEndEvent && false == string.IsNullOrEmpty(endWaveEventID))
        {
            WaveEvent.TriggerEvent(this, endWaveEventID);
        }
    }

    private void WaveStart(int _index)
    {
        if (_index < 0 || waves.Count - 1 < _index)
        {
            return;
        }

        if (false == string.IsNullOrEmpty(waves[_index].waveEventID) &&
            waves[_index].waveEventType == WaveEventStartTypes.StartWave)
        {
            //Debug.Log("웨이브 시작 이벤트");
            WaveEvent.TriggerEvent(this, waves[_index].waveEventID);
        }
    }

    private void WaveCompleted(int _index)
    {
        if (_index < 0 || waves.Count - 1 < _index)
        {
            return;
        }

        if (false == string.IsNullOrEmpty(waves[_index].waveEventID) &&
            waves[_index].waveEventType == WaveEventStartTypes.EndWave)
        {
            //Debug.Log("웨이브 종료 이벤트");
            WaveEvent.TriggerEvent(this, waves[_index].waveEventID);
        }
    }
}
