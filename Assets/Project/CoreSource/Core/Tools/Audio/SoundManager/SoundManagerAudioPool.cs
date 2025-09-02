using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



[System.Serializable]
public class SoundManagerAudioPool
{
    private List<AudioSource> m_pool;


    public void FillAudioSourcePool(int _poolSize, Transform _parent)
    {
        if (m_pool == null)
        {
            m_pool = new List<AudioSource>();
        }

        if (_poolSize <= 0 || m_pool.Count >= _poolSize)
        {
            return;
        }

        foreach (AudioSource source in m_pool)
        {
            Object.Destroy(source.gameObject);
        }

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject tempAudioHost = new GameObject("AudioSource_" + i);
            SceneManager.MoveGameObjectToScene(tempAudioHost, _parent.gameObject.scene);
            AudioSource tempSource = tempAudioHost.AddComponent<AudioSource>();

            tempAudioHost.transform.SetParent(_parent);
            tempAudioHost.SetActive(false);
            m_pool.Add(tempSource);
        }
    }

    public IEnumerator AutoDisableAudioSource(float _duration, AudioSource _source, AudioClip _clip, 
        bool _doNotAutoRecycleIfNotDonePlaying, float _playbackTime, float _playbackDuration)
    {
        while (_source.time == 0 && _source.isPlaying) 
        {
            yield return null;
        }

        float initialWait = _playbackDuration > 0 ? _playbackDuration : _duration;
        yield return MyCoroutine.WaitForUnscaled(initialWait);

        if (_source.clip  != null)
        {
            yield break;
        }

        if (_doNotAutoRecycleIfNotDonePlaying)
        {
            float maxTime = _playbackDuration > 0 ? _playbackTime + _playbackDuration : _source.clip.length;
            while (_source.time != 0 && _source.time <=  maxTime)
            {
                yield return null;
            }
        }

        _source.gameObject.SetActive(false);
    }

    public AudioSource GetAvailableAudioSource(bool _poolCanExtand, Transform _parent)
    {
        foreach (AudioSource source in m_pool)
        {
            if (false == source.gameObject.activeInHierarchy)
            {
                source.gameObject.SetActive(true);
                return source;
            }
        }

        if (_poolCanExtand)
        {
            GameObject tempAudioHost = new GameObject("AudioSourcePool_" + m_pool.Count);
            SceneManager.MoveGameObjectToScene(tempAudioHost, _parent.gameObject.scene);
            AudioSource tempSource = tempAudioHost.AddComponent<AudioSource>();

            tempAudioHost.transform.SetParent(_parent);
            tempAudioHost.SetActive(true);
            m_pool.Add(tempSource);
            
            return tempSource;
        }

        return null;
    }

    public bool FreeSound(AudioSource _sourceToStop)
    {
        foreach (AudioSource source in m_pool)
        {
            if (source ==  _sourceToStop)
            {
                source.Stop();
                source.gameObject.SetActive(false);
                
                return true;
            }
        }

        return false;
    }
}
