using System;
using UnityEditor;
using UnityEngine;




#if UNITY_EDITOR
[CustomEditor(typeof(SoundManager), true)]
[CanEditMultipleObjects]
public class SoundManagerEditor : Editor
{
    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    protected SoundManagerSettingsSO m_settingsSO;
    protected SoundManager m_soundManager;

    private static float m_masterVolume, m_musicVolume, m_sfxVolume, m_uiVolume;

    protected Color m_originalBackgroundColor;

    protected Color m_saveButtonColor = new Color32(80, 80, 80, 255);
    protected Color m_loadButtonColor = new Color32(107, 107, 107, 255);
    protected Color m_resetButtonColor = new Color32(120, 120, 120, 255);

    protected Color m_baseColor = new Color32(150, 150, 150, 255);

    protected Color m_masterColorBase = MyColors.ReunoYellow;
    protected Color m_masterColorMute;
    protected Color m_masterColorUnmute;
    protected Color m_masterColorPause;
    protected Color m_masterColorStop;
    protected Color m_masterColorPlay;
    protected Color m_masterColorFree;
                    
    protected Color m_musicColorBase = MyColors.Aquamarine;
    protected Color m_musicColorMute;
    protected Color m_musicColorUnmute;
    protected Color m_musicColorPause;
    protected Color m_musicColorStop;
    protected Color m_musicColorPlay;
    protected Color m_musicColorFree;
                    
    protected Color m_sfxColorBase = MyColors.Coral;
    protected Color m_sfxColorMute;
    protected Color m_sfxColorUnmute;
    protected Color m_sfxColorPause;
    protected Color m_sfxColorStop;
    protected Color m_sfxColorPlay;
    protected Color m_sfxColorFree;
                    
    protected Color m_uiColorBase = MyColors.SteelBlue;
    protected Color m_uiColorMute;
    protected Color m_uiColorUnmute;
    protected Color m_uiColorPause;
    protected Color m_uiColorStop;
    protected Color m_uiColorPlay;
    protected Color m_uiColorFree;

    protected MyColors.ColoringMode m_coloringMode = MyColors.ColoringMode.Add;



    protected virtual void OnEnable()
    {
        m_masterColorMute   = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 1f);
        m_masterColorUnmute = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 0.9f);
        m_masterColorPause  = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 0.8f);
        m_masterColorStop   = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 0.7f);
        m_masterColorPlay   = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 0.5f);
        m_masterColorFree   = MyColors.MMColorize(m_baseColor, m_masterColorBase, m_coloringMode, 0.4f);

        m_musicColorMute    = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 1f);
        m_musicColorUnmute  = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 0.9f);
        m_musicColorPause   = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 0.8f);
        m_musicColorStop    = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 0.7f);
        m_musicColorPlay    = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 0.5f);
        m_musicColorFree    = MyColors.MMColorize(m_baseColor, m_musicColorBase, m_coloringMode, 0.4f);

        m_sfxColorMute      = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 1f);
        m_sfxColorUnmute    = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 0.9f);
        m_sfxColorPause     = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 0.8f);
        m_sfxColorStop      = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 0.7f);
        m_sfxColorPlay      = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 0.5f);
        m_sfxColorFree      = MyColors.MMColorize(m_baseColor, m_sfxColorBase, m_coloringMode, 0.4f);

        m_uiColorMute       = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 1f);
        m_uiColorUnmute     = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 0.9f);
        m_uiColorPause      = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 0.8f);
        m_uiColorStop       = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 0.7f);
        m_uiColorPlay       = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 0.5f);
        m_uiColorFree       = MyColors.MMColorize(m_baseColor, m_uiColorBase, m_coloringMode, 0.4f);
    }

    public override void OnInspectorGUI()
    {
        m_settingsSO = (target as SoundManager).settingsSO;
        m_soundManager = target as SoundManager;

        if (m_settingsSO != null)
        {
            m_masterVolume = m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.Master);
            m_musicVolume = m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.Music);
            m_sfxVolume = m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.SFX);
            m_uiVolume = m_settingsSO.GetTrackVolume(SoundManager.SoundManagerTracks.UI);
        }

        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        if (m_settingsSO != null && m_soundManager.gameObject.activeInHierarchy)
        {
            DrawTrack("마스터", m_soundManager.settingsSO.settings.mastarOn, SoundManager.SoundManagerTracks.Master, 
                m_masterColorMute, m_masterColorUnmute, m_masterColorPause, m_masterColorStop, m_masterColorPlay, m_masterColorFree);
            DrawTrack("뮤직", m_soundManager.settingsSO.settings.musicOn, SoundManager.SoundManagerTracks.Music,
                m_musicColorMute, m_musicColorUnmute, m_musicColorPause, m_musicColorStop, m_musicColorPlay, m_musicColorFree);
            DrawTrack("SFX", m_soundManager.settingsSO.settings.sfxOn, SoundManager.SoundManagerTracks.SFX,
                m_sfxColorMute, m_sfxColorUnmute, m_sfxColorPause, m_sfxColorStop, m_sfxColorPlay, m_sfxColorFree);
            DrawTrack("UI", m_soundManager.settingsSO.settings.uiOn, SoundManager.SoundManagerTracks.UI,
                m_uiColorMute, m_uiColorUnmute, m_uiColorPause, m_uiColorStop, m_uiColorPlay, m_uiColorFree);
        }
    }

    private void DrawTrack(string _title, bool _mute, SoundManager.SoundManagerTracks _track, 
        Color _muteColor, Color _unmuteColor, Color _pauseColor, Color _stopColor, Color _playColor, Color _freeColor)
    {
        GUILayout.Space(10);
        GUILayout.Label(_title, EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(false == Application.isPlaying);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("볼륨");

        float newVolume = 0f;
        switch (_track)
        {
            case SoundManager.SoundManagerTracks.Master:
                newVolume = EditorGUILayout.Slider(m_masterVolume, SoundManagerSettings.minVolume, SoundManagerSettings.maxVolume);
                if (newVolume != m_masterVolume)
                {
                    m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.Master, newVolume);
                }
                break;
            case SoundManager.SoundManagerTracks.Music:
                newVolume = EditorGUILayout.Slider(m_musicVolume, SoundManagerSettings.minVolume, SoundManagerSettings.maxVolume);
                if (newVolume != m_musicVolume)
                {
                    m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.Music, newVolume);
                }
                break;
            case SoundManager.SoundManagerTracks.SFX:
                newVolume = EditorGUILayout.Slider(m_sfxVolume, SoundManagerSettings.minVolume, SoundManagerSettings.maxVolume);
                if (newVolume != m_sfxVolume)
                {
                    m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.SFX, newVolume);
                }
                break;
            case SoundManager.SoundManagerTracks.UI:
                newVolume = EditorGUILayout.Slider(m_uiVolume, SoundManagerSettings.minVolume, SoundManagerSettings.maxVolume);
                if (newVolume != m_uiVolume)
                {
                    m_settingsSO.SetTrackVolume(SoundManager.SoundManagerTracks.UI, newVolume);
                }
                break;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            if (_mute)
            {
                DrawColoredButton("Mute", _muteColor, _track, m_soundManager.MuteTrack, EditorStyles.miniButtonLeft);
            }
            else
            {
                DrawColoredButton("Unmute", _unmuteColor, _track, m_soundManager.UnmuteTrack, EditorStyles.miniButtonMid);
            }

            DrawColoredButton("Pause", _pauseColor, _track, m_soundManager.PauseTrack, EditorStyles.miniButtonMid);
            DrawColoredButton("Stop", _stopColor, _track, m_soundManager.StopTrack, EditorStyles.miniButtonMid);
            DrawColoredButton("Play", _playColor, _track, m_soundManager.PlayTrack, EditorStyles.miniButtonMid);
            DrawColoredButton("Free", _freeColor, _track, m_soundManager.FreeTrack, EditorStyles.miniButtonRight);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();
    }

    private void DrawColoredButton(string _buttonLabel, Color _buttonColor, SoundManager.SoundManagerTracks _track,
        Action<SoundManager.SoundManagerTracks> _action, GUIStyle _styles)
    {
        m_originalBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = _buttonColor;

        if (GUILayout.Button(_buttonLabel, _styles))
        {
            _action?.Invoke(_track);
        }

        GUI.backgroundColor = m_originalBackgroundColor;
    }
}
#endif