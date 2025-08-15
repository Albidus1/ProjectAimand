using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Collider2D roomCollider => m_roomCollider;
    public bool isEntered { get; private set; } = false;

    [Header("카메라")]
    public CinemachineCamera virtualCamera;
    public Collider2D confiner;
    public CinemachineConfiner2D cinemachineCameraConfiner;
    public CinemachineCameraController controller;

    [Header("웨이브")]
    public EnemyWave enemyWave;

    private BoxCollider2D m_roomCollider;
    private Camera m_mainCamera;
    private Vector2 m_cameraSize;



    private void Start()
    {
        m_roomCollider = GetComponent<BoxCollider2D>();
        m_mainCamera = Camera.main;
        StartCoroutine(ResizeConfiner());

        if (virtualCamera != null)
        {
            virtualCamera.enabled = false;
        }

        controller = GetComponentInChildren<CinemachineCameraController>();

        enemyWave = GetComponentInChildren<EnemyWave>();
    }

    private IEnumerator ResizeConfiner()
    {
        if (virtualCamera == null || confiner == null)
        {
            yield break;
        }

        yield return null;
        yield return null;

        (confiner as BoxCollider2D).offset = m_roomCollider.offset;
        (confiner as BoxCollider2D).size = m_roomCollider.size;

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
    }

    public void PlayerEnterRoom()
    {
        if (virtualCamera != null)
        {
            virtualCamera.enabled = true;
            isEntered = true;
        }
    }

    public void PlayerExitRoom()
    {
        if (virtualCamera != null)
        {
            virtualCamera.enabled = false;
            isEntered = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerEnterRoom();

            controller.SetTarget(LevelManager.Instance.player);
            controller.StartFollowing();

            if (enemyWave != null && false == enemyWave.isWaveActive)
            {
                enemyWave.StartNextWave();
            }
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
}
