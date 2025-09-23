using UnityEngine;



public struct ColorInvertEvent
{
    static ColorInvertEvent e;

    public bool isInvert;

    public ColorInvertEvent(int _invertAmount)
    {
        isInvert = _invertAmount == 1;
    }

    public static void Trigger(int  _invertAmount)
    {
        e.isInvert = _invertAmount == 1;
        EventManager.TriggerEvent(e);
    }
}


public class PlayerSpriteColorInversion : MonoBehaviour
{
    public KeyCode keyCode = KeyCode.Space;

    [Range(0, 1)]
    public int initialInvertAmount = 0; 

    private Material m_material;
    private SpriteRenderer m_spriteRenderer;
    private int m_invertAmount;
    private bool m_inverted;

    private static readonly int InvertAmountID = Shader.PropertyToID("_InvertAmount");



    private void Awake()
    {
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_material = m_spriteRenderer.material;
    }

    private void Start()
    {
        m_invertAmount = initialInvertAmount;
        m_inverted = m_invertAmount == 1;
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        if (m_material != null)
        {
            m_material.SetFloat(InvertAmountID, m_invertAmount);

            ColorInvertEvent.Trigger(m_invertAmount);
        }
    }

    public void SetInvertAmount(int _invertAmount)
    {
        m_invertAmount = _invertAmount;
        UpdateShaderProperties();
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            if (m_inverted)
            {
                m_invertAmount = 0;
            }
            else
            {
                m_invertAmount = 1;
            }

            m_inverted = !m_inverted;

            SetInvertAmount(m_invertAmount);
        }
    }
}
