using System;
using System.Collections.Generic;
using UnityEngine;



public class AutoRespawn : MonoBehaviour, Respawnable
{
    [Header("플레이어 재생성 시 함께 재생성")]
    [Tooltip("true로 설정하면, 플레이어가 부활할 때 이 객체도 마지막 위치에서 재생성.")]
    public bool respawnOnPlayerRespawn = true;
    [Tooltip("true로 설정하면, 플레이어가 부활할 때 이 객체를 초기 위치에 재배치.")]
    public bool repositionToInitOnPlayerRespawn = true;
    //[Tooltip("Kill 함수가 호출될 때 이 객체가 자신의 게임 오브젝트를 비활성화할지 여부")]
    //public bool disableOnKill = true;

    [Tooltip("0보다 큰 값을 가지면, 이 객체는 사망 후 X초가 지나면 마지막 위치에서 재생성.")]
    public float autoRespawnDuration;
    [Tooltip("이 객체가 자동으로 재생성될 수 있는 횟수, 음수 값: 무제한")]
    public int autoRespawnAmount;
    [Tooltip("남은 재생성 횟수")]
    [MyReadOnly]
    public int autoRespawnRemainingAmount;

    [Tooltip("플레이어가 이 체크포인트들에서 재생성될 때 객체도 함께 재생성.")]
    public bool ignoreCheckPointsAlwaysRespawn = true;
    public List<CheckPoint> associatedCheckpoints;

    public delegate void OnReviveDelegate();
    public OnReviveDelegate OnRevive;

    private MonoBehaviour[] m_otherComponents;
    private Collider2D m_collider2D;
    private Renderer m_renderer;
    private Health m_health;
    private bool m_reviving = false;
    private float m_deathTimer = 0f;
    private Vector2 m_initialPosition;



    private void Start()
    {
        m_otherComponents = GetComponents<MonoBehaviour>();
        m_collider2D = GetComponent<Collider2D>();
        m_renderer = GetComponent<Renderer>();
        m_health = GetComponent<Health>();


        autoRespawnRemainingAmount = autoRespawnAmount;
        m_initialPosition = transform.position;
    }


    public virtual void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        if (repositionToInitOnPlayerRespawn)
        {
            transform.position = m_initialPosition;
        }

        if (respawnOnPlayerRespawn)
        {
            Revive();
        }

        autoRespawnRemainingAmount = autoRespawnAmount;
    }

    private void Update()
    {
        if (false == m_reviving)
        {
            return;
        }

        if (m_deathTimer + autoRespawnDuration > Time.time)
        {
            return;
        }

        if (autoRespawnAmount <= 0 || autoRespawnRemainingAmount <= 0)
        {
            return;
        }

        autoRespawnRemainingAmount -= 1;

        Revive();
        m_reviving = false;
    }

    public void Kill()
    {
        if (autoRespawnDuration <= 0f) //&& disableOnKill)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            SetComponentsStatus(true);

            m_reviving = true;
            m_deathTimer = Time.time;
        }
    }

    public void Revive()
    {
        if (m_health != null)
        {
            m_health.Revive();
        }

        if (autoRespawnDuration <= 0f)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            SetComponentsStatus(true);
        }

        OnRevive?.Invoke();
    }

    private void SetComponentsStatus(bool _flag)
    {
        foreach (MonoBehaviour component in m_otherComponents)
        {
            component.enabled = _flag;
        }

        if (m_collider2D != null)
        {
            m_collider2D.enabled = _flag;
        }
        if (m_renderer != null)
        {
            m_renderer.enabled = _flag;
        }
    }
}
