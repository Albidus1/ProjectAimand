using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;



public class PlaySound : MonoBehaviour
{
    public bool active = true;

    public List<AudioClip> soundFXs = new List<AudioClip>();
    public bool loop;

    [Header("거리 설정")]
    public bool useDistanceSound;
    [MyConditionalHide("useDistanceSound", true)]
    public AudioRolloffMode rolloffMode;
    [MyConditionalHide("useDistanceSound", true)]
    public float minDistance = 0f;
    [MyConditionalHide("useDistanceSound", true)]
    public float maxDistance = 500f;
    [Space(10)]

    public bool randomSet;

    protected AudioSource m_audioSource;
    protected int m_index;
    protected float m_specialBlend;



    private void Start()
    {
        if (useDistanceSound)
        {
            PlaySoundFX();
        }
    }

    [ContextMenu("사운드 출력")]
    public void PlaySoundFX()
    {
        if (false == active)
        {
            return;
        }


        if (randomSet)
        {
            m_index = Random.Range(0, soundFXs.Count);
        }

        if (soundFXs.Count > 0)
        {
            m_specialBlend = useDistanceSound ? 1f : 0f;

            m_audioSource = SoundManagerSoundPlayEvent.Trigger
                (soundFXs[m_index],
                SoundManager.SoundManagerTracks.SFX,
                transform.position,
                _loop:loop,
                _specialBlend:m_specialBlend,
                _rolloffMode:rolloffMode,
                _minDistance:minDistance,
                _maxDistance:maxDistance);

            m_index++;

            if (m_index >= soundFXs.Count)
            {
                m_index = 0;
            }
        }
    }

    [ContextMenu("사운드 종료")]
    public void StopSoundFX()
    {
        if (m_audioSource == null)
        {
            return;
        }

        if (loop)
        {
            m_audioSource.Stop();
        }

        SoundManager.Instance.FreeSound(m_audioSource);
    }

    private void Update()
    {
        if (m_audioSource == null || false == useDistanceSound)
        {
            return;
        }

        Vector3 relativePosition = Camera.main.transform.position - m_audioSource.transform.position;
        bool isAudible = relativePosition.magnitude <= m_audioSource.maxDistance;

        if (false == m_audioSource.isPlaying && isAudible)
        {
            PlaySoundFX();
        }
        if (m_audioSource.isPlaying && false == isAudible)
        {
            StopSoundFX();
        }
    }
}
