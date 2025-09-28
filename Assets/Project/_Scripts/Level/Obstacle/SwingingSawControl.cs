using System;
using UnityEngine;

public class SwingingSawControl : MonoBehaviour, IEventListener<TriggerEvent>,Respawnable
{
    [MyReadOnly]
    public bool isActive = true;

    [Header("스윙 설정")]
    [Range(0f, 180f)]
    public float swingAngle = 45f;
    [Range(-180f, 180f)]
    public float startAngleOffset = 0f;
    public float swingSpeed = 2f;

    [Header("피봇 설정")]
    public Transform pivotPoint;
    public float distanceFromPivot = 5f;

    [Header("이벤트 설정")]
    public bool useTriggerEvent = false;
    [MyConditionalHide("useTriggerEvent", true)]
    public string eventID = "default";

    private bool m_isInitialized = false;
    private float m_timeCounter;
    private Vector2 m_initialPosition;



    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (false == m_isInitialized)
        {
            m_initialPosition = transform.position;

            if (pivotPoint == null && transform.parent != null)
            {
                pivotPoint = transform.parent;
            }
            if (pivotPoint == null && transform.parent == null)
            {
                pivotPoint = new GameObject("PivotPoint").transform;
                transform.SetParent(pivotPoint);

                pivotPoint.position = transform.position;
                transform.position += Vector3.down * distanceFromPivot;
            }

            m_isInitialized = true;
        }

        isActive = false == useTriggerEvent;
        m_timeCounter = startAngleOffset;
        transform.position = m_initialPosition;

        SwingMovement();
    }

    private void Update()
    {
        if (false == isActive)
        {
            return;
        }

        SwingMovement();
    }

    private void SwingMovement()
    {
        m_timeCounter += swingSpeed * Time.deltaTime;
        SetRotation();
    }

    private void SetRotation()
    {
        float angle = Mathf.Sin(m_timeCounter) * swingAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (pivotPoint != null)
        {
            float radianAngle = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2
                (Mathf.Sin(radianAngle) * distanceFromPivot,
                -Mathf.Cos(radianAngle) * distanceFromPivot);

            transform.position = (Vector2)pivotPoint.position + offset;
        }
    }

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Initialization();
    }

    public void OnEvent(TriggerEvent e)
    {
        if (e.eventID != this.eventID)
        {
            return;
        }

        isActive = true;
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
