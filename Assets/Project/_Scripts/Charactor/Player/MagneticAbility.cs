using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class MagneticAbility : ConeOfVision2D
{
    [Header("플레이어")]
    [SerializeField] private PlayerMovement playerController;
    private bool previousFacingDirection = true;
    private bool isScanning = false;

    private SpriteRenderer spriteRenderer;

    [Header("자력 능력")]
    public float pullForce = 20f; //기본 자력 세기
    [Range(0.01f, 0.5f)]
    public float minAdjust = 0.1f;
    public Ease easeType = Ease.InQuart;

    private float lastPressedAbilityInputTime;

    private bool isNorthPole = true; //플레이어 극성 (true: N극, false: S극)


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
        spriteRenderer = GetComponent<SpriteRenderer>();

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

        base.shouldScanForTargets = _trigger;
        base.shouldDrawMesh = _trigger;
        base.visionMeshFilter.gameObject.SetActive(_trigger);
    }

    public void UpdateDirection()
    {
        if (transform.localScale.x > 0)
        {
            abilityDirection = Vector3.right;
        }
        else
        {
            abilityDirection = Vector3.left;
        }
    }

    void UpdateColor()
    {
        spriteRenderer.color = isNorthPole ? Color.blue : Color.red;
    }

    void PullMagnet()
    {
        foreach (Transform col in base.visibleTargets)
        {
            PlatformMagnetic pole = col.GetComponent<PlatformMagnetic>();

            if (pole == null)
                continue;

            Vector2 direction = col.transform.position - transform.position;
            float distance = Vector2.Distance(col.transform.position, transform.position);

            if (distance < 0.75f || distance > base.visionRadius || false == isScanning)
            {
                pole.MagneticActivate(false, Vector3.zero, 0, isNorthPole);
                return;
            }
            
            float normalizedDistance = Mathf.Clamp01(distance / base.visionRadius);
            float t = 1 - normalizedDistance;
            float easedForce = (isNorthPole != (pole.Pole == PlatformMagnetic.PoleType.NPole)) ?
                                            DOVirtual.EasedValue(0, pullForce, Mathf.Max(t, minAdjust), easeType) :
                                            DOVirtual.EasedValue(pullForce, 0, Mathf.Max(t, minAdjust), easeType);

            Debug.DrawRay(transform.position, easedForce * Vector2.up, Color.red);

            pole.MagneticActivate(true, direction.normalized, easedForce, isNorthPole);
        }
    }
}
