using System;
using UnityEditor;
using UnityEngine;



public enum MainEventTypes
{
    LevelStart,
    LevelComplete,
    LevelEnd,
    GameOver,
    LoadNextLevel,
    Pause,
    UnPause,
    TogglePause,
    SpawnPlayer,
    PlayerDeath,
    PlayerRespawn,
    CameraEventEnabled,
    CameraEventDisabled,
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
    public bool cameraEventActive { get; private set; }

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
        //Debug.Log("퍼즈");

        Instance.paused = true;


        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;

            if (GUIManager.HasInstance)
            {
                GUIManager.Instance.SetPause(true);
                m_pauseMenuOpen = true;
            }
        }

        SetCursorVisible(true);
    }

    public virtual void UnPause()
    {
        //Debug.Log("퍼즈 해제");

        Instance.paused = false;
        Time.timeScale = 1f;

        if (GUIManager.HasInstance)
        {
            GUIManager.Instance.SetPause(false);
            m_pauseMenuOpen = false;
        }

        SetCursorVisible(false);
    }

    public virtual void SetCursorVisible(bool _visible)
    {
        Debug.Log("커서 상태 변경: " + _visible);

        Cursor.visible = _visible;
        Cursor.lockState = _visible ? CursorLockMode.None : CursorLockMode.Locked;
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

            case MainEventTypes.CameraEventEnabled:
                cameraEventActive = true;
                break;

            case MainEventTypes.CameraEventDisabled:
                cameraEventActive = false;
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
