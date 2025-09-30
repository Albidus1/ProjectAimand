using System;
using UnityEngine;
using UnityEngine.Audio;



[System.Serializable]
[CreateAssetMenu(menuName = "오디오/사운드 매니저 세팅")]
public class SoundManagerSettingsSO : ScriptableObject
{
    [Header("오디오 믹서")]
    public AudioMixer targetAudioMixer;
    public AudioMixerGroup masterAudioMixerGroup;
    public AudioMixerGroup musicAudioMixerGroup;
    public AudioMixerGroup sfxAudioMixerGroup;
    public AudioMixerGroup uiAudioMixerGroup;
    public float mixerValuesMultiplier = 20f;

    [Header("오디오 설정")]
    public SoundManagerSettings settings;

    private const string m_saveForderName = "SoundManager/";
    private const string m_saveFileName = "sound.settings";



    #region SAVE & LOAD
    public void SaveSoundSettings()
    {

    }

    public void LoadSoundSettings()
    {

    }

    public void ResetSoundSettings()
    {

    }
    #endregion

    public void SetTrackVolume(SoundManager.SoundManagerTracks _track, float _volume)
    {
        if (_volume <= 0f)
        {
            _volume = SoundManagerSettings.minVolume;
        }

        switch (_track)
        {
            case SoundManager.SoundManagerTracks.Master:
                targetAudioMixer.SetFloat(settings.masterVolumeParameter, NormalizedToMixerVolume(_volume));
                settings.masterVolume = _volume;
                break;

            case SoundManager.SoundManagerTracks.Music:
                targetAudioMixer.SetFloat(settings.musicVolumeParameter, NormalizedToMixerVolume(_volume));
                settings.musicVolume = _volume;
                break;

            case SoundManager.SoundManagerTracks.SFX:
                targetAudioMixer.SetFloat(settings.sfxVolumeParameter, NormalizedToMixerVolume(_volume));
                settings.sfxVolume = _volume;
                break;

            case SoundManager.SoundManagerTracks.UI:
                targetAudioMixer.SetFloat(settings.uiVolumeParameter, NormalizedToMixerVolume(_volume));
                settings.uiVolume = _volume;
                break;
        }

        if (settings.autoSave)
        {
            SaveSoundSettings();
        }
    }

    public virtual float GetTrackVolume(SoundManager.SoundManagerTracks _track)
    {
        float volume = 1f;

        switch (_track)
        {
            case SoundManager.SoundManagerTracks.Master:
                targetAudioMixer.GetFloat(settings.masterVolumeParameter, out volume);
                break;
            case SoundManager.SoundManagerTracks.Music:
                targetAudioMixer.GetFloat(settings.musicVolumeParameter, out volume);
                break;
            case SoundManager.SoundManagerTracks.SFX:
                targetAudioMixer.GetFloat(settings.sfxVolumeParameter, out volume);
                break;
            case SoundManager.SoundManagerTracks.UI:
                targetAudioMixer.GetFloat(settings.uiVolumeParameter, out volume);
                break;
        }

        return MixerVolumeToNormalized(volume);
    }

    public virtual float NormalizedToMixerVolume(float _normalizedVolume)
    {
        return Mathf.Log10(_normalizedVolume) * mixerValuesMultiplier;
    }

    public virtual float MixerVolumeToNormalized(float mixerVolume)
    {
        return (float)Math.Pow(10, (mixerVolume / mixerValuesMultiplier));
    }
}
