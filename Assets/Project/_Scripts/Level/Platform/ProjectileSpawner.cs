using System;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour, IEventListener<TriggerEvent>, Respawnable
{
    public bool isActive;

    [Header("풀링 설정")]
    public MyObjectPooler objectPooler;
    [Space(10)]

    [Tooltip("null이면 스폰 위치는 이 오브젝트의 위치로 설정")]
    public Transform projecttileSpawnTransform;
    public Vector3 projecttileSpawnOffset = Vector3.zero;
    public int projecttilePerShot = 1;
    [MyReadOnly]
    public Vector3 spawnPosition = Vector3.zero;

    [Header("이동 설정")]
    public float moveSpeed = 200f;
    public Vector2 direction = Vector2.right;

    [Header("쿨타임")]
    public float initialCooldownTime = 0f;
    public float cooldownTime = 5f;

    [Header("이벤트 설정")]
    public bool useTriggerEvent = false;
    [MyConditionalHide("useTriggerEvent", true)]
    public string eventID = "default";

    private bool m_poolInitialized = false;
    private Vector3 m_spawnPositionCenter;
    private Vector2 m_offset;
    private float m_cooldownTimer = 0f;



    private void Awake()
    {
        objectPooler = GetComponent<MyObjectPooler>();
    }

    private void Start()
    {
        Initialization();
    }

    public void Initialization()
    {
        isActive = false == useTriggerEvent;
        ResetCooldown();
    }

    private void ResetCooldown()
    {
        m_cooldownTimer = initialCooldownTime + Time.time;
    }

    private void Update()
    {   
        if (isActive && m_cooldownTimer < Time.time)
        {
            UseSpawner();
        }
    }

    [ContextMenu("UseSpawner")]
    private void UseSpawner()
    {
        m_cooldownTimer = cooldownTime + Time.time;

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

        nextGameObject.SetActive(true);

        Projectile projectile = nextGameObject.GetComponent<Projectile>();

        projectile.SetDirection(direction.normalized, transform.rotation, true);
        projectile.moveSpeed = moveSpeed;

        return nextGameObject;
    }

    public void DetermineSpawnPosition()
    {
        m_spawnPositionCenter = projecttileSpawnTransform == null ?
            transform.position : projecttileSpawnTransform.position;

        m_offset = projecttileSpawnOffset;

        spawnPosition = m_spawnPositionCenter + transform.rotation * m_offset;
    }

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Initialization();

        if (objectPooler != null)
        {
            objectPooler.DeactivateAllPooledGameObject();
        }
    }

    public void OnEvent(TriggerEvent e)
    {
        if (e.eventID != this.eventID)
        {
            return;
        }

        isActive = true;
        ResetCooldown();
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<TriggerEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<TriggerEvent>();
    }
}
