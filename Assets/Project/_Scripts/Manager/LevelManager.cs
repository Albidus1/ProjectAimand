using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.PlayerLoop;



[Serializable]
public struct PointOfEntry
{
    public string name;
    public Transform position;
}

[AddComponentMenu("게임/Managers/LevelManager")]
public class LevelManager : MySingleton<LevelManager>
{
    public enum CheckpointsAxis { x, y, checkpointOrder }
    public enum CheckpointDirection { Asending, Descending }


    [Header("플레이어")]
    public PlayerMovement playerPrefab;


    [Header("체크 포인트")]
    public CheckPoint debugSpawn;
    public CheckpointsAxis checkpointAttributeAxis = CheckpointsAxis.x;
    public CheckpointDirection checkpointAttributeDirection = CheckpointDirection.Asending;

    [MyReadOnly] public CheckPoint currentCheckPoint;

    [Space(10)]
    [Header("Points of Entry")]
    public List<PointOfEntry> pointsOfEntry;

    [Space(10)]
    [Header("인트로 & 아웃트로")]
    public float respawnDelay = 2f;

    [Space(10)]
    [Header("레벨 바운드")] 
    public Bounds levelBounds = new Bounds(Vector3.zero, Vector3.one * 10);
    public Collider2D boundsCollider2D { get; private set; }

    public virtual CameraController levelCameraController { get; set; }

    public virtual PlayerMovement player { get; private set; }
    public virtual List<CheckPoint> m_checkPoints { get; private set; }
    private int m_savedPoints;
    private BoxCollider2D m_collider2D;
    private Bounds m_bounds;



    protected override void Awake()
    {
        base.Awake();
        m_bounds = levelBounds;
    }

    private void Start()
    {
        InstantiatePlayableCharacters();

        if (player == null)
            return;

        Initialization();
        SpawnPlayer();

        CheckpointAssignment();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeStatics()
    {
        // 서브 시스템이 등록될 때 호출
        m_instance = null;
    }

    private void Initialization()
    {
        levelCameraController = FindFirstObjectByType<CameraController>();

        switch (checkpointAttributeAxis)
        {
            case CheckpointsAxis.x:
                if (checkpointAttributeDirection == CheckpointDirection.Asending)
                {
                    m_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderBy(o => o.transform.position.x)
                                    .ToList();
                }
                else
                {
                    m_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderByDescending(o => o.transform.position.x)
                                    .ToList();
                }
                break;

            case CheckpointsAxis.y:
                if (checkpointAttributeDirection == CheckpointDirection.Asending)
                {
                    m_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderBy(o => o.transform.position.y)
                                    .ToList();
                }
                else
                {
                    m_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderByDescending(o => o.transform.position.y)
                                    .ToList();
                }
                break;

            case CheckpointsAxis.checkpointOrder:
                m_checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                .OrderBy(o => o.checkpointOrder)
                                .ToList();
                break;
        }

        currentCheckPoint = m_checkPoints.Count > 0 ? m_checkPoints[0] : null;
    }

    private void InstantiatePlayableCharacters()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("플레이어 프리팹이 없습니다.");
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player.name = playerPrefab.name;
    }

    private void CheckpointAssignment()
    {

    }

    private void SpawnPlayer()
    {
#if UNITY_EDITOR
        if (debugSpawn != null)
        {
            debugSpawn.SpawnPlayer(player);
        }
        else
        {
            RegularSpawnPlayer();
        }
#else
            RegularSpawnPlayer();
#endif
    }

    private void RegularSpawnPlayer()
    {
        if (currentCheckPoint != null)
        {
            currentCheckPoint.SpawnPlayer(player);
            return;
        }
    }

    public virtual void SetCurrentCheckPoint(CheckPoint _newCheckPoint)
    {
        if (_newCheckPoint.forceAssignation)
        {
            currentCheckPoint = _newCheckPoint;
            return;
        }

        if (currentCheckPoint == null)
        {
            currentCheckPoint = _newCheckPoint;
            return;
        }

        if (currentCheckPoint.checkpointOrder >= _newCheckPoint.checkpointOrder)
        {
            currentCheckPoint = _newCheckPoint;
            return;
        }
    }

    public void PlayerDead(PlayerMovement _player)
    {
        if (player != null)
        {
            StartCoroutine(Restart());
        }
    }

    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(respawnDelay);
       
        if (currentCheckPoint != null)
        {
            currentCheckPoint.SpawnPlayer(player);
        }
    }
}
