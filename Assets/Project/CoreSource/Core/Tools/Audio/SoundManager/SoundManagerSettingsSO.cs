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
        MySaveLoadManager.Save(settings, m_saveFileName, m_saveForderName);
    }

    public void LoadSoundSettings()
    {
        SoundManagerSettings settings =
            (SoundManagerSettings)MySaveLoadManager.Load(typeof(SoundManagerSettings), m_saveFileName, m_saveForderName);


        if (settings != null)
        {
            this.settings = settings;
            ApplyTrackVolumes();
        }

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

    public virtual void GetTrackVolumes()
    {
        settings.masterVolume = GetTrackVolume(SoundManager.SoundManagerTracks.Master);
        settings.musicVolume = GetTrackVolume(SoundManager.SoundManagerTracks.Music);
        settings.sfxVolume = GetTrackVolume(SoundManager.SoundManagerTracks.SFX);
        settings.uiVolume = GetTrackVolume(SoundManager.SoundManagerTracks.UI);
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

    protected virtual void ApplyTrackVolumes()
    {
        targetAudioMixer.SetFloat(settings.masterVolumeParameter, NormalizedToMixerVolume(settings.masterVolume));
        targetAudioMixer.SetFloat(settings.musicVolumeParameter, NormalizedToMixerVolume(settings.musicVolume));
        targetAudioMixer.SetFloat(settings.sfxVolumeParameter, NormalizedToMixerVolume(settings.sfxVolume));
        targetAudioMixer.SetFloat(settings.uiVolumeParameter, NormalizedToMixerVolume(settings.uiVolume));

        if (!settings.mastarOn) 
        { 
            targetAudioMixer.SetFloat(settings.masterVolumeParameter, -80f); 
        }
        if (!settings.musicOn) 
        { 
            targetAudioMixer.SetFloat(settings.musicVolumeParameter, -80f);
        }
        if (!settings.sfxOn) 
        { 
            targetAudioMixer.SetFloat(settings.sfxVolumeParameter, -80f);
        }
        if (!settings.uiOn) 
        { 
            targetAudioMixer.SetFloat(settings.uiVolumeParameter, -80f); 
        }

        if (settings.autoSave)
        {
            SaveSoundSettings();
        }
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
