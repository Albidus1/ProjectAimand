using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;



public class LightControl : MonoBehaviour
{
    public float intensity = 1.2f;

    [Header("켜질때 설정")]
    public float startOuterRadius = 0f;
    public float targetOuterRadius = 5f;
    public float fadeDuration;

    [Header("일렁임 설정")]
    public bool flickerEnabled = true;
    public float flickerSpeed = 5f;
    public float flickerAmount = 0.2f;


    private bool m_isEnabled = false;
    private Light2D m_spotLight;
    private float m_baseIntensity;



    private void Awake()
    {
        m_spotLight = GetComponentInChildren<Light2D>();
    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (m_spotLight != null)
        {
            m_spotLight.lightType = Light2D.LightType.Point;
            m_spotLight.intensity = intensity;
            m_spotLight.pointLightOuterRadius = startOuterRadius;
            m_baseIntensity = targetOuterRadius;
        }

        m_isEnabled = false;
    }

    private void OnEnable()
    {
        Initialization();
    }

    public void TurnLight()
    {
        if (m_isEnabled)
        {
            return;
        }

        StartCoroutine(LightOnEffect());
    }

    private IEnumerator LightOnEffect()
    {
        m_isEnabled = true;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            m_spotLight.pointLightOuterRadius = Mathf.Lerp(startOuterRadius, targetOuterRadius, time / fadeDuration);
            yield return null;
        }

        m_spotLight.pointLightOuterRadius = targetOuterRadius;
    }

    private void Update()
    {
        if (m_spotLight == null)
        {
            return;
        }

        if (flickerEnabled && m_isEnabled)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float flicker = Mathf.Lerp(1f -  flickerAmount, 1f + flickerAmount, noise);
            m_spotLight.pointLightOuterRadius = m_baseIntensity * flicker;
        }
    }
}
