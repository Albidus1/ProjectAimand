using UnityEngine;



public class PlaySound : MonoBehaviour
{
    public bool active = true;

    public AudioClip soundFX;
    public bool loop;

    protected AudioSource m_audioSource;



    public void PlaySoundFX()
    {
        if (false == active)
        {
            return;
        }

        if (soundFX != null)
        {
            m_audioSource = SoundManagerSoundPlayEvent.Trigger
                (soundFX,
                SoundManager.SoundManagerTracks.SFX,
                transform.position,
                _loop:loop);
        }
    }

    public void StopSoundFX()
    {
        if (m_audioSource == null)
        {
            return;
        }

        if (loop)
        {
            m_audioSource.Stop();
        }

        SoundManager.Instance.FreeSound(m_audioSource);
    }
}
