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
    TogglePause,
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

public class GameManager : MyPersistentSingleton<GameManager>,
    IEventListener<GameEvent>,
    IEventListener<MainEvent>
{
    public bool paused { get; set; }

    protected bool m_pauseMenuOpen = false;



    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {

    }

    public void Reset()
    {
        paused = false;
    }
    

    public virtual void Pause()
    {
        Debug.Log("퍼즈");

        Instance.paused = true;

        if (GUIManager.HasInstance)
        {
            GUIManager.Instance.SetPause(true);
            m_pauseMenuOpen = true;
        }
    }

    public virtual void UnPause()
    {
        Debug.Log("퍼즈 해제");

        Instance.paused = false;

        if (GUIManager.HasInstance)
        {
            GUIManager.Instance.SetPause(false);
            m_pauseMenuOpen = false;
        }
    }

    public virtual void OnEvent(GameEvent _gameEvent)
    {
        
    }

    public virtual void OnEvent(MainEvent _mainEvent)
    {
        switch (_mainEvent.eventType)
        {
            case MainEventTypes.TogglePause:
                if (paused)
                { 
                    UnPause();
                }
                else
                {
                    Pause();
                }
                break;
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
