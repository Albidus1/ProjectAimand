using UnityEngine;
using UnityEngine.PlayerLoop;

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

    public bool isSameType { get; private set; }

    private Collider2D m_collider2D;
    private DamageOnTouch m_damageOnTouch;
    private SpriteRenderer m_spriteRenderer;
    private Color m_color;



    private void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
        m_damageOnTouch = GetComponent<DamageOnTouch>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (m_spriteRenderer != null)
        {
            m_color = m_spriteRenderer.color;
        }
    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (m_spriteRenderer != null && m_color != null)
        {
            Debug.Log(m_color);

            if (m_color.r <= 0.1f &&  m_color.g <= 0.1f && m_color.b <= 0.1f)
            {
                state = ColorState.Normal;
            }
            else if (m_color.r >= 0.9f && m_color.g >= 0.9f && m_color.b >= 0.9f)
            {
                state = ColorState.Invert;
            }
            else
            {
                state = ColorState.None;
            }
        }
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

        isSameType = true;
    }

    private void DisableDamage()
    {
        if (m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = false;
        }

        isSameType = false;
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
