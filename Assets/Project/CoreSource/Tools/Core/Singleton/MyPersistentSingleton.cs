using UnityEngine;



public class MyPersistentSingleton<T> : MonoBehaviour where T : Component 
{
    public static bool HasInstance => m_instance != null;
    public static T current => m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindFirstObjectByType<T>();
                if (m_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name + "_AutoCreated");
                    m_instance = singletonObject.AddComponent<T>();
                }
            }

            return m_instance;
        }
    }


    protected static T m_instance;
    protected bool m_enabled;




    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton()
    {
        if (false == Application.isPlaying)
        {
            return;
        }

        transform.SetParent(null);

        if (m_instance == null)
        {
            m_instance = this as T;
            DontDestroyOnLoad(transform.gameObject);
            m_enabled = true;
        }
        else if (this != m_instance)
        {
            Destroy(this.gameObject);
        }
    }
}
