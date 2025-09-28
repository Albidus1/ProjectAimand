using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public abstract class MyObjectPooler : MonoBehaviour
{
    public static MyObjectPooler Instance;
    /// 동일 이름의 대기 풀이 있으면 새 풀을 생성하지 않음
    public bool mutualizeWaitingPools = false;
    /// 모든 대기 / 활성 오브젝트를 빈 게임 오브젝트 하위로 그룹화
    public bool nestWaitingPool = true;
    /// 대기 풀이 이 오브젝트 하위에 중첩
    [MyConditionalHide("nestWaitingPool", true)]
    public bool nestUnderThisObject = false;

    protected GameObject m_waitingPool = null;
    protected MyObjectPool m_objectPool;
    protected const int m_initialPoolsListCapacity = 5;
    protected bool m_onSceneLoadedRegistered = false;

    public static List<MyObjectPool> pools = new List<MyObjectPool>(m_initialPoolsListCapacity);



    protected virtual void Awake()
    {
        Instance = this;
        FillObjectPool();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void InitializeStatics()
    {
        Instance = null;
    }

    public static void AddPool(MyObjectPool _pool)
    {
        if (pools == null)
        {
            pools = new List<MyObjectPool>(m_initialPoolsListCapacity);
        }
        if (false == pools.Contains(_pool))
        {
            pools.Add(_pool);
        }
    }

    public static void RemovePool(MyObjectPool _pool)
    {
        if (pools != null)
        {
            pools.Remove(_pool);
        }
    }

    protected virtual bool CreateWaitingPool()
    {
        if (false == mutualizeWaitingPools)
        {
            m_waitingPool = new GameObject(DetermineObjectPoolName());
            SceneManager.MoveGameObjectToScene(m_waitingPool, gameObject.scene);
            m_objectPool = m_waitingPool.AddComponent<MyObjectPool>();
            m_objectPool.pooledObjects = new List<GameObject>();

            ApplyNesting();

            return true;
        }
        else
        {
            MyObjectPool objectPool = ExistingPool(DetermineObjectPoolName());
            if (objectPool != null)
            {
                m_objectPool = objectPool;
                m_waitingPool = m_objectPool.gameObject;

                return false;
            }
            else
            {
                m_waitingPool = new GameObject(DetermineObjectPoolName());
                SceneManager.MoveGameObjectToScene(m_waitingPool, gameObject.scene);
                m_objectPool = m_waitingPool.AddComponent<MyObjectPool>();
                m_objectPool.pooledObjects = new List<GameObject>();

                ApplyNesting();
                AddPool(m_objectPool);

                return true;
            }
        }
    }

    public virtual MyObjectPool ExistingPool(string _poolName)
    {
        if (pools == null)
        {
            pools = new List<MyObjectPool>(m_initialPoolsListCapacity);
        }
        if (pools.Count == 0)
        {
            var pools = FindObjectsByType<MyObjectPool>(FindObjectsInactive.Exclude ,FindObjectsSortMode.None);

            if (pools.Length > 0)
            {
                pools.AddRange(pools);
            }
        }

        foreach (MyObjectPool pool in pools)
        {
            if (pool != null && pool.name == _poolName)
            {
                return pool;
            }
        }

        return null;
    }

    protected virtual void ApplyNesting()
    {
        if (nestWaitingPool && nestUnderThisObject && m_waitingPool != null)
        {
            m_waitingPool.transform.SetParent(this.transform);
        }
    }

    protected virtual string DetermineObjectPoolName()
    {
        string poolName = $"[{this.name}_ObjectPooler]";

        return poolName;
    }

    public virtual void FillObjectPool()
    {

    }

    public virtual GameObject GetPooledGameObject()
    {
        return null;
    }

    public virtual void DeactivateAllPooledGameObject()
    {

    }

    public virtual void DestroyObjectPool()
    {
        if (m_waitingPool != null)
        {
            Destroy(m_waitingPool.gameObject);
        }
    }

    protected virtual void OnEnable()
    {
        if (m_onSceneLoadedRegistered == false)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
    {
        if (this == null)
        {
            return;
        }
        if (m_objectPool == null || m_waitingPool == null)
        {
            if (this != null)
            {
                FillObjectPool();
            }
        }
    }

    private void OnDestroy()
    {
        if (m_objectPool != null && nestUnderThisObject)
        {
            RemovePool(m_objectPool);
        }

        if (m_onSceneLoadedRegistered)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            m_onSceneLoadedRegistered = false;
        }
    }
}
