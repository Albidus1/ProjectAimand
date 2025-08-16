using UnityEngine;


public enum WaveEventStartTypes
{
    None,
    StartWave,
    EndWave,
    WaveCompleted,
    WaveFailed,
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
    public ObjectTriggerTypes eventTriggerType;
    public string waveID;

    public WaveEvent(EnemyWave _wave, ObjectTriggerTypes _type, string _waveID)
    {
        wave = _wave;
        eventTriggerType = _type;
        waveID = _waveID;
    }

    public static void TriggerEvent(EnemyWave _wave, ObjectTriggerTypes _type, string _waveID)
    {
        e.wave = _wave;
        e.eventTriggerType = _type;
        e.waveID = _waveID;
        EventManager.TriggerEvent(e);
    }
}
