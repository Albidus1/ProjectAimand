using UnityEngine;




public class BackgroundMusic : MonoBehaviour
{
    public AudioClip soundClip;
    public bool loop = true;
    public int ID = 255;



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

        SoundManagerSoundPlayEvent.Trigger(soundClip, options);
    }
}
