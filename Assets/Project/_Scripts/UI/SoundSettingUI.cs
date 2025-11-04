using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;


    private bool m_initialized = false;
    private SoundManagerSettingsSO m_settingsSO;


    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        masterVolumeSlider.maxValue = 1;
        masterVolumeSlider.minValue = 0;
        musicVolumeSlider.maxValue = 1;
        musicVolumeSlider.minValue = 0;
        sfxVolumeSlider.maxValue = 1;
        sfxVolumeSlider.minValue = 0;

        if (SoundManager.HasInstance)
        {
            m_settingsSO = SoundManager.Instance.settingsSO;

            masterVolumeSlider.value = Mathf.Clamp01(m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.Master));
            musicVolumeSlider.value = Mathf.Clamp01(m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.Music));
            sfxVolumeSlider.value = Mathf.Clamp01(m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.SFX));

            m_initialized = true;
        }
    }

    private void OnEnable()
    {
        if (false == m_initialized)
        {
            Initialization();
        }
    }

    private void Update()
    {
        if (this.gameObject.activeSelf)
        {
            m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.Master, masterVolumeSlider.value);
            m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.Music, musicVolumeSlider.value);
            m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.SFX, sfxVolumeSlider.value);
        }
    }
}
