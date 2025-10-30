using System.Collections;
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
    [Header("쿨타임")]
    public float coolTime = 0.5f;

    [Header("VFX")]
    public GameObject VFX;
    public SpriteRenderer shockWaveRenderer;
    [Range(1f, 15f)]
    public float shockWaveSpeed = 2.0f;

    [Header("SFX")]
    public PlaySound abilitySound;


    public KeyCode keyCode = KeyCode.Space;

    [Range(0, 1)]
    public int initialInvertAmount = 0;

    

    private InputManager m_inputManager;
    private PlayerMovement m_playerMovement;
    private List<ParticleSystem> m_particles = new List<ParticleSystem>();
    private Material m_material;
    private SpriteRenderer m_spriteRenderer;
    private int m_invertAmount;
    private bool m_inverted;
    private float m_coolTimer;

    private Material m_shockWaveMaterial;
    private float m_waveDistance;

    private static readonly int InvertAmountID = Shader.PropertyToID("_InvertAmount");
    private static readonly int WaveDistanceFromCenterID = Shader.PropertyToID("_WaveDistanceFromCenter");


    private void Awake()
    {
        m_playerMovement = GetComponent<PlayerMovement>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        m_material = m_spriteRenderer.material;
    }

    private void Start()
    {
        m_inputManager = FindFirstObjectByType<InputManager>();

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

        if (shockWaveRenderer != null)
        {
            m_shockWaveMaterial = shockWaveRenderer.material;
            m_waveDistance = -0.1f;
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
        if (GameManager.Instance.paused || GameManager.Instance.cameraEventActive)
        {
            return;
        }

        if (m_playerMovement != null && 
            (m_playerMovement.movementState.currentState == PlayerStates.MovementStates.Die))
        {
            return;
        }


        if (m_inputManager.ColorInvertButton.state.currentState == MyInput.ButtonStates.ButtonDown &&
            Time.time >= m_coolTimer)
        {
            Debug.Log("확인용");

            m_inverted = !m_inverted;
            m_invertAmount = m_inverted ? 1 : 0;

            //Debug.Log(m_inverted);
            SetInvertAmount(m_invertAmount);

            foreach (ParticleSystem ps in m_particles)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }

            if (abilitySound != null)
            {
                abilitySound.PlaySoundFX();
            }

            if (m_shockWaveMaterial != null)
            {
                StartCoroutine(ShockWave());
            }

            m_coolTimer = Time.time + coolTime;
        }
    }

    private IEnumerator ShockWave()
    {
        if (m_shockWaveMaterial == null)
        {
            yield break;
        }

        m_waveDistance = -0.1f;
        m_shockWaveMaterial.SetFloat(WaveDistanceFromCenterID, m_waveDistance);

        while (m_waveDistance <= 1.0f)
        {
            m_waveDistance += Time.deltaTime * shockWaveSpeed;
            m_shockWaveMaterial.SetFloat(WaveDistanceFromCenterID, m_waveDistance);
            yield return null;
        }

        m_shockWaveMaterial.SetFloat(WaveDistanceFromCenterID, -10f);
    }
}
