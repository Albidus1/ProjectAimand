using UnityEngine;



[System.Serializable]
public class SoundManagerSettings
{
    public const float minVolume = 0.0001f;
    public const float maxVolume = 10f;
    public const float defaultVolume = 1f;

    [Header("오디오 믹서 컨트롤")]
    public string masterVolumeParameter = "MasterVolume";
    public string musicVolumeParameter = "MusicVolume";
    public string sfxVolumeParameter = "SFXVolume";
    public string uiVolumeParameter = "UIVolume";

    [Header("마스터 볼륨")]
    [Range(minVolume, maxVolume)]
    public float masterVolume = defaultVolume;
    [MyReadOnly]
    public bool mastarOn = true;
    [MyReadOnly]
    public float mutedMasterVolume;

    [Header("뮤직 볼륨")]
    [Range(minVolume, maxVolume)]
    public float musicVolume = defaultVolume;
    [MyReadOnly]
    public bool musicOn = true;
    [MyReadOnly]
    public float mutedMusicVolume;

    [Header("사운드 이펙트")]
    [Range(minVolume, maxVolume)]
    public float sfxVolume = defaultVolume;
    [MyReadOnly]
    public bool sfxOn = true;
    [MyReadOnly]
    public float mutedSFXVolume;

    [Header("UI 볼륨")]
    [Range(minVolume, maxVolume)]
    public float uiVolume = defaultVolume;
    [MyReadOnly]
    public bool uiOn = true;
    [MyReadOnly]
    public float mutedUIVolume;

    [Header("세이브 로드")]
    public bool autoLoad = true;
    public bool autoSave = true;
}
