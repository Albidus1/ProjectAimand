using UnityEngine;



public struct TriggerEvent
{
    static TriggerEvent e;

    public string eventID;
    public TriggerEventSetting setting;

    public TriggerEvent(string _eventID, TriggerEventSetting _setting)
    {
        eventID = _eventID;
        setting = _setting;
    }
    public static void Trigger(string _eventID, TriggerEventSetting _setting)
    {
        e.eventID = _eventID;
        e.setting = _setting;
        EventManager.TriggerEvent(e);
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class TriggerEventSetting : MonoBehaviour
{
    [Header("이벤트")]
    public string eventID = "default";

    [Header("기본 설정")]
    public bool triggerOnce;
    public float triggerCooldownTime;
    public LayerMask targetLayer;


    public bool isTrigger => m_triggered;

    protected BoxCollider2D m_boxCollider2D;
    protected bool m_triggerOnce;
    protected bool m_triggered;



    protected virtual void Awake()
    {
        m_boxCollider2D = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        if (m_boxCollider2D != null)
        {
            m_boxCollider2D.isTrigger = true; 
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Trigger();
        }
    }

    protected virtual void Trigger()
    {
        if (m_triggered || triggerOnce)
        {
            return;
        }

        m_triggerOnce = triggerOnce;
        m_triggered = true;

        TriggerEvent.Trigger(eventID, this);

        if (triggerCooldownTime > 0)
        {
            Invoke(nameof(ResetTriggerCooldown), triggerCooldownTime);
        }
    }

    public virtual void ResetTrigger()
    {
        m_triggered = false;
        m_triggerOnce = false;
    }

    protected virtual void ResetTriggerCooldown() => m_triggered = false;




#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (m_boxCollider2D == null)
        {
            m_boxCollider2D = GetComponent<BoxCollider2D>();
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_boxCollider2D.bounds.center, m_boxCollider2D.bounds.size);
    }
#endif
}
