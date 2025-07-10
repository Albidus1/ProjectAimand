using System;
using UnityEditor;
using UnityEngine;



public enum MainEventTypes
{
    LevelStart,
    LevelComplete,
    LevelEnd,
    Pause,
    UnPause,
    PlayerDeath,
    PlayerRespawn,
}

public struct MainEvent
{
    static MainEvent e;

    public MainEventTypes eventType;
    public PlayerMovement player;

    public MainEvent(MainEventTypes _eventType, PlayerMovement _player = null)
    {
        eventType = _eventType;
        player = _player;
    }

    public static void Trigger(MainEventTypes _eventType, PlayerMovement _player = null)
    {
        e.eventType = _eventType;
        e.player = _player;
        EventManager.TriggerEvent(e);
    }
}

public class GameManager : MySingleton<GameManager>,
    IEventListener<GameEvent>,
    IEventListener<MainEvent>
{
    public bool paused { get; set; }



    protected override void Awake()
    {
        base.Awake();
    }

    public virtual void Pause()
    {
        if (Time.timeScale > 0)
        {
            Instance.paused = true;
        }
    }

    public virtual void UnPause()
    {
        throw new NotImplementedException();
    }

    public virtual void OnEvent(GameEvent _gameEvent)
    {
        switch (_gameEvent.eventName)
        {

        }
    }

    public virtual void OnEvent(MainEvent _mainEvent)
    {
        switch (_mainEvent.eventType)
        {
            case MainEventTypes.Pause:
                Pause();
                break;
            case MainEventTypes.UnPause:
                UnPause();
                break;
        }
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<GameEvent>();
        this.EventStartListening<MainEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<GameEvent>();
        this.EventStopListening<MainEvent>();
    }
}
