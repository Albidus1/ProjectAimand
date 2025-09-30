using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;



[System.Serializable]
public struct SoundManagerPlayOptions
{
    public bool initailized { get; set; }

    [Header("트랙")]
    public SoundManager.SoundManagerTracks soundManagerTrack;
    public AudioMixerGroup audioMixerGroup;

    [Header("사운드")]
    public int ID;
    public bool loop;
    [Range(0f, 2f)]
    public float valume;
    [Range(-3f, 3f)]
    public float pitch;

    [Header("사운드 설정")]
    [Range(0, 256)]
    public int priority;
    public Vector3 location;
    public bool doNotAutoRecycleIfNotDonePlaying;

    [Header("Fade")]
    public bool fade;
    [MyConditionalHide("fade", true)]
    public float fadeInitialVolume;
    [MyConditionalHide("fade", true)]
    public float fadeDuration;
    [MyConditionalHide("fade", true)]
    public Ease fadeTween;

    public bool persistent;
    public AudioSource recycleAudioSource;

    [Header("시간")]
    public float playbackTime;
    public float playbackDuration;

    [Header("특수 설정")]
    [Range(0f, 1f)]
    public float specialBlend;
    public Transform attachToTransform;

    [Header("3D 사운드 세팅")]
    public AudioRolloffMode rolloffMode;
    public float minDistance;
    public float maxDistance;





    public static SoundManagerPlayOptions DefaultOption
    {
        get
        {
            SoundManagerPlayOptions defaultOption = new SoundManagerPlayOptions();

            defaultOption.initailized = true;
            defaultOption.soundManagerTrack = SoundManager.SoundManagerTracks.SFX;
            defaultOption.location = Vector3.zero;
            defaultOption.ID = 0;
            defaultOption.loop = false;
            defaultOption.valume = 1f;
            defaultOption.pitch = 1f;
            defaultOption.fade = false;
            defaultOption.fadeInitialVolume = 0f;
            defaultOption.fadeDuration = 1f;
            defaultOption.fadeTween = Ease.InCubic;
            defaultOption.persistent = false;
            defaultOption.recycleAudioSource = null;
            defaultOption.priority = 128;
            defaultOption.specialBlend = 0f;
            defaultOption.rolloffMode = AudioRolloffMode.Logarithmic;
            defaultOption.minDistance = 0f;
            defaultOption.maxDistance = 500f;

            return defaultOption;
        }
    }
}
