using System.Collections.Generic;
using UnityEngine;



public struct ColorInvertEvent
{
    static ColorInvertEvent e;

    public bool isInvert;

    public ColorInvertEvent(bool _inverted)
    {
        isInvert = _inverted;
    }

    public static void Trigger(bool _inverted)
    {
        e.isInvert = _inverted;
        EventManager.TriggerEvent(e);
    }
}


public class PlayerSpriteColorInversion : MonoBehaviour
{
    [Header("VFX")]
    public GameObject VFX;

    public KeyCode keyCode = KeyCode.Space;

    [Range(0, 1)]
    public int initialInvertAmount = 0;


    private List<ParticleSystem> m_particles = new List<ParticleSystem>();
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

        if (VFX == null)
        {
            VFX = transform.Find("VFX").gameObject;
        }

        if (VFX != null)
        {
            for (int i = 0; i < VFX.transform.childCount; i++)
            {
                if (VFX.transform.GetChild(i).TryGetComponent<ParticleSystem>(out var ps))
                {
                    m_particles.Add(ps);
                }
            }
        }
    }

    private void OnEnable()
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
            ColorInvertEvent.Trigger(m_inverted);
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
            m_inverted = !m_inverted;
            m_invertAmount = m_inverted ? 1 : 0;

            Debug.Log(m_inverted);
            SetInvertAmount(m_invertAmount);

            foreach (ParticleSystem ps in m_particles)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }
    }
}
