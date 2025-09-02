using System;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public struct SoundManagerSound
{
    public int id;
    public SoundManager.SoundManagerTracks track;
    public AudioSource audioSource;

    public bool persistent;

    public float playbackTime;
    public float playbackDuration;
}

public class SoundManager : MyPersistentSingleton<SoundManager>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void InitializeStatics()
    {
        m_instance = null;
    }

    public enum SoundManagerTracks
    {
        Master,
        Music,
        SFX,
        UI,
        Other
    }

    [Header("설정")]
    public SoundManagerSettingsSO settingsSO;

    [Header("풀링 설정")]
    public int audioSourcePoolSize = 10;
    public bool poolCanExpand = true;


    protected GameObject m_tempAudioSourceGameObject;
    protected SoundManagerAudioPool m_pool;
    protected SoundManagerSound m_sound;
    protected List<SoundManagerSound> m_sounds;
    protected AudioSource m_tempAudioSource;
    protected Dictionary<AudioSource, Coroutine> m_fadeInSoundCorotines;
    protected Dictionary<AudioSource, Coroutine> m_fadeOutSoundCorotines;
    protected Dictionary<SoundManagerSound, Coroutine> m_fadeTrackCorotines;

    

    protected override void Awake()
    {
        base.Awake();
        InitializeSoundManager();
    }

    protected virtual void Start()
    {
        if (settingsSO != null && settingsSO.settings.autoLoad)
        {
            settingsSO.LoadSoundSettings();
        }
    }
    protected virtual void InitializeSoundManager()
    {
        if (m_pool == null)
        {
            m_pool = new SoundManagerAudioPool();
        }

        m_pool.FillAudioSourcePool(audioSourcePoolSize, transform);
        m_sounds = new List<SoundManagerSound>();
        m_fadeInSoundCorotines = new Dictionary<AudioSource, Coroutine>();
        m_fadeOutSoundCorotines = new Dictionary<AudioSource, Coroutine>();
        m_fadeTrackCorotines = new Dictionary<SoundManagerSound, Coroutine>();
    }

    //public virtual AudioSource PlaySound(AudioClip _clip, SoundManager)
}
