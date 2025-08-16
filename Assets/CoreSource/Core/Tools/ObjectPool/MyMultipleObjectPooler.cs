using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class MyMultipleObjectPoolerObject
{
    public GameObject gameObjectToPool;
    public int poolSize = 10;
    public bool poolCanExpand = true;
    public bool enabled = true;
}


public class MyMultipleObjectPooler : MyObjectPooler
{
    public enum PoolingMode { OriginalOrder, OriginalOrderSequential }
    [Tooltip
        ("- OriginalOrder: 풀링된 오브젝트를 인스펙터에서 설정한 순서대로 가져옴 (위에서 아래로)\n" +
         "- OriginalOrderSequential: 동일한 순서지만 각 풀을 완전히 소진 후 다음 오브젝트로 이동\n")]
    public PoolingMode poolingMode = PoolingMode.OriginalOrder;

    public List<MyMultipleObjectPoolerObject> pool;
    public bool canPoolSameObjectTwice = true;
    [MyConditionalHide(nameof(base.mutualizeWaitingPools), true)]
    public string mutualizedPoolName = "";

    public virtual List<MyMultipleObjectPoolerObject> owner { get; set; }

    protected GameObject m_lastPooledObject;
    protected int m_currentIndex = 0;
    protected int m_currentIndexCounter = 0;
    protected int m_currentCount = 0;


    protected override string DetermineObjectPoolName()
    {
        if (mutualizedPoolName == null || mutualizedPoolName == "")
        {
            return ($"[{this.name}_MultipleObjectPooler]");
        }
        else
        {
            return ($"[{mutualizedPoolName}_MultipleObjectPooler]");
        }
    }

    public override void FillObjectPool()
    {
        if (pool == null || pool.Count == 0)
        {
            return;
        }

        if (false == CreateWaitingPool())
        {
            return;
        }

        if (pool.Count <= 1)
        {
            canPoolSameObjectTwice = true;
        }

        bool stillObjectsToPool;
        int[] poolSizes;

        switch (poolingMode)
        {
            case PoolingMode.OriginalOrder:
                stillObjectsToPool = true;
                poolSizes = new int[pool.Count];
                for (int i = 0; i < pool.Count; i++)
                {
                    poolSizes[i] = pool[i].poolSize;
                }

                while (stillObjectsToPool)
                {
                    stillObjectsToPool = false;
                    for (int i = 0; i < pool.Count; i++)
                    {
                        if (poolSizes[i] > 0)
                        {
                            AddOneObjectToThePool(pool[i].gameObjectToPool);
                            poolSizes[i]--;
                            stillObjectsToPool = true;
                        }
                    }
                }
                break;

            case PoolingMode.OriginalOrderSequential:
                foreach (var pooledGameObject in pool)
                {
                    for (int i = 0; i < pooledGameObject.poolSize; i++)
                    {
                        AddOneObjectToThePool(pooledGameObject.gameObjectToPool);
                    }
                }
                break;

            default:
                break;

        }
    }

    protected virtual GameObject AddOneObjectToThePool(GameObject _object)
    {
        if (_object == null)
        {
            return null;
        }

        bool initialStatus = _object.activeSelf;
        _object.SetActive(false);

        GameObject newGameObject = Instantiate(_object);
        _object.SetActive(initialStatus);

        SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);

        if (base.nestWaitingPool)
        {
            newGameObject.transform.SetParent(base.m_waitingPool.transform);
        }

        newGameObject.name = _object.name;
        m_objectPool.pooledObjects.Add(newGameObject);

        return newGameObject;
    }

    public override GameObject GetPooledGameObject()
    {
        GameObject pooledGameObject = poolingMode switch
        {
            PoolingMode.OriginalOrder => GetPooledGameObjectOriginalOrder(),
            PoolingMode.OriginalOrderSequential => GetPooledGameObjectOriginalOrderQequential(),
            _ => null,
        };

        if (pooledGameObject != null)
        {
            m_lastPooledObject = pooledGameObject;
        }
        else
        {
            m_lastPooledObject = null;
        }

        return pooledGameObject;
    }

    public GameObject GetPooledGameObjectAtIndex(int _index)
    {
        if (_index < 0 || _index >= pool.Count)
        {
            return null;
        }

        return GetPooledGameObjectOfName(pool[_index].gameObjectToPool.name);
    }

    public GameObject GetPooledGameObjectOfName(string _searchName)
    {
        GameObject newObject = FindInactiveObject(_searchName, m_objectPool.pooledObjects);

        if (newObject != null)
        {
            return newObject;
        }
        else
        {
            GameObject searchedObject = FindObject(_searchName, m_objectPool.pooledObjects);

            if (searchedObject == null)
            {
                return null;
            }

            if (GetPoolObject(FindObject(_searchName, m_objectPool.pooledObjects)).poolCanExpand)
            {
                return AddOneObjectToThePool(searchedObject);
            }
        }

        return null;
    }

    #region ORIGINAL ORDER
    private GameObject GetPooledGameObjectOriginalOrder()
    {
        int newIndex;

        if (m_currentIndex >= pool.Count)
        {
            ResetCurrentIndex();
        }

        MyMultipleObjectPoolerObject searchedObject = GetPoolObject(pool[m_currentIndex].gameObjectToPool);

        if (m_currentIndex >= m_objectPool.pooledObjects.Count)
        {
            return null;
        }
        if (false == searchedObject.enabled)
        {
            m_currentIndex++;
            return null;
        }

        if (m_objectPool.pooledObjects[m_currentIndex].gameObject.activeInHierarchy)
        {
            GameObject findObject = FindInactiveObject(m_objectPool.pooledObjects[m_currentIndex].gameObject.name, m_objectPool.pooledObjects);
            if (findObject != null)
            {
                m_currentIndex++;
                return findObject;
            }

            if (searchedObject.poolCanExpand)
            {
                m_currentIndex++;
                return AddOneObjectToThePool(searchedObject.gameObjectToPool);
            }
            else
            {
                m_currentIndex++;
                return null;
            }
        }
        else
        {
            newIndex = m_currentIndex;
            m_currentIndex++;

            return m_objectPool.pooledObjects[newIndex];
        }
    }
    #endregion

    #region ORIGINAL ORDER SEQUENTIAL
    private GameObject GetPooledGameObjectOriginalOrderQequential()
    {
        if (m_currentIndex >= pool.Count)
        {
            m_currentCount = 0;
            ResetCurrentIndex();
        }

        MyMultipleObjectPoolerObject searchedObject = GetPoolObject(pool[m_currentIndex].gameObjectToPool);

        if (m_currentIndex >= m_objectPool.pooledObjects.Count)
        {
            return null;
        }
        if (false == searchedObject.enabled)
        {
            m_currentIndex++;
            m_currentCount = 0;

            return null;
        }

        if (m_objectPool.pooledObjects[m_currentIndex].gameObject.activeInHierarchy)
        {
            GameObject findObject = FindInactiveObject(pool[m_currentIndex].gameObjectToPool.name, m_objectPool.pooledObjects);

            if (findObject != null)
            {
                m_currentCount++;
                OrderSequentialResetCounter(searchedObject);

                return findObject;
            }

            if (searchedObject.poolCanExpand)
            {
                m_currentCount++;
                OrderSequentialResetCounter(searchedObject);

                return AddOneObjectToThePool(searchedObject.gameObjectToPool);
            }
            else
            {
                m_currentIndex++;
                m_currentCount = 0;

                return null;
            }
        }
        else
        {
            m_currentCount++;
            OrderSequentialResetCounter(searchedObject);

            return m_objectPool.pooledObjects[m_currentIndex];
        }
    }

    private void OrderSequentialResetCounter(MyMultipleObjectPoolerObject _searchedObject)
    {
        if (m_currentCount >= _searchedObject.poolSize)
        {
            m_currentIndex++;
            m_currentCount = 0;
        }
    }
    #endregion

    private GameObject FindObject(string _name, List<GameObject> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i].name.Equals(_name))
            {
                return _list[i];
            }
        }

        return null;
    }


    private GameObject FindInactiveObject(string _name, List<GameObject> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i].name.Equals(_name) && false == _list[i].gameObject.activeInHierarchy)
            {
                return _list[i];
            }
        }

        return null;
    }

    private MyMultipleObjectPoolerObject GetPoolObject(GameObject _object)
    {
        if (_object == null)
        {
            return null;
        }

        foreach (var poolerObject in pool)
        {
            if (_object.name.Equals(poolerObject.gameObjectToPool.name))
            {
                return poolerObject;
            }
        }

        return null;
    }

    public void ResetCurrentIndex()
    {
        m_currentIndex = 0;
        m_currentIndexCounter = 0;
    }
}
