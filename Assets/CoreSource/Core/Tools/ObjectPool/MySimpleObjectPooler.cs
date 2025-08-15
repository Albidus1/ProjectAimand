using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySimpleObjectPooler : MyObjectPooler
{
    public GameObject gameObjectToPool;
    public int poolSize = 20;
    public bool poolCanExpand = true;

    public virtual List<MySimpleObjectPooler> owner { get; set; }



    public override void FillObjectPool()
    {
        if (gameObjectToPool == null)
        {
            return;
        }

        if (m_objectPool != null && m_objectPool.pooledObjects.Count > poolSize)
        {
            return;
        }

        CreateWaitingPool();

        int objectsToCreate = poolSize;

        if (m_objectPool != null)
        {
            objectsToCreate -= m_objectPool.pooledObjects.Count;
        }

        for (int i = 0; i < objectsToCreate; i++)
        {
            AddOneObjectToThePool();
        }
    }

    protected override string DetermineObjectPoolName()
    {
        string poolName = $"[{gameObjectToPool.name}_SimpleObjectPooler]";

        return poolName;
    }

    public override GameObject GetPooledGameObject()
    {
        for (int i = 0; i < m_objectPool.pooledObjects.Count; i++)
        {
            if (false == m_objectPool.pooledObjects[i].gameObject.activeInHierarchy)
            {
                return m_objectPool.pooledObjects[i];
            }
        }

        if (poolCanExpand)
        {
            return AddOneObjectToThePool();
        }

        return null;
    }

    private GameObject AddOneObjectToThePool()
    {
        if (gameObjectToPool == null)
        {
            return null;
        }

        bool initialStatus = gameObjectToPool.activeSelf;

        gameObjectToPool.SetActive(false);
        GameObject newObject = Instantiate(gameObjectToPool);
        gameObjectToPool.SetActive(initialStatus);
        
        SceneManager.MoveGameObjectToScene(newObject, gameObject.scene);

        if (nestWaitingPool)
        {
            newObject.transform.SetParent(m_waitingPool.transform);
        }

        newObject.name = gameObjectToPool.name + " " + m_objectPool.pooledObjects.Count;

        m_objectPool.pooledObjects.Add(newObject);

        return newObject;
    }
}
