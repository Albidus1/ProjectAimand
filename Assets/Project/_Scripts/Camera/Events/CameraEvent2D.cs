using UnityEngine;



public enum CameraEventType { SetTargetCharacter, SetConfiner, StartFollowing, StopFollowing, ResetPriorities }

public struct CameraEvent2D
{
    static CameraEvent2D e;

    public CameraEventType eventType;
    public PlayerMovement targetCharacter;
    public Collider2D bounds2D;

    public CameraEvent2D(CameraEventType _type, PlayerMovement _targetCharacter = null, Collider2D _confiner = null)
    {
        this.eventType = _type;
        this.targetCharacter = _targetCharacter;
        this.bounds2D = _confiner;
    }

    public static void Trigger(CameraEventType _type, PlayerMovement _targetCharacter = null, Collider2D _confiner = null)
    {
        e.eventType = _type;
        e.targetCharacter = _targetCharacter;
        e.bounds2D = _confiner;

        EventManager.TriggerEvent(e);
    }
}
