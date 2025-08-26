using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("게임/스폰/체크 포인트")]
public class CheckPoint : MonoBehaviour
{
    [Header("스폰")]
    [Tooltip("스폰될 때 바라보는 방향")]
    public bool isFacingRight = true;

    [Tooltip("다른 순서를 무시하고 진입 시 강제로 이 체크포인트를 할당할지 여부")]
    public bool forceAssignation = false;

    [Tooltip("체크포인트의 순서")]
    public int checkpointOrder;

    [Tooltip("이 체크포인트에 여러 번 도달할 수 있는지 여부")]
    public bool canBeReachedMoreThanOnce = true;

    [Tooltip("이 체크포인트에 도달했을 때 발생할 이벤트")]
    public UnityEvent OnCheckPointReached;

    private bool m_reached = false;
    private List<RespawnAble> m_listeners;


    private void Awake()
    {
        m_listeners = new List<RespawnAble>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponent<PlayerMovement>();

        if (player == null)
            return;

        if (m_reached && false == canBeReachedMoreThanOnce)
            return;

        if (false == LevelManager.HasInstance)
            return;

        OnCheckPointReached?.Invoke();
        LevelManager.Instance.SetCurrentCheckPoint(this);
        m_reached = true;
    }

    public void SpawnPlayer(PlayerMovement _player)
    {
        _player.RespawnAt(transform, isFacingRight);

        foreach(RespawnAble listener in m_listeners)
        {
            listener.OnRespawnAble(this, _player);
        }
    }

    public void AssignObjectToCheckPoint(RespawnAble _listener)
    {
        m_listeners.Add(_listener);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0));

        if (false == LevelManager.HasInstance)
            return;

        if (LevelManager.Instance.m_checkPoints == null)
            return;

        if (LevelManager.Instance.m_checkPoints.Count == 0 )
            return;

        for (int i = 0; i < LevelManager.Instance.m_checkPoints.Count; i++)
        {
            if ((i + 1) <  LevelManager.Instance.m_checkPoints.Count)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(
                    LevelManager.Instance.m_checkPoints[i].transform.position, 
                    LevelManager.Instance.m_checkPoints[i + 1].transform.position);
            }
        }
    }
#endif
}
