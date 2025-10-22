using UnityEngine;



public class BackgroundMusic : MyPersistentHumbleSingleton<BackgroundMusic>
{
    public AudioClip soundClip;
    public bool loop = true;
    public int ID = 255;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 1f)]
    public float pitch = 1f;

    protected AudioSource m_audioSource;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void InitializeStatics()
    {
        m_instance = null;
    }

    private void Start()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        SoundManagerPlayOptions options = SoundManagerPlayOptions.DefaultOption;
        
        options.ID = ID;
        options.loop = loop;
        options.location = Vector3.zero;
        options.soundManagerTrack = SoundManager.SoundManagerTracks.Music;
        options.valume = volume;
        options.pitch = pitch;

        m_audioSource = SoundManagerSoundPlayEvent.Trigger(soundClip, options);
    }
}
