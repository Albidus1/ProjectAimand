using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraController : MonoBehaviour
{
    public bool followsPlayer { get; set; }

    [Header("기본 세팅")]
    public bool followsAPlayer = true;
    public GameObject targetPlayer;
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
        targetPlayer = _character.gameObject;
        playerMovement = _character;
    }

    public void StartFollowing()
    {
        if (false == followsAPlayer)
            return;

        followsPlayer = true;
        m_virtualCamera.Target.TrackingTarget = targetPlayer.transform;
        m_virtualCamera.enabled = true;
    }

    public void StopFollowing()
    {
        if (false == followsAPlayer)
            return;

        followsPlayer = false;
        m_virtualCamera.Target.TrackingTarget = null;
        m_virtualCamera.enabled = false;
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


        float playerSpeed = Mathf.Abs(playerMovement.rb.linearVelocity.x);
        float currentVelocity = Mathf.Max(playerSpeed, 0);
        float targetZoom = Remap(playerSpeed, 0, 16, orthographicZoom.x, orthographicZoom.y);
        m_currentZoom = Mathf.Lerp(m_currentZoom, targetZoom, Time.deltaTime * orthographicZoomSpeed);
        m_virtualCamera.Lens.OrthographicSize = m_currentZoom;
    }

    public float Remap(float x, float A, float B, float C, float D)
    {
        float remappedValue = C + (x - A) / (B - A) * (D - C);
        return remappedValue;
    }
}
