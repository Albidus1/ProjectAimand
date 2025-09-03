using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using static SoundManager;



public struct SoundManagerSoundPlayEvent
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInitialization()
    {
        OnEvent = null;
    }

    public delegate AudioSource Delegate(AudioClip _clip, SoundManagerPlayOptions _options);
    static private event Delegate OnEvent;

    static public void Register(Delegate _callback)
    {
        OnEvent += _callback;
    }

    static public void Unregister(Delegate _callback)
    {
        OnEvent -= _callback;
    }

    static public AudioSource Trigger(AudioClip _clip, SoundManagerPlayOptions _options)
    {
        return OnEvent?.Invoke(_clip, _options);
    }

    static public AudioSource Trigger(AudioClip _clip, SoundManagerTracks _soundManagerTrack, Vector3 _location,
        AudioMixerGroup _audioMixerGroup = null, int _ID = 0, bool _loop = false, float _volume = 0f, float _pitch = 0f,
        bool _fade = false, float _fadeInitialVolume = 0f, float _fadeDuration = 1f, Ease _fadeTween = Ease.InCubic,
        bool _persistent = false, int _priority = 128,
        AudioSource _recycleAudioSource = null,
        float _playbackTime = 0f, float _playbackDuration = 0f,
        Transform _attachToTransform = null, bool _doNotAutoRecycleIfNotDonePlaying = false)
    {
        SoundManagerPlayOptions options = SoundManagerPlayOptions.DefaultOption;

        options.soundManagerTrack = _soundManagerTrack;
        options.location = _location;
        options.audioMixerGroup = _audioMixerGroup;
        options.ID = _ID;
        options.loop = _loop;
        options.valume = _volume;
        options.pitch = _pitch;
        options.fade = _fade;
        options.fadeInitialVolume = _fadeInitialVolume;
        options.fadeDuration = _fadeDuration;
        options.fadeTween = _fadeTween;
        options.persistent = _persistent;
        options.priority = _priority;
        options.recycleAudioSource = _recycleAudioSource;
        options.playbackTime = _playbackTime;
        options.playbackDuration = _playbackDuration;
        options.attachToTransform = _attachToTransform;
        options.doNotAutoRecycleIfNotDonePlaying = _doNotAutoRecycleIfNotDonePlaying;

        return OnEvent?.Invoke(_clip, options);
    }
}
