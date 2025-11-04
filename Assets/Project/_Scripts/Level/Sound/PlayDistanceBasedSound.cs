using System.Collections.Generic;
using UnityEngine;

public class PlayDistanceBasedSound : MonoBehaviour
{
    public bool active = true;
    public bool autoPlay = false;

    public AudioClip soundFX;

    public int id;
    public bool loop;

    [Header("사운드 설정")]
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 1f)]
    public float pitch = 1f;

    [Header("거리 설정")]
    public AudioRolloffMode rolloffMode;
    public float minDistance = 8f;
    public float maxDistance = 24f;
    public float extraRange = 5f;

    [Header("특수 설정")]
    public Transform attachToTransform;

    protected AudioSource m_audioSource;
    protected CircleCollider2D m_circleCollider;



    private void Start()
    {
        m_circleCollider = GetComponent<CircleCollider2D>();
        if (m_circleCollider != null)
        {
            m_circleCollider.radius = maxDistance + extraRange;
            m_circleCollider.isTrigger = true;
        }
    }

    [ContextMenu("사운드 출력")]
    public void PlaySoundFX()
    {
        if (false == active)
        {
            return;
        }

        if (soundFX != null)
        {
            if (MyDebug.CircleCast(transform.position, maxDistance + extraRange, Vector2.zero, 0f, LayerManager.playerLayerMask, MyColors.Violet))
            {
                m_audioSource = SoundManagerSoundPlayEvent.Trigger
                  (soundFX,
                  SoundManager.SoundManagerTracks.SFX,
                  transform.position,
                  _ID: id,
                  _loop: loop,
                  _volume: volume,
                  _pitch: pitch,
                  _specialBlend: 1f,
                  _rolloffMode: rolloffMode,
                  _minDistance: minDistance,
                  _maxDistance: maxDistance,
                  _attachToTransform: attachToTransform);
            }
        }
    }

    [ContextMenu("사운드 종료")]
    public void StopSoundFX()
    {
        if (m_audioSource == null)
        {
            return;
        }

        m_audioSource.Stop();
        SoundManager.Instance.FreeSound(m_audioSource);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerMovement>(out _) && autoPlay)
        {
            PlaySoundFX();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerMovement>(out _))
        {
            StopSoundFX();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = MyColors.Violet;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
#endif
}
