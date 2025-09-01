using UnityEngine;
using UnityEngine.Audio;



[System.Serializable]
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
}
