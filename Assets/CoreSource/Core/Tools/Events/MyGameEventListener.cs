using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;




public class MyGameEventListener : MonoBehaviour, IEventListener<GameEvent>
{
    public string eventName;
    public UnityEvent OnGameEvent;


    public void OnEvent(GameEvent _gameEvent)
    {
        if (_gameEvent.eventName == eventName)
        {
            OnGameEvent?.Invoke();
        }
    }    

    protected virtual void OnEnable()
    {
        this.EventStartListening<GameEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<GameEvent>();
    }
}