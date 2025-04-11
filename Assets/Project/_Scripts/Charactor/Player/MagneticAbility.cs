using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MagneticAbility : ConeOfVision2D
{
    [Header("플레이어")]
    [SerializeField] private PlayerMovement playerController;
    private bool previousFacingDirection = true;
    private bool isScanning = false;

    [Header("자력 능력")]
    public float pullForce = 20f; //기본 자력 세기
    private bool isNorthPole = true; //플레이어 극성 (true: N극, false: S극)
    private SpriteRenderer spriteRenderer;

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
        #region INPUT HANDLER
        //D 키를 누르면 극성 변경
        if (Input.GetKeyDown(KeyCode.D))
        {
            isNorthPole = !isNorthPole;
            UpdateColor();
        }

        if (Input.GetKey(KeyCode.S))
        {
            if (false == isScanning)
            {
                isScanning = true;
                OnOff(isScanning);
            }

            PullMagnet();
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            isScanning = false;
            OnOff(isScanning);
        }
        #endregion

        if (previousFacingDirection != playerController.isFacingRight)
        {
            //Debug.Log("방향 전환");
            UpdateDirection();
            previousFacingDirection = playerController.isFacingRight;
        }
    }

    private void OnOff(bool _trigger)
    {
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


            pole.MagneticActivate(transform.position, pullForce, isNorthPole);
        }
    }
}
