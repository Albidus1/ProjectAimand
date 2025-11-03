using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;



//[Serializable]
//public struct PointOfEntry
//{
//    public string name;
//    public Transform position;
//}

[AddComponentMenu("게임/Managers/LevelManager")]
public class LevelManager : MySingleton<LevelManager>
{
    public enum CheckpointsAxis { x, y, checkpointOrder }
    public enum CheckpointDirection { Asending, Descending }


    [Header("플레이어")]
    public GameObject playerPrefab;

    [Header("빛")]
    public MyFollowTarget light2D;

    [Header("체크 포인트")]
    public CheckPoint debugSpawn;
    public CheckpointsAxis checkpointAttributeAxis = CheckpointsAxis.x;
    public CheckpointDirection checkpointAttributeDirection = CheckpointDirection.Asending;

    [MyReadOnly] public CheckPoint currentCheckPoint;

    //[Space(10)]
    //[Header("Points of Entry")]
    //public List<PointOfEntry> pointsOfEntry;

    [Space(10)]
    [Header("인트로 & 아웃트로")]
    public float respawnDelay = 2f;

    [Space(10)]
    [Header("레벨 바운드")] 
    public Bounds levelBounds = new Bounds(Vector3.zero, Vector3.one * 10);


    public virtual CameraController levelCameraController { get; set; }
    public virtual PlayerMovement player { get; private set; }
    public Collider2D boundsCollider2D { get; private set; }
    public virtual List<CheckPoint> checkPoints { get; private set; }

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
        {
            return; 
        }

        Initialization();

        MainEvent.Trigger(MainEventTypes.SpawnPlayer);
        SpawnPlayer();

        CheckpointAssignment();

        MainEvent.Trigger(MainEventTypes.LevelStart, player);
        GameManager.Instance.SetCursorVisible(false);

        CameraEvent2D.Trigger(CameraEventType.SetConfiner, null, boundsCollider2D);
        CameraEvent2D.Trigger(CameraEventType.SetTargetCharacter, player);
        CameraEvent2D.Trigger(CameraEventType.StartFollowing);
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
                    checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderBy(o => o.transform.position.x)
                                    .ToList();
                }
                else
                {
                    checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderByDescending(o => o.transform.position.x)
                                    .ToList();
                }
                break;

            case CheckpointsAxis.y:
                if (checkpointAttributeDirection == CheckpointDirection.Asending)
                {
                    checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderBy(o => o.transform.position.y)
                                    .ToList();
                }
                else
                {
                    checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                    .OrderByDescending(o => o.transform.position.y)
                                    .ToList();
                }
                break;

            case CheckpointsAxis.checkpointOrder:
                checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                .OrderBy(o => o.checkpointOrder)
                                .ToList();
                break;
        }

        currentCheckPoint = checkPoints.Count > 0 ? checkPoints[0] : null;
    }

    private void InstantiatePlayableCharacters()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("플레이어 프리팹이 없습니다.");
            return;
        }

        player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<PlayerMovement>();
        player.name = playerPrefab.name;

        if (light2D != null)
        {
            light2D.target = player.transform;
        }

        AttatchToPlayer[] attatchToPlayers = FindObjectsByType<AttatchToPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var obj in attatchToPlayers)
        {
            obj.Attatch(player.transform);
        }
    }

    private void CheckpointAssignment()
    {
        IEnumerable<Respawnable> listeners = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OfType<Respawnable>();
        AutoRespawn autoRespawn;

        foreach (Respawnable listener in listeners)
        {
            for (int i = checkPoints.Count - 1; i >= 0; i--)
            {
                autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
                if (autoRespawn != null)
                {
                    if (autoRespawn.ignoreCheckPointsAlwaysRespawn)
                    {
                        checkPoints[i].AssignObjectToCheckPoint(listener);
                        continue;
                    }
                    else
                    {
                        if (autoRespawn.associatedCheckpoints.Contains(checkPoints[i]))
                        {
                            checkPoints[i].AssignObjectToCheckPoint(listener);
                        }

                        continue;
                    }
                }


                Vector3 vectorDistance = ((MonoBehaviour)listener).transform.position - checkPoints[i].transform.position;

                float distance = 0;
                if (checkpointAttributeAxis == CheckpointsAxis.x)
                {
                    distance = vectorDistance.x;
                }
                else if (checkpointAttributeAxis == CheckpointsAxis.y)
                {
                    distance = vectorDistance.y;
                }

                if(distance < 0 && checkpointAttributeDirection == CheckpointDirection.Asending)
                {
                    continue;
                }
                else if (distance > 0 && checkpointAttributeDirection == CheckpointDirection.Descending)
                {
                    continue;
                }

                checkPoints[i].AssignObjectToCheckPoint(listener);
                break;
            }      
        }
    }

    private void SpawnPlayer()
    {
#if UNITY_EDITOR
        if (debugSpawn != null && debugSpawn.gameObject.activeInHierarchy)
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

    public void KillPlayer()
    {
        if (player.TryGetComponent<Health>(out var health))
        {   
            health.Kill();
        }
    }

    public void PlayerDead()
    {
        StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        player.movementState.StateChange(PlayerStates.MovementStates.Die);

        MainEvent.Trigger(MainEventTypes.PlayerDeath, player);
        CameraEvent2D.Trigger(CameraEventType.StopFollowing);

        yield return new WaitForSeconds(0.2f);

        MyFadeInEvent.Trigger(0.5f, Ease.Linear, true, Vector2.zero);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(respawnDelay);

        if (currentCheckPoint != null)
        {
            player.gameObject.SetActive(true);
            player.movementState.StateChange(PlayerStates.MovementStates.Idle);
            currentCheckPoint.SpawnPlayer(player);

            MainEvent.Trigger(MainEventTypes.PlayerRespawn, player);
            CameraEvent2D.Trigger(CameraEventType.StartFollowing);
        }

        MyFadeOutEvent.Trigger(0.5f, Ease.Linear);
        yield return new WaitForSeconds(0.5f);
    }

    public void GoToLevel(string _levelName, bool _fadeOut = true)
    { 
        if (_fadeOut)
        {
            if (player != null)
            {
                MyFadeInEvent.Trigger(1f, DG.Tweening.Ease.Linear, true, player.transform.position);
            }
            else
            {
                MyFadeInEvent.Trigger(1f, DG.Tweening.Ease.Linear, true, Vector3.zero);
            }
        }

        StartCoroutine(GotoLevelCoroutine(_levelName, _fadeOut));
    }

    private IEnumerator GotoLevelCoroutine(string _levelName, bool _fadeOut = true)
    {
        if (player != null)
        {
            player.enabled = false;
        }

        if (_fadeOut)
        {
            yield return new WaitForSeconds(1f);
        }

        string destinationScene = (string.IsNullOrEmpty(_levelName)) ? "StartScreen" : _levelName;
        LoadScene(_levelName);
    }

    private void LoadScene(string _destinationScene)
    {
        SceneLoadingManager.LoadScene(_destinationScene);
    }

    public void RestartScene()
    {
        if (SceneManager.GetActiveScene().isLoaded)
        {
            SceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }
#else
        Application.Quit();
#endif
    }
}
