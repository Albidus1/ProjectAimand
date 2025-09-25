using UnityEditor;
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

    [Header("설정")]
    public bool disableCollider = true;
    public bool disableDamageOnTouch = true;

    public bool isSameType { get; private set; }

    private Collider2D m_collider2D;
    private DamageOnTouch m_damageOnTouch;
    private SpriteRenderer m_spriteRenderer;





    private void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
        m_damageOnTouch = GetComponent<DamageOnTouch>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (m_damageOnTouch != null)
        {
            disableCollider = false;
        }

        SetMaterial();
    }

    private void SetMaterial()
    {
        if (m_spriteRenderer != null)
        {
            switch (state)
            {
                case ColorState.Normal:
                    m_spriteRenderer.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                    break;

                case ColorState.Invert:
                    m_spriteRenderer.material = Resources.Load<Material>("Shaders/Materials/ColorInversionMaterial");
                    break;
            }

            //if (false == m_spriteRenderer.material.name.Contains("ColorInversionMaterial"))
            //{
            //    m_spriteRenderer.material = Resources.Load<Material>("Shaders/Materials/ColorInversionMaterial");
            //}

            //float amount = m_material.GetFloat(InvertAmountID);

            //if (amount == 0)
            //{
            //    state = ColorState.Normal;
            //    m_material.SetFloat(InvertAmountID, 0);
            //}
            //else if (amount == 1)
            //{
            //    state = ColorState.Invert;
            //    m_material.SetFloat(InvertAmountID, 1);
            //}
            //else
            //{
            //    state = ColorState.None;
            //}
        }
    }

    private void SetCollider(bool _invert)
    {
        currentInvertState = _invert;

        if ((state == ColorState.Normal && false == _invert)
            || (state == ColorState.Invert && _invert))
        {
            EnableSettings();
        }
        else
        {
            DisableSettings();
        }
    }

    private void EnableSettings()
    {
        if (disableCollider && m_collider2D != null)
        {
            m_collider2D.enabled = true;
        }

        if (disableDamageOnTouch && m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = false;
        }

        isSameType = true;
    }

    private void DisableSettings()
    {
        if (disableCollider && m_collider2D != null)
        {
            m_collider2D.enabled = false;
        }

        if (disableDamageOnTouch && m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = true;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        string text = state == ColorState.Normal ? "Normal" : "Invert";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = state == ColorState.Normal ? Color.yellow : Color.blue;
        Handles.Label(transform.position + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), text, style);
    }

    private void Reset()
    {

    }

    private void OnValidate()
    {
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (m_spriteRenderer != null)
        {
            switch (state)
            {
                case ColorState.Normal:
                    m_spriteRenderer.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                    break;

                case ColorState.Invert:
                    m_spriteRenderer.material = Resources.Load<Material>("Shaders/Materials/ColorInversionMaterial");
                    break;
            }
        }
    }
#endif
}
