using UnityEngine;


public enum WaveEventStartTypes
{
    None,
    StartWave,
    EndWave,
    //WaveCompleted,
    //WaveFailed,
}

public enum ObjectTriggerTypes
{
    Enable,
    Disable,
    Activate,
    Deactivate,
    Toggle,
}

public struct WaveEvent
{
    static WaveEvent e;

    public EnemyWave wave;
    public string waveID;

    public WaveEvent(EnemyWave _wave, string _waveID)
    {
        wave = _wave;
        waveID = _waveID;
    }

    public static void TriggerEvent(EnemyWave _wave, string _waveID)
    {
        e.wave = _wave;
        e.waveID = _waveID;
        EventManager.TriggerEvent(e);
    }
}
