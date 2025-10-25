using System.Collections;
using Unity.Cinemachine;
using UnityEngine;



public struct CameraShakeEvent
{
    static CameraShakeEvent e;

    public string cameraID;
    public float duration;
    public float amplitude;
    public float frequency;

    public CameraShakeEvent (string _id, float _duration, float _amplitude, float _frequency)
    {
        cameraID = _id;
        duration = _duration;
        amplitude = _amplitude;
        frequency = _frequency;
    }

    public static void Trigger(string _id, float _duration, float _amplitude, float _frequency)
    {
        e.cameraID = _id;
        e.duration = _duration;
        e.amplitude = _amplitude;
        e.frequency = _frequency;
        
        EventManager.TriggerEvent(e);
    }
}

public class CinemachineCameraShake : MonoBehaviour, IEventListener<CameraShakeEvent>
{
    protected CinemachineCamera m_cmCamera;
    protected CinemachineBasicMultiChannelPerlin m_perlin;

    protected string m_cameraID = "MainCamera";
    protected float m_duration;
    protected float m_amplitude;
    protected float m_frequency;


    private void Awake()
    {
        m_cmCamera = GetComponent<CinemachineCamera>();
        if (m_cmCamera != null)
        {
            m_perlin = m_cmCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        }
    }

    private IEnumerator StartShack()
    {
        m_perlin.AmplitudeGain = m_amplitude;
        m_perlin.FrequencyGain = m_frequency;

        yield return new WaitForSeconds(m_duration);

        m_perlin.AmplitudeGain = 0f;
        m_perlin.FrequencyGain = 0f;
    }

    public void OnEvent(CameraShakeEvent _e)
    {
        if (_e.cameraID != m_cameraID)
        {
            return;
        }

        if (m_perlin == null)
        {
            return;
        }

        m_duration = _e.duration;
        m_amplitude = _e.amplitude;
        m_frequency = _e.frequency;

        StartCoroutine(StartShack());
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<CameraShakeEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<CameraShakeEvent>();
    }
}
