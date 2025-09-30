using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;



public class PlaySound : MonoBehaviour
{
    public bool active = true;

    public List<AudioClip> soundFXs = new List<AudioClip>();
    public bool loop;

    [Header("특수 설정")]
    public Transform attachToTransform;

    public bool randomSet;

    protected AudioSource m_audioSource;
    protected int m_index;
    protected float m_specialBlend;




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
            m_audioSource = SoundManagerSoundPlayEvent.Trigger
                (soundFXs[m_index],
                SoundManager.SoundManagerTracks.SFX,
                transform.position,
                _loop:loop,
                _attachToTransform:attachToTransform);

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

    //private void Update()
    //{
    //    if (false == useDistanceSound)
    //    {
    //        return;
    //    }

    //    if (m_audioSource == null)
    //    { 
    //        return;
    //    }

    //    Vector3 relativePosition = Camera.main.transform.position - m_audioSource.transform.position;
    //    bool outOfRange = relativePosition.magnitude > m_audioSource.maxDistance;

    //    if (m_audioSource.isPlaying && outOfRange)
    //    {
    //        StopSoundFX();
    //    }
    //}
}
