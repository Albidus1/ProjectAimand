using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using System.Xml.Xsl;

public class MagneticAbility : ConeOfVision2D
{
    [Header("플레이어")]
    [SerializeField] private PlayerMovement playerController;
    private bool previousFacingDirection = true;
    private bool isScanning = false;

    public SpriteRenderer spriteRenderer;

    [Header("자력 능력")]
    public float pullForce = 20f; //기본 자력 세기
    [Range(0.01f, 0.5f)]
    public float minAdjust = 0.1f;
    public Ease easeType = Ease.InQuart;

    private float lastPressedAbilityInputTime;

    private bool isNorthPole = true; //플레이어 극성 (true: N극, false: S극)
    private float m_halfXSize;
    private Vector2 m_playerFrontPosition;

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



    protected override void Awake()
    {
        base.Awake();
        OnOff(isScanning);

        playerController = GetComponent<PlayerMovement>();
        m_halfXSize = spriteRenderer.size.x * 0.5f;

        UpdateColor();
    }

    private void Update()
    {
        lastPressedAbilityInputTime -= Time.deltaTime;

        #region INPUT HANDLER
        //D 키를 누르면 극성 변경
        if (Input.GetKeyDown(KeyCode.D) && false == isScanning)
        {
            isNorthPole = !isNorthPole;
            UpdateColor();
        }

        if (Input.GetKey(KeyCode.S))
        {
            lastPressedAbilityInputTime = 0.1f;

            if (false == isScanning)
            {
                OnOff(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            OnOff(false);
        }
        #endregion

        if (lastPressedAbilityInputTime > 0)
        {
            ScanForTargets();
            PullMagnet();
        }
        if (lastPressedAbilityInputTime < 0 && base.visibleTargets.Count > 0)
        {
            base.visibleTargets.Clear();
        }

        if (previousFacingDirection != playerController.isFacingRight)
        {
            //Debug.Log("방향 전환");
            UpdateDirection();
            previousFacingDirection = playerController.isFacingRight;
        }
    }

    private void OnOff(bool _trigger)
    {
        isScanning = _trigger;

        base.shouldDrawMesh = _trigger;
        base.visionMeshFilter.gameObject.SetActive(_trigger);
    }

    public void UpdateDirection()
    {
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

    private void UpdateColor()
    {
        spriteRenderer.color = isNorthPole ? Color.blue : Color.red;
    }

    protected override void ScanForTargets()
    {
        if ((Time.time - base.lastScanTime > base.scanFrequencyInSeconds))
        {
            base.ScanForTargets();
        }
    }

    void PullMagnet()
    {
        foreach (Transform col in base.visibleTargets)
        {
            PlatformMagnetic pole = col.GetComponent<PlatformMagnetic>();

            if (pole == null)
                continue;

            float distance = Vector2.Distance(col.transform.position, transform.position);

            if (distance > base.visionRadius || false == isScanning)
            {
                pole.MagneticActivate(false, Vector3.zero, 0, isNorthPole);
                continue;
            }

            bool isSamePole = ((pole.Pole == PlatformMagnetic.PoleType.NPole) && isNorthPole) ||
                                    ((pole.Pole == PlatformMagnetic.PoleType.SPole) && !isNorthPole);

            Vector2 direction = isSamePole ?
                (m_playerFrontPosition - (Vector2)col.transform.position).normalized :
                ((Vector2)col.transform.position - m_playerFrontPosition).normalized;

            float t = 1f - Mathf.Clamp01(distance / base.visionRadius);
            float rawForce = DOVirtual.EasedValue(0, pullForce, t, easeType);
            float easedForce = Mathf.Max(rawForce, 1);

            //Debug.DrawRay(transform.position, easedForce * Vector2.up, Color.red);

            pole.MagneticActivate(true, direction, easedForce, isSamePole);
        }
    }
}

