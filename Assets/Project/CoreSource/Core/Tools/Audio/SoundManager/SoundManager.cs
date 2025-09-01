using UnityEngine;




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

}
