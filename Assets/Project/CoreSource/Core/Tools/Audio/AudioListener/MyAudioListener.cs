using UnityEngine;



[RequireComponent(typeof(AudioListener))]
public class MyAudioListener : MonoBehaviour
{
    protected AudioListener m_audioListener;
    protected AudioListener[] m_otherListeners;



    protected virtual void OnEnable()
    {
        m_audioListener = this.gameObject.GetComponent<AudioListener>();
        m_otherListeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None) as AudioListener[];

        foreach (AudioListener listener in m_otherListeners )
        {
            if (listener != null && listener != m_audioListener)
            {
                listener.enabled = false;
            }
        }
    }
}
