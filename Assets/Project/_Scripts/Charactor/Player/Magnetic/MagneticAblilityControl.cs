using DG.Tweening;
using UnityEngine;

public class MagneticAblilityControl : ConeOfVision2D
{
    public enum UseType { None, Pull, Push}
    protected UseType m_UseType;

    [Header("플레이어")]
    public PlayerMovement playerController;
    public SpriteRenderer spriteRenderer;

    [Header("인력")]
    public float pullRadius;
    [Range(0, 360)] public float pullAngle;
    [Space(10)]

    public float pullPower;
    public Ease pullMoveEase;

    [Header("척력")]
    public float pushRadius;
    public float pushRadiusLimit;
    [Range(0, 360)] public float pushAngle;
    [Space(10)]

    public float pushPower;
    public Ease pushMoveEase;
    public float pushableTime = 0.5f;

    [MyReadOnly]
    public Vector3 abilityDirection
    {
        get
        {
            return base.direction;
        }
        set
        {
            base.direction = value;
            base.angleOffset = (base.direction == Vector3.right) ? 0 : 180;
            base.SetDirectionAndAngles(base.direction, transform.eulerAngles);
        }
    }


    protected bool isScanning = false;
    protected bool isPulling = false;
    protected bool isPushing = false;
    protected float m_pushTimer = -0.1f;

    protected bool m_previousFacingDirection = true;
    protected float m_halfXSize;
    protected float m_halfYSize;
    protected Vector2 m_playerFrontPosition;
    protected float m_lastPressedAbilityInputTime;


    protected override void Awake()
    {
        Initializaion();
    }

    protected override void OnEnable()
    {
        Initializaion();
    }

    public void Initializaion()
    {
        playerController = GetComponent<PlayerMovement>();
        base.Awake();
    }

    private void Update()
    {
        m_lastPressedAbilityInputTime -= Time.deltaTime;
        m_UseType = UseType.None;

        #region INPUT HANDLER
        if (Time.time > m_pushTimer && Input.GetKeyDown(KeyCode.D))
        {
            m_lastPressedAbilityInputTime = 0.1f;

            isPushing = true;
            m_UseType = UseType.Push;
        }

        if (Time.time > m_pushTimer && Input.GetKey(KeyCode.S) && false == isPushing)
        {
            m_lastPressedAbilityInputTime = 0.1f;
            isPulling = true;
            m_UseType = UseType.Pull;
        }

        if (Input.GetKeyUp(KeyCode.S) || isPushing)
        {
            isPulling = false;
        }
        #endregion
    }

    protected override void LateUpdate()
    {
        SetMesh();
        base.LateUpdate();

        if (m_lastPressedAbilityInputTime > 0)
        {
            OnOff(true);
            UseMagneticAbility();
        }
        else
        {
            OnOff(false);
            CheckMagnet();
        }

        if (m_previousFacingDirection != playerController.isFacingRight)
        {
            //Debug.Log("방향 전환");
            UpdateDirection();
            m_previousFacingDirection = playerController.isFacingRight;
        }
    }

    private void SetMesh()
    {
        switch (m_UseType)
        {
            case UseType.None:
                base.visionRadius = pushRadiusLimit;
                base.visionAngle = 360;
                break;
            case UseType.Pull:
                base.visionRadius = pullRadius;
                base.visionAngle = pullAngle;
                break;
            case UseType.Push:
                base.visionRadius = pushRadius;
                base.visionAngle = pushAngle;
                break;
        }
    }

    private void UseMagneticAbility()
    {
        switch (m_UseType)
        {
            case UseType.Pull:
                PullMagnet();
                break;
            case UseType.Push:
                PushMagnet();
                break;
        }
    }

    private void PullMagnet()
    {
        PlatformMagnetic closestPole = null;
        float minDistance = float.MaxValue;
        Vector2 closestDirection = Vector2.zero;

        foreach (Transform col in base.visibleTargets)
        {
            if (false == col.TryGetComponent<PlatformMagnetic>(out var pole)) 
            { 
                continue; 
            }

            float distance = Vector2.Distance(col.position, m_playerFrontPosition);
            if (distance < minDistance && distance <= pullRadius)
            {
                minDistance = distance;
                closestPole = pole;
                closestDirection = (m_playerFrontPosition - (Vector2)col.position).normalized;
            }
        }

        if (closestPole != null)
        {
            float t = 1f - Mathf.Clamp01(minDistance / pullRadius);
            float force = DOVirtual.EasedValue(0, pullPower, t, pullMoveEase);

            closestPole.MagneticActivate(true, closestDirection, force, true);
        }
    }

    private void PushMagnet()
    {
        m_pushTimer = Time.time + pushableTime;

        foreach (Transform col in base.visibleTargets)
        {
            if (false == col.TryGetComponent<PlatformMagnetic>(out var pole))
            {
                continue;
            }

            Vector2 direction = ((Vector2)col.position - m_playerFrontPosition).normalized;
            pole.MagneticActivate(true, direction, pushPower, false);
        }

        isPushing = false;
    }

    private void CheckMagnet()
    {
        foreach (Transform col in base.visibleTargets)
        {
            if (col.TryGetComponent<PlatformMagnetic>(out var pole) && pole.isActive)
            {
                pole.MagneticActivate(false, Vector3.zero);
            }
        }
    }

    #region GENERAL METHODS
    protected virtual void OnOff(bool _trigger)
    {
        isScanning = _trigger;

        base.shouldDrawMesh = _trigger;
        //base.visionMeshFilter.gameObject.SetActive(_trigger);
    }

    protected virtual void UpdateDirection()
    {
        m_halfXSize = playerController.col.bounds.size.x * 0.5f;
        m_halfYSize = playerController.col.bounds.size.y * 0.5f;

        if (transform.localScale.x > 0)
        {
            abilityDirection = Vector3.right;
            m_playerFrontPosition = new Vector2(transform.position.x + m_halfXSize, transform.position.y);
        }
        else
        {
            abilityDirection = Vector3.left;
            m_playerFrontPosition = new Vector2(transform.position.x - m_halfXSize, transform.position.y);
        }
    }
    #endregion
}
