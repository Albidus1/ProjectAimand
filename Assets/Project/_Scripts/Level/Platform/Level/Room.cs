using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Collider2D roomCollider => m_roomCollider;

    [Header("카메라")]
    public CinemachineCamera virtualCamera;
    public Collider2D confiner;
    public CinemachineConfiner2D cinemachineCameraConfiner;
    public CinemachineCameraController controller;
    [Space(10)]

    public bool resizeConfinerAutomatically = true;
    public bool autoDetectFirstRoomOnStart = true;

    [Header("상태")]
    public bool currentRoom = false;
    public bool roomVisited = false;

    private BoxCollider2D m_roomCollider;
    private Camera m_mainCamera;
    private Vector2 m_cameraSize;
    private bool m_initialized = false;


    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (m_initialized)
        {
            return;
        }

        if (controller == null)
        {
            controller = GetComponentInChildren<CinemachineCameraController>();
        }

        m_roomCollider = GetComponent<BoxCollider2D>();
        m_mainCamera = Camera.main;
        StartCoroutine(ResizeConfiner());
        m_initialized = true;

        if (virtualCamera != null)
        {
            virtualCamera.enabled = false;
        }

        StartCoroutine(CameraInitialization());
    }

    private IEnumerator CameraInitialization()
    {
        yield return null;
        yield return null;

        if (currentRoom)
        {
            yield break;
        }

        if (virtualCamera != null)
        {
            virtualCamera.enabled = false;
        }
    }

    private IEnumerator ResizeConfiner()
    {
        if (virtualCamera == null || confiner == null || false == resizeConfinerAutomatically)
        {
            yield break;
        }

        yield return null;
        yield return null;

        (confiner as BoxCollider2D).offset = m_roomCollider.offset;
        (confiner as BoxCollider2D).size = (m_roomCollider as BoxCollider2D).size;

        m_cameraSize.y = 2 * m_mainCamera.orthographicSize;
        m_cameraSize.x = m_cameraSize.y * m_mainCamera.aspect;

        Vector2 newSize = (confiner as BoxCollider2D).size;

        if ((confiner as BoxCollider2D).size.x < m_cameraSize.x)
        {
            newSize.x = m_cameraSize.x;
        }
        if ((confiner as BoxCollider2D).size.y < m_cameraSize.y)
        {
            newSize.y = m_cameraSize.y;
        }

        (confiner as BoxCollider2D).size = newSize;
        cinemachineCameraConfiner.InvalidateBoundingShapeCache();

        HandleLevelStartDetection();
    }

    private void HandleLevelStartDetection()
    {
        if (false == m_initialized)
        {
            Initialization();
        }

        if (autoDetectFirstRoomOnStart && LevelManager.HasInstance)
        {
            if (m_roomCollider.bounds.Contains(LevelManager.Instance.player.transform.position.MySetZ(transform.position.z)))
            {
                CameraEvent2D.Trigger(CameraEventType.ResetPriorities);
                CinemachineBrainEvent.Trigger(0.3f);

                if (virtualCamera != null)
                {
                    virtualCamera.Priority = 10;
                }

                PlayerEnterRoom();
            }
        }
    }

    public void PlayerEnterRoom()
    {
        currentRoom = true;

        if (virtualCamera != null)
        {
            virtualCamera.enabled = true;
        }
    }

    public void PlayerExitRoom()
    {
        if (virtualCamera != null)
        {
            virtualCamera.enabled = false;
        }

        currentRoom = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerEnterRoom();

            controller.SetTarget(LevelManager.Instance.player);
            controller.StartFollowing();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerExitRoom();

            controller.StopFollowing();
        }
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("방 바뀜");
            cm.confiner.BoundingShape2D = m_roomCollider;
            cm.confiner.InvalidateBoundingShapeCache();

            cm.confiner.Damping = 3f;
            cm.confiner.SlowingDistance = 2f;

            cm.cinemachineCamera.ForceCameraPosition(cm.cinemachineCamera.State.GetFinalPosition(), cm.cinemachineCamera.State.GetFinalOrientation());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }*/

    protected virtual void OnEnable()
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_roomCollider == null)
        {
            m_roomCollider = (BoxCollider2D)GetComponent<Collider2D>();
        }

        Vector3 pos = m_roomCollider.bounds.center;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos, m_roomCollider.bounds.size);
    }
#endif
}
