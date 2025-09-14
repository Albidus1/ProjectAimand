using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MyProgressBar : MonoBehaviour
{
    [Header("바인딩")]
    public Transform foregroundBar;

    [Header("값 설정")]
    [FormerlySerializedAs("StartValue")]
    [Range(0f, 1f)]
    public float minBarFillValue = 0f;
    [FormerlySerializedAs("EndValue")]
    [Range(0f, 1f)]
    public float maxBarFillValue = 1f;
    public bool setInitialFillValueOnStart = false;
    [MyConditionalHide("setInitialFillValueOnStart", true)]
    [Range(0f, 1f)]
    public float initialFillValue = 0f;


    protected bool m_isInitialized;
    protected Vector2 m_initialBarSize;
    protected Color m_initialBarColor;
    protected Vector3 m_initialScale;
    protected Vector3 m_targetLocalScale = Vector3.one;
    protected Image m_foregroundImage;
    protected float m_newPercent;


    private void Start()
    {
        if (false == m_isInitialized)
        {
            Initialization();
        }
    }

    private void OnEnable()
    {
        if (false == m_isInitialized)
        {
            return;
        }

        StoreInitialColor();
    }

    private void Initialization()
    {
        m_initialScale = transform.localScale;

        if (foregroundBar != null)
        {
            m_foregroundImage = foregroundBar.GetComponent<Image>();
            m_initialBarSize = m_foregroundImage.rectTransform.sizeDelta;
        }

        m_isInitialized = true;

        StoreInitialColor();

        if (setInitialFillValueOnStart)
        {
            SetBar01(initialFillValue);
        }
    }

    private void StoreInitialColor()
    {
        if (m_foregroundImage != null)
        {
            m_initialBarColor = m_foregroundImage.color;
        }
    }

    public void SetBar(float _currentValue, float _minValue, float _maxValue)
    {
        float newPercent = MyMaths.Remap(_currentValue, _minValue, _maxValue, 0f, 1f);
        SetBar01(newPercent);
    }

    public void SetBar01(float _newPercent)
    {
        if (false == m_isInitialized)
        {
            Initialization();
        }

        _newPercent = MyMaths.Remap(_newPercent, 0f, 1f, minBarFillValue, maxBarFillValue);
        SetBarInternal(_newPercent, foregroundBar, m_foregroundImage, m_initialBarSize);
    }

    public void UpdateBar01(float _normalizedValue)
    {
        UpdateBar(Mathf.Clamp01(_normalizedValue), 0f, 1f);
    }

    public void UpdateBar(float _currentValue, float _minValue, float _maxValue)
    {
        if (false == m_isInitialized)
        {
            Initialization();
        }
        
        // 

        SetBar(_currentValue, _minValue, _maxValue);
    }

    private void SetBarInternal(float _newAmount, Transform _bar, Image _image, Vector2 _initialSize)
    {
        if (_bar == null || _image == null)
        {
            return;
        }
        
        m_targetLocalScale = Vector3.one;
        m_targetLocalScale.x = _newAmount;

        _bar.localScale = m_targetLocalScale;
    }
}
