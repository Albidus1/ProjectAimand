using DG.Tweening;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;




[System.Serializable]
public struct SoundManagerSound
{
    public int ID;
    public SoundManager.SoundManagerTracks track;
    public AudioSource audioSource;

    public bool persistent;

    public float playbackTime;
    public float playbackDuration;
}

public class SoundManager : MyPersistentSingleton<SoundManager>,
    IEventListener<SoundManagerTrackFadeEvent>
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
    protected Dictionary<SoundManagerTracks, Coroutine> m_fadeTrackCorotines;



    #region INITIALIZAION
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
        m_fadeTrackCorotines = new Dictionary<SoundManagerTracks, Coroutine>();
    }
    #endregion

    #region PLAY SOUND
    public virtual AudioSource PlaySound(AudioClip _clip, SoundManagerPlayOptions options)
    {
        return PlaySound(
            _clip,
            options.soundManagerTrack,
            options.location,
            options.audioMixerGroup,
            options.ID,
            options.loop,
            options.valume,
            options.pitch,
            options.fade,
            options.fadeInitialVolume,
            options.fadeDuration,
            options.fadeTween,
            options.persistent,
            options.priority,
            options.recycleAudioSource,
            options.playbackTime,
            options.playbackDuration,
            options.attachToTransform,
            options.doNotAutoRecycleIfNotDonePlaying,
            options.specialBlend,
            options.rolloffMode,
            options.minDistance,
            options.maxDistance);
    }

    public virtual AudioSource PlaySound(AudioClip _audioClip, SoundManagerTracks _soundManagerTrack, Vector3 _location,
        AudioMixerGroup _audioMixerGroup = null, int _ID = 0, bool _loop = false, float _volume = 1f, float _pitch = 1f, 
        bool _fade = false, float _fadeInitialVolume = 0f, float _fadeDuration = 1f, Ease _fadeTween = Ease.Unset,
        bool _persistent = false, int _priority = 128,
        AudioSource _recycleAudioSource = null,
        float _playbackTime = 0f, float _playbackDuration = 0f,
        Transform _attachToTransform = null, bool _doNotAutoRecycleIfNotDonePlaying = false, float _specialBlend = 0f,
        AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic, float _minDistance = 0f, float _maxDistance = 500f)
    {
        if (this == null)
        {
            return null;
        }

        if (_audioClip == null)
        {
            return null;
        }

        AudioSource audioSource = _recycleAudioSource;
        if (audioSource == null)
        {
            audioSource = m_pool.GetAvailableAudioSource(poolCanExpand, transform);

            if (audioSource != null && false == _loop)
            {
                _recycleAudioSource = audioSource;
                StartCoroutine(m_pool.AutoDisableAudioSource(_audioClip.length / Mathf.Abs(_pitch), audioSource, _audioClip, _doNotAutoRecycleIfNotDonePlaying, _playbackTime, _playbackDuration));
            }
        }
        if (audioSource == null)
        {
            m_tempAudioSourceGameObject = new GameObject("Audio_" + _audioClip.name);
            SceneManager.MoveGameObjectToScene(m_tempAudioSourceGameObject, this.gameObject.scene);
            audioSource = m_tempAudioSourceGameObject.AddComponent<AudioSource>();
        }

        audioSource.transform.position = _location;
        audioSource.clip = _audioClip;
        audioSource.loop = _loop;
        audioSource.pitch = _pitch;
        audioSource.priority = _priority;
        audioSource.spatialBlend = _specialBlend;
        audioSource.rolloffMode = _rolloffMode;
        audioSource.minDistance = _minDistance;
        audioSource.maxDistance = _maxDistance;
        audioSource.time = _playbackTime;

        if (_attachToTransform != null)
        {
            // 따라갈 타겟이 필요할 경우
        }

        if (settingsSO != null)
        {
            audioSource.outputAudioMixerGroup = settingsSO.masterAudioMixerGroup;
            switch (_soundManagerTrack)
            {
                case SoundManagerTracks.Master:
                    audioSource.outputAudioMixerGroup = settingsSO.masterAudioMixerGroup;
                    break;
                case SoundManagerTracks.Music:
                    audioSource.outputAudioMixerGroup = settingsSO.musicAudioMixerGroup;
                    break;
                case SoundManagerTracks.SFX:
                    audioSource.outputAudioMixerGroup = settingsSO.sfxAudioMixerGroup;
                    break;
                case SoundManagerTracks.UI:
                    audioSource.outputAudioMixerGroup = settingsSO.uiAudioMixerGroup;
                    break;
            }
        }

        if (_audioMixerGroup)
        {
            audioSource.outputAudioMixerGroup = _audioMixerGroup;
        }
        
        audioSource.volume = _volume;

        audioSource.Play();

        if (false == _loop && _recycleAudioSource == null)
        {
            float destroyDelay = _playbackDuration > 0 ? _playbackDuration : _audioClip.length - _playbackTime;
            Destroy(m_tempAudioSourceGameObject, destroyDelay);
        }

        if (_fade)
        {
            FadeSound(audioSource, _fadeDuration, _fadeInitialVolume, _volume, _fadeTween);
        }

        m_sound.ID = _ID;
        m_sound.track = _soundManagerTrack;
        m_sound.audioSource = audioSource;
        m_sound.persistent = _persistent;
        m_sound.playbackTime = _playbackTime;
        m_sound.playbackDuration = _playbackDuration;

        bool alreadyIn = false;
        for (int i = 0; i < m_sounds.Count; i++)
        {
            if (m_sounds[i].audioSource == audioSource)
            {
                m_sounds[i] = m_sound;
                alreadyIn = true;
            }
        }

        if (false == alreadyIn) 
        {
            m_sounds.Add(m_sound);
        }

        return audioSource;
    }
    #endregion

    #region SOUND CONTROLS
    public virtual void PauseSound(AudioSource _source)
    {
        _source.Pause();
    }

    public virtual void ResumeSound(AudioSource _source)
    {
        _source.Play();
    }

    public virtual void StopSound(AudioSource _source)
    {
        _source.Stop();
    }

    public virtual void FreeSound(AudioSource _source)
    {
        _source.Stop();

        if (false == m_pool.FreeSound(_source))
        {
            Destroy(_source.gameObject);
        }
    }
    #endregion

    #region FADE METHODS
    public void FadeTrack(SoundManagerTracks _track, float _duration, float _initialVolume = 0f, float _finalVolume = 1f, Ease _tweenType = Ease.Unset)
    {
        Coroutine coroutine = StartCoroutine(FadeTrackCoroutine(_track, _duration, _initialVolume, _finalVolume, _tweenType));
        m_fadeTrackCorotines[_track] = coroutine;
    }

    public void FadeSound(AudioSource _source, float _duration, float _initialVolume = 0f, float _finalVolume = 1f, Ease _tweenType = Ease.Unset, bool _freeAfterFade = false)
    {
        Coroutine coroutine = StartCoroutine(FadeCoroutine(_source, _duration, _initialVolume, _finalVolume, _tweenType, _freeAfterFade));
        if (_initialVolume < _finalVolume)
        {
            m_fadeInSoundCorotines[_source] = coroutine;
        }
        else
        {
            m_fadeOutSoundCorotines[_source] = coroutine;
        }
    }

    private IEnumerator FadeTrackCoroutine(SoundManagerTracks _track, float _duration, float _initialVolume = 0f, float _finalVolume = 1f, Ease _tweenType = Ease.Unset)
    {
        if (_tweenType == Ease.Unset)
        {
            _tweenType = Ease.InOutQuart;
        }

        bool tweenCompleted = false;

        DOTween.To(
            () => _initialVolume,
            x => settingsSO.SetTrackVolume(_track, x),
            _finalVolume,
            _duration
        )
        .SetUpdate(true)
        .SetEase(_tweenType)
        .OnComplete(() => {
            tweenCompleted = true;
        });

        while (!tweenCompleted)
        {
            yield return null;
        }

        settingsSO.SetTrackVolume(_track, _finalVolume);
    }

    private IEnumerator FadeCoroutine(AudioSource _source, float _duration, float _initialVolume, float _finalVolume, Ease _tweenType, bool _freeAfterFade)
    {
        bool isComplete = false;

        if (_tweenType == Ease.Unset)
        {
            _tweenType = Ease.InOutQuart;
        }

        DOTween.To(
            () => _source.volume,
            x => _source.volume = x,
            _finalVolume,
            _duration)
            .SetUpdate(true)
            .SetEase(_tweenType)
            .OnComplete(() =>
            {
                _source.volume = _finalVolume;
                isComplete = true;
            });

        yield return new WaitUntil(() => isComplete);

        if (_freeAfterFade)
        {
            FreeSound(_source);
        }

        if (_initialVolume < _finalVolume)
        {
            m_fadeInSoundCorotines[_source] = null;
        }
        else
        {
            m_fadeOutSoundCorotines[_source] = null;
        }
    }

    public void StopFadeTrack(SoundManagerTracks _track)
    {
        Coroutine outCoroutine;
        if (m_fadeTrackCorotines.TryGetValue(_track, out outCoroutine))
        {
            StopCoroutine(outCoroutine);
            m_fadeTrackCorotines.Remove(_track);
        }
    }
    #endregion

    #region ALL SOUNDS CONTROLS
    public virtual void FreeAllSoundsButPersistent()
    {
        foreach (SoundManagerSound sound in m_sounds)
        {
            if (false == sound.persistent && sound.audioSource != null)
            {
                FreeSound(sound.audioSource);
            }
        }
    }
    #endregion

    #region FIND SOUND SOURCE
    public virtual AudioSource FindByID(int _ID)
    {
        foreach (SoundManagerSound sound in m_sounds)
        {
            if (sound.ID == _ID)
            {
                return sound.audioSource;
            }
        }

        return null;
    }

    public virtual AudioSource FindByClip(AudioClip _clip)
    {
        foreach (SoundManagerSound sound in m_sounds)
        {
            if (sound.audioSource.clip == _clip)
            {
                return sound.audioSource;
            }
        }

        return null;
    }
    #endregion

    #region EVENT
    public virtual void OnEvent(SoundManagerTrackFadeEvent _e)
    {
        switch (_e.mode)
        {
            case SoundManagerTrackFadeEvent.Modes.PlayFade:
                FadeTrack(_e.track, _e.fadeDuration, settingsSO.GetTrackVolume(_e.track), _e.finalVolume, _e.ease);
                break;

            case SoundManagerTrackFadeEvent.Modes.StopFade:
                StopFadeTrack(_e.track);
                break;
        }
    }

    protected virtual void OnEnable()
    {
        SoundManagerSoundPlayEvent.Register(OnSoundManagerSoundPlayEvent);
        this.EventStartListening<SoundManagerTrackFadeEvent>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected void OnDisable()
    {
        if (base.m_enabled)
        {
            SoundManagerSoundPlayEvent.Unregister(OnSoundManagerSoundPlayEvent);
            this.EventStopListening<SoundManagerTrackFadeEvent>();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public virtual AudioSource OnSoundManagerSoundPlayEvent(AudioClip _clip, SoundManagerPlayOptions _options)
    {
        return PlaySound(_clip, _options);
    }


    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        FreeAllSoundsButPersistent();
    }
    #endregion
}
