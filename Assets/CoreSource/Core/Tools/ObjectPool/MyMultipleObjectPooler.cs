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
    public List<MyMultipleObjectPoolerObject> pool;
    public bool canPoolSameObjectTwice = true;
    [MyConditionalHide(nameof(base.mutualizeWaitingPools), true)]
    public string mutualizedPoolName = "";

    public virtual List<MyMultipleObjectPoolerObject> owner { get; set; }

    protected GameObject m_lastPooledObject;
    protected int m_currentIndex = 0;
    //protected int m_currentIndexCounter = 0;
    protected int m_currentCount = 0;


    protected override string DetermineObjectPoolName()
    {
        if (mutualizedPoolName == null || mutualizedPoolName == "")
        {
            return ("[MultipleObjectPooler] " + this.name);
        }
        else
        {
            return ("[MultipleObjectPooler] " + mutualizedPoolName);
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

        foreach (var pooledGameObject in pool)
        {
            for (int i = 0; i < pooledGameObject.poolSize; i++)
            {
                AddOneObjectToThePool(pooledGameObject.gameObjectToPool);
            }
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

        SceneManager.MoveGameObjectToScene(_object, gameObject.scene);

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
                ResetCounter(searchedObject);

                return findObject;
            }

            if (searchedObject.poolCanExpand)
            {
                m_currentCount++;
                ResetCounter(searchedObject);

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
            ResetCounter(searchedObject);

            return m_objectPool.pooledObjects[m_currentIndex];
        }
    }

    private void ResetCounter(MyMultipleObjectPoolerObject _searchedObject)
    {
        if (m_currentCount >= _searchedObject.poolSize)
        {
            m_currentIndex++;
            m_currentCount = 0;
        }
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
        //m_currentIndexCounter = 0;
    }
}
