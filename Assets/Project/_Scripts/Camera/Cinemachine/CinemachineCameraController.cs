using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraController : MonoBehaviour, IEventListener<CameraEvent2D>
{
    public bool followsPlayer { get; set; }

    [Header("기본 세팅")]
    public bool followsAPlayer = true;
    public Transform targetPlayer;
    public PlayerMovement playerMovement;
    [Space(10)]

    [Header("Orthograhpic")]
    public Vector2 orthographicZoom = new Vector2(5f, 9f);
    public float initialOrthographicZoom = 5f;
    public float orthographicZoomSpeed = 0.4f;


    private CinemachineCamera m_virtualCamera;
    private CinemachineConfiner2D m_confiner;
    private float m_currentZoom;



    private void Awake()
    {
        m_virtualCamera = GetComponent<CinemachineCamera>();
        m_confiner = GetComponent<CinemachineConfiner2D>();

        m_currentZoom = initialOrthographicZoom;
    }

    private void Start()
    {
        m_virtualCamera.Lens.OrthographicSize = initialOrthographicZoom;
    }

    public void SetTarget(PlayerMovement _character)
    {
        //Debug.Log("타겟 설정");
        targetPlayer = _character.transform;
        playerMovement = _character;
    }

    public void StartFollowing()
    {
        if (false == followsAPlayer)
            return;

        followsPlayer = true;

        if (playerMovement != null && playerMovement.CameraTarget != null)
        {
            m_virtualCamera.Target.TrackingTarget = playerMovement.CameraTarget;
        }
        else
        {
            m_virtualCamera.Target.TrackingTarget = targetPlayer.transform;
        }

        m_virtualCamera.enabled = true;
    }

    public void StopFollowing()
    {
        if (false == followsAPlayer)
            return;

        followsPlayer = false;
        m_virtualCamera.enabled = false;
        //m_virtualCamera.Target.TrackingTarget = null;
    }

    private void LateUpdate()
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        //PerformOrthographicZoom();
    }

    private void PerformOrthographicZoom()
    {
        if (playerMovement == null) 
            return;


        Vector2 speed = playerMovement.speed;

        float playerSpeed = Mathf.Abs(speed.x);
        float currentVelocity = Mathf.Max(playerSpeed, 0);
        float targetZoom = MyMaths.Remap(currentVelocity, 0, 16, orthographicZoom.x, orthographicZoom.y);
        m_currentZoom = Mathf.Lerp(m_currentZoom, targetZoom, Time.deltaTime * orthographicZoomSpeed);
        m_virtualCamera.Lens.OrthographicSize = m_currentZoom;
    }

    public void OnEvent(CameraEvent2D e)
    {
        switch (e.eventType)
        {
            case CameraEventType.SetTargetCharacter:
                SetTarget(e.targetCharacter);
                break;

            case CameraEventType.SetConfiner:
                if (m_confiner != null && e.bounds2D != null)
                {
                    m_confiner.BoundingShape2D = e.bounds2D;
                }
                break;

            case CameraEventType.StartFollowing:
                if (e.targetCharacter != null && e.targetCharacter != targetPlayer)
                {
                    return;
                }

                StartFollowing();
                break;

            case CameraEventType.StopFollowing:
                if (e.targetCharacter != null && e.targetCharacter != targetPlayer)
                {
                    return;
                }

                StopFollowing();
                break;

            case CameraEventType.ResetPriorities:
                m_virtualCamera.Priority = 0;
                break;
        }
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<CameraEvent2D>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<CameraEvent2D>();
    }
}
