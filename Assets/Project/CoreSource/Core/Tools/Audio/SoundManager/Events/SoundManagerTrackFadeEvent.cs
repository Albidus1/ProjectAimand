using DG.Tweening;
using UnityEngine;




public struct SoundManagerTrackFadeEvent
{
    static SoundManagerTrackFadeEvent e;

    public enum Modes { PlayFade, StopFade };

    public Modes mode;
    public SoundManager.SoundManagerTracks track;
    public float fadeDuration;
    public float finalVolume;
    public Ease ease;



    public SoundManagerTrackFadeEvent(Modes _mode, SoundManager.SoundManagerTracks _track, float _fadeDuration, float _finalVolume, Ease _ease)
    {
        mode = _mode;
        track = _track;
        fadeDuration = _fadeDuration;
        finalVolume = _finalVolume;
        ease = _ease;
    }

    public static void Trigger(Modes _mode, SoundManager.SoundManagerTracks _track, float _fadeDuration, float _finalVolume, Ease _ease)
    {
        e.mode = _mode;
        e.track = _track;
        e.fadeDuration = _fadeDuration;
        e.finalVolume = _finalVolume;
        e.ease = _ease;

        EventManager.TriggerEvent(e);
    }
}
