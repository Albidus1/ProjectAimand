using UnityEngine;

public class DetectColorInversion : MonoBehaviour, IEventListener<ColorInvertEvent>
{
    public enum ColorState
    {
        None,
        Normal,
        Invert,
    }

    [MyReadOnly]
    public bool currentInvertState;

    public ColorState state;

    private Collider2D m_collider2D;
    private DamageOnTouch m_damageOnTouch;



    private void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
        m_damageOnTouch = GetComponent<DamageOnTouch>();
    }

    private void SetCollider(bool _invert)
    {
        currentInvertState = _invert;

        if ((state == ColorState.Normal && false == _invert)
            || (state == ColorState.Invert && _invert))
        {
            EnableDamage();
        }
        else
        {
            DisableDamage();
        }
    }

    private void EnableDamage()
    {
        if (m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = true;
        }
    }

    private void DisableDamage()
    {
        if (m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = false;
        }
    }


    public void OnEvent(ColorInvertEvent e)
    {
        SetCollider(e.isInvert);
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<ColorInvertEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<ColorInvertEvent>();
    }
}
