using UnityEngine;


/// <summary>
/// 동일한 유형의 싱클톤 모두 제거
/// </summary>
public class MyPersistentHumbleSingleton<T> : MonoBehaviour where T : Component
{
    [MyReadOnly]
    public float initializationTime;
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
                    singletonObject.hideFlags = HideFlags.HideAndDontSave;
                    m_instance = singletonObject.AddComponent<T>();
                }
            }

            return m_instance;
        }
    }


    protected static T m_instance;




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

        initializationTime = Time.time;

        DontDestroyOnLoad(this.gameObject);

        T[] check = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (T searched in check)
        {
            if (searched != this)
            {
                if (searched.GetComponent<MyPersistentHumbleSingleton<T>>().initializationTime < initializationTime)
                {
                    Destroy(searched.gameObject);
                }
            }
        }

        if (m_instance == null)
        {
            m_instance = this as T;
        }
    }
}
