using UnityEngine;



public class MySingleton<T> : MonoBehaviour where T : Component
{
    protected static T m_instance;
    public static bool HasInstance => m_instance != null;
    public static T TryGetInstance() => HasInstance ? m_instance : null;
    public static T Current => m_instance;

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindFirstObjectByType<T>();

                if (m_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name + "_AutoCreated";
                    m_instance = obj.AddComponent<T>();
                }
            }

            return m_instance;
        }
    }

    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        m_instance = this as T;
    }
}