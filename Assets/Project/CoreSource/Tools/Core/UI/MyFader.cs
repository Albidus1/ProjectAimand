using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;



public struct MyFadeEvent
{
    static MyFadeEvent e;

    public float duration;
    public float targetAlpha;
    public Ease ease;
    public bool ignoreTimeScale;
    public Vector3 worldPosition;


    public MyFadeEvent(float _duration, float _targetAlpha, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        duration = _duration;
        targetAlpha = _targetAlpha;
        ease = _ease;
        ignoreTimeScale = _ignoreTimeScale;
        worldPosition = _worldPosition;
    }

    public static void Trigger(float _duration, float _targetAlpha)
    {
        Trigger(_duration, _targetAlpha, Ease.InCubic);
    }

    public static void Trigger(float _duration, float _targetAlpha, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        e.duration = _duration;
        e.targetAlpha = _targetAlpha;
        e.ease = _ease;
        e.ignoreTimeScale = _ignoreTimeScale;
        e.worldPosition = _worldPosition;

        EventManager.TriggerEvent(e);
    }
}

public struct MyFadeInEvent
{
    static MyFadeInEvent e;

    public float duration;
    public Ease ease;
    public bool ignoreTimeScale;
    public Vector3 worldPosition;


    public MyFadeInEvent(float _duration, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        duration = _duration;
        ease = _ease;
        ignoreTimeScale = _ignoreTimeScale;
        worldPosition = _worldPosition;
    }

    public static void Trigger(float _duration, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        e.duration = _duration;
        e.ease = _ease;
        e.ignoreTimeScale = _ignoreTimeScale;
        e.worldPosition = _worldPosition;

        EventManager.TriggerEvent(e);
    }
}

public struct MyFadeOutEvent
{
    static MyFadeOutEvent e;

    public float duration;
    public Ease ease;
    public bool ignoreTimeScale;
    public Vector3 worldPosition;


    public MyFadeOutEvent(float _duration, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        duration = _duration;
        ease = _ease;
        ignoreTimeScale = _ignoreTimeScale;
        worldPosition = _worldPosition;
    }

    public static void Trigger(float _duration, Ease _ease, bool _ignoreTimeScale = true, Vector3 _worldPosition = new Vector3())
    {
        e.duration = _duration;
        e.ease = _ease;
        e.ignoreTimeScale = _ignoreTimeScale;
        e.worldPosition = _worldPosition;

        EventManager.TriggerEvent(e);
    }
}

[RequireComponent(typeof(CanvasGroup))]
public class MyFader : MonoBehaviour, 
    IEventListener<MyFadeEvent>,
    IEventListener<MyFadeInEvent>,
    IEventListener<MyFadeOutEvent>
{
    public enum ForcedInitState { None, Active, Inactive }

    [Header("투명도 설정")]
    [Tooltip("비활성 상태의 투명도")]
    public float inactiveAlpha = 0f;
    [Tooltip("활성 상태의 투명도")]
    public float activeAlpha = 1f;
    [Tooltip("초기화 시 상태 설정")]
    public ForcedInitState forcedInitState = ForcedInitState.Inactive;

    [Header("시간 설정")]
    public float defaultFadeDuration = 0.2f;
    public Ease defaultEase = Ease.Linear;
    public bool ignoreTimeScale = true;
    public bool canFadeToCurrentAlpha = true;

    [Header("인터렉션 설정")]
    public bool shouldBlockRaycastsWhenActive = false;

    [Header("디버그")]
    [MyInspectorButtonBar(
        new string[] { "FadeIn1Second", "FadeOut1Second", "DefaultFade", "ResetFader" }, 
        new string[] { "FadeIn1Second", "FadeOut1Second", "DefaultFade", "ResetFader" },
        new bool[] { true, true, true, true }, 
        new string[] { "main-call-to-action", "", "", "" })]
    public bool debugToolbar;


    protected CanvasGroup m_canvasGroup;
    protected Image m_image;
    protected float m_initialAlpha;
    protected float m_currentTargetAlpha;
    protected float m_currentFadeDuration;
    protected Ease m_currentEase;

    protected bool m_fading = false;
    protected float m_fadeStartTime;
    protected bool m_frameCountOne;



    protected virtual void Awake()
    {
        Initialization();
    }

    protected virtual void Initialization()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_image = GetComponent<Image>();

        if (forcedInitState == ForcedInitState.Inactive)
        {
            m_canvasGroup.alpha = inactiveAlpha;
            m_image.enabled = false;
        }
        else if (forcedInitState == ForcedInitState.Active)
        {
            m_canvasGroup.alpha = activeAlpha;
            m_image.enabled = true;
        }
    }

    protected virtual void Update()
    {
        if (m_canvasGroup == null)
        {
            return;
        }

        if (m_fading)
        {
            Fade();
        }
    }

    protected virtual void Fade()
    {
        float currentTime = ignoreTimeScale ? Time.unscaledTime : Time.time;

        if (m_frameCountOne)
        {
            if (Time.frameCount <= 2)
            {
                m_canvasGroup.alpha = m_initialAlpha;
                return;
            }

            m_fadeStartTime = ignoreTimeScale ? Time.unscaledTime : Time.time;
            currentTime = m_fadeStartTime;
            m_frameCountOne = false;
        }

        float endTime = m_fadeStartTime + m_currentFadeDuration;
        if (currentTime - m_fadeStartTime < m_currentFadeDuration)
        {
            float t = Mathf.InverseLerp(m_fadeStartTime, endTime, currentTime);
            float easeValue = DG.Tweening.Core.Easing.EaseManager.Evaluate
                (m_currentEase,
                null,
                t,
                m_currentFadeDuration,
                0f,
                0f);
            float result = Mathf.Lerp(m_initialAlpha, m_currentTargetAlpha, easeValue);

            m_canvasGroup.alpha = result;
        }
        else
        {
            StopFading();
        }
    }

    protected virtual void StopFading()
    {
        m_canvasGroup.alpha = m_currentTargetAlpha;
        m_fading = false;

        if (Mathf.Approximately(m_canvasGroup.alpha, inactiveAlpha))
        {
            DisableFader();
        }
    }

    protected virtual void EnableFader()
    {
        m_image.enabled = true;

        if (shouldBlockRaycastsWhenActive)
        {
            m_canvasGroup.blocksRaycasts = true;
        }
    }   

    protected virtual void DisableFader()
    {
        m_image.enabled = false;

        if (shouldBlockRaycastsWhenActive)
        {
            m_canvasGroup.blocksRaycasts = false;
        }
    }

    protected virtual void StartFading(float _initialAlpha, float _endAlpha, float _duration, Ease _ease, bool _ignoreTimeScale)
    {
        if (false == canFadeToCurrentAlpha &&
            Mathf.Approximately(m_canvasGroup.alpha, _endAlpha))
        {
            return;
        }

        ignoreTimeScale = _ignoreTimeScale;
        EnableFader();
        m_fading = true;
        m_initialAlpha = _initialAlpha;
        m_currentTargetAlpha = _endAlpha;
        m_fadeStartTime = ignoreTimeScale ? Time.unscaledTime : Time.time;
        m_currentEase = _ease;
        m_currentFadeDuration = _duration;

        if (Time.frameCount == 1)
        {
            m_frameCountOne = true;
        }
    }

    public void OnEvent(MyFadeEvent _fadeEvent)
    {
        Fade(_fadeEvent.targetAlpha, _fadeEvent.duration, _fadeEvent.ease, _fadeEvent.ignoreTimeScale);
    }

    public void OnEvent(MyFadeInEvent _fadeEvent)
    {
        FadeIn(_fadeEvent.duration, _fadeEvent.ease, _fadeEvent.ignoreTimeScale);
    }

    public void OnEvent(MyFadeOutEvent _fadeEvent)
    {
        FadeOut(_fadeEvent.duration, _fadeEvent.ease, _fadeEvent.ignoreTimeScale);
    }

    public virtual void Fade(float _targetAlpha, float _duration, Ease _ease, bool _ignoreTimeScale)
    {
        m_currentTargetAlpha = _targetAlpha == -1 ? activeAlpha : _targetAlpha;
        StartFading(m_canvasGroup.alpha, _targetAlpha, _duration, _ease, _ignoreTimeScale);
    }

    public virtual void FadeIn(float _duration, Ease _ease, bool _ignoreTimeScale = true)
    {
        StartFading(inactiveAlpha, activeAlpha, _duration, _ease, _ignoreTimeScale);
    }

    public virtual void FadeOut(float _duration, Ease _ease, bool _ignoreTimeScale = true)
    {
        StartFading(activeAlpha, inactiveAlpha, _duration, _ease, _ignoreTimeScale);
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<MyFadeEvent>();
        this.EventStartListening<MyFadeInEvent>();
        this.EventStartListening<MyFadeOutEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<MyFadeEvent>();
        this.EventStopListening<MyFadeInEvent>();
        this.EventStopListening<MyFadeOutEvent>();
    }

    #region DEBUG
    protected virtual void FadeIn1Second()
    {
        MyFadeInEvent.Trigger(1f, Ease.Linear);
    }

    protected virtual void FadeOut1Second()
    {
        MyFadeOutEvent.Trigger(1f, Ease.Linear);
    }

    protected virtual void DefaultFade()
    {
        //Debug.Log("기본 페이드 효과 확인용");
        MyFadeEvent.Trigger(defaultFadeDuration, activeAlpha, defaultEase);
    }

    protected virtual void ResetFader()
    {
        //Debug.Log("페이더 초기화");
        m_canvasGroup.alpha = inactiveAlpha;
    }
    #endregion
}
