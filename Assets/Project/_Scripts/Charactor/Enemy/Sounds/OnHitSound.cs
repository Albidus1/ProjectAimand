using System.Collections;
using UnityEngine;

public class OnHitSound : MonoBehaviour
{
    public AudioClip[] soundClips;
    public int ID = 100;

    private Health m_health;
    private AudioSource m_source;

    private void Awake()
    {
        m_health = GetComponent<Health>();
    }

    public void OnHit()
    {
        SoundManagerPlayOptions options = SoundManagerPlayOptions.DefaultOption;

        options.ID = ID;
        options.location = Vector3.zero;
        options.soundManagerTrack = SoundManager.SoundManagerTracks.SFX;
        options.loop = false;

        m_source = SoundManagerSoundPlayEvent.Trigger(GetRandomSound(), options);
    }

    private AudioClip GetRandomSound()
    {
        int index = Random.Range(0, soundClips.Length);

        return soundClips[index];
    }

    private void OnEnable()
    {
        if (m_health == null)
        {
            m_health = GetComponent<Health>();
        }

        m_health.OnHit += OnHit;
    }

    private void OnDisable()
    {
        if (m_health != null)
        {
            m_health.OnHit -= OnHit;
        }
    }
}
