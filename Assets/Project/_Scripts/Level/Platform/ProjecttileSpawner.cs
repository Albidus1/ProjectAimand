using System;
using UnityEngine;

public class ProjecttileSpawner : MonoBehaviour
{
    public Transform projecttileSpawnTransform;
    public Vector3 projecttileSpawnOffset = Vector3.zero;
    public int projecttilePerShot = 1;

    public MyObjectPooler objectPooler;

    [MyReadOnly]
    public Vector3 spawnPosition = Vector3.zero;


    private bool m_poolInitialized = false;
    private Vector3 m_spawnPositionCenter;
    private Vector2 m_offset;



    private void Awake()
    {
        Initialization();
    }
    public void Initialization()
    {
        if (false == m_poolInitialized)
        {
            if (objectPooler == null)
            {
                objectPooler = GetComponent<MyObjectPooler>();
            }
            if (objectPooler == null)
            {
                Debug.LogError(this.name + "오브젝트 풀러 없음");
                return;
            }

            m_poolInitialized = true;
        }
    }

    [ContextMenu("UseSpawner")]
    private void UseSpawner()
    {
        DetermineSpawnPosition();

        for (int i = 0; i < projecttilePerShot; i++)
        {
            SpawnProjectile(spawnPosition, i, projecttilePerShot);
        }
    }

    public GameObject SpawnProjectile(Vector3 _spawnPosition, int _projectileIndex, int _totalProjectiles)
    {
        GameObject nextGameObject = objectPooler.GetPooledGameObject();

        if (nextGameObject == null)
        {
            return null;
        }
        if (nextGameObject.GetComponent<MyPoolableObject>() == null)
        {
            throw new Exception(gameObject.name + "PoolalbeObject 없음");
        }

        nextGameObject.transform.position = _spawnPosition;

        nextGameObject.gameObject.SetActive(true);

        Projectile projectile = nextGameObject.GetComponent<Projectile>();

        projectile.SetDirection(Vector2.right, transform.rotation, true);

        return nextGameObject;
    }

    public void DetermineSpawnPosition()
    {
        m_spawnPositionCenter = projecttileSpawnTransform == null ?
            transform.position : projecttileSpawnTransform.position;

        m_offset = projecttileSpawnOffset;

        spawnPosition = m_spawnPositionCenter + transform.rotation * m_offset;
    }
}
