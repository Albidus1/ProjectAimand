using UnityEngine;



[System.Serializable]
public struct SoundManagerPlayOptions
{
    public bool initailized { get; set; }

    [Header("트랙")]
    public SoundManager.SoundManagerTracks soundManagerTrack;
}
