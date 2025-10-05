using Unity.Cinemachine;
using UnityEngine;



public struct CinemachineBrainEvent
{
    static CinemachineBrainEvent e;

    public float duration;

    public CinemachineBrainEvent(float _duration)
    {
        this.duration = _duration;
    }

    public static void Trigger(float _duration)
    {
        e.duration = _duration;
        EventManager.TriggerEvent(e);
    }
}

[RequireComponent(typeof(CinemachineBrain))]
public class CinemachineBrainController : MonoBehaviour, IEventListener<CinemachineBrainEvent>
{
    protected CinemachineBrain m_brain;



    protected virtual void Awake()
    {
        m_brain = GetComponent<CinemachineBrain>();
    }

    public void SetDefaultBlendDuration(float _duration)
    {
        m_brain.DefaultBlend.Time = _duration;
    }

    public void OnEvent(CinemachineBrainEvent _eventType)
    {
        SetDefaultBlendDuration(_eventType.duration);
    }
    
    protected virtual void OnEnable()
    {
        this.EventStartListening<CinemachineBrainEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<CinemachineBrainEvent>();
    }
}
