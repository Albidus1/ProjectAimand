using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MySingleton<SFXManager>
{
    protected List<AudioSource> sfxAudioSources = new List<AudioSource>();

    protected override void Awake()
    {
        base.Awake();
    }
}
