using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerRangeAttack : MonoBehaviour
{
    [SerializeField]
    private PlayerAttackData attackData;
    [SerializeField]
    private InputActionReference actionReference;
    [SerializeField]
    private ChargeBarPresenter chargeBarPresenter;
    [SerializeField]
    private Health playerHealth;
    [SerializeField]
    private Transform bulletPos;

    [SerializeField]
    private PlayerMovement playerMovement;
    private bool isJumping
    {
        get
        {
            // 나중에 조건을 playerMovement.movementState로 변경 필요
            return Mathf.Abs(playerMovement.rb.linearVelocityY) > 0.1f;
        }
    }

    private float holdStartTime;
    private bool isCharging = false;

    private GameObject rangeAttackSquareObject;
    private Mesh rangeAttackSquare;

    private void Awake()
    {
        if (chargeBarPresenter == null)
        {
            chargeBarPresenter = FindFirstObjectByType<ChargeBarPresenter>();
            if (chargeBarPresenter == null) Debug.LogError("차지바 presenter를 찾을 수 없습니다.");
        }

        InitializeRangeAttackSquareMesh();
    }

    private void Start()
    {
        actionReference.action.started += context => { OnStartHold(context); };
        actionReference.action.performed += context => { OnHolding(context); };
        actionReference.action.canceled += context => { OnReleaseHold(context); };
    }

    private void OnEnable()
    {
        playerHealth.OnDamageEvent.AddListener(OnDamaged);
    }

    private void OnDisable()
    {
        playerHealth.OnDamageEvent.RemoveListener(OnDamaged);
    }

    private void InitializeRangeAttackSquareMesh()
    {
        rangeAttackSquareObject = new GameObject();
        rangeAttackSquareObject.transform.parent = this.transform;
        rangeAttackSquareObject.transform.localPosition = Vector3.zero;
        rangeAttackSquareObject.transform.localRotation = Quaternion.identity;

        rangeAttackSquare = new Mesh();
        const float height = 0.2f;
        const float width = 20f;
        const float halfHeight = height * 0.5f;

        rangeAttackSquare.Clear();
        rangeAttackSquare.vertices = new Vector3[4]{
                new Vector3(0,-halfHeight,0),
                new Vector3(width, -halfHeight, 0),
                new Vector3(width, halfHeight, 0),
                new Vector3(0, halfHeight, 0)
            };
        rangeAttackSquare.triangles = new int[6]{
                0, 2, 1,
                0, 3, 2
            };
        rangeAttackSquare.RecalculateNormals();
        rangeAttackSquare.RecalculateBounds();

        MeshFilter meshFilter = rangeAttackSquareObject.AddComponent<MeshFilter>();
        meshFilter.mesh = rangeAttackSquare;
        MeshRenderer meshRenderer = rangeAttackSquareObject.AddComponent<MeshRenderer>();

        // 머테리얼
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0f, 0f, 0.3f);
        meshRenderer.material = mat;

        rangeAttackSquareObject.SetActive(false);
    }

    private void OnStartHold(CallbackContext context)
    {
        if (isJumping) return;

        isCharging = true;
        holdStartTime = Time.time;
    }

    private void OnHolding(CallbackContext context)
    {

    }

    private void OnReleaseHold(CallbackContext context)
    {
        if (!isCharging) return;

        isCharging = false;

        // Hold 시간 계산
        float holdingTime = Time.time - holdStartTime;
        if (holdingTime < attackData.rangeAttackHoldMinTime) return;

        // 대미지 비율 계산
        float damage = Mathf.Lerp(
            attackData.rangeAttackMinDamage, 
            attackData.rangeAttackMaxDamage, 
            holdingTime - attackData.rangeAttackHoldMinTime / 
            attackData.rangeAttackHoldMaxTime - attackData.rangeAttackHoldMinTime);

        // 원거리 공격 생성 및 위치, 회전 초기화
        PlayerBullet bullet = (PlayerBullet)PlayerBulletFactory.Instance.GetProduct(
            damage, attackData.maxFlyTime, attackData.bulletSpeed);
        bullet.transform.position = bulletPos.position;
        // 회전 부분은 플레이어가 flip 되었을때 rotation으로 도는게 아닌 scale로 반전시켜서 수정이 필요함
        bullet.transform.rotation = bulletPos.rotation;
        if (!playerMovement.isFacingRight) bullet.transform.Rotate(Vector3.up, 180f, Space.Self);

        HideChargeBar();
    }

    private void Update()
    {
        if (!isCharging) return;

        float holdTime = Time.time - holdStartTime;
        if (holdTime < attackData.rangeAttackHoldMinTime)
        {
            // 차지 시작 시간이 되기 전에 점프했다면 차지 안되게
            if (isJumping) isCharging = false;
            return;
        }

        EnableChargeBar(holdTime);
    }

    private void EnableChargeBar(float holdTime)
    {
        chargeBarPresenter.EnableChageBar();
        float chargeBarValue = Mathf.Lerp(0, 1,
        holdTime - attackData.rangeAttackHoldMinTime /
        attackData.rangeAttackHoldMaxTime - attackData.rangeAttackHoldMinTime);
        chargeBarPresenter.UpdateChargeBar(chargeBarValue);

        // 범위 활성화
        rangeAttackSquareObject.SetActive(true);
        rangeAttackSquareObject.transform.localScale = new Vector3(playerMovement.isFacingRight ? 1f : -1f, 1f, 1f);
    }

    private void HideChargeBar()
    {
        chargeBarPresenter.DisableChageBar();
        chargeBarPresenter.UpdateChargeBar(0);

        // 범위 활성화
        rangeAttackSquareObject.SetActive(false);
    }

    private void OnDamaged(float damage, DamageSource source = DamageSource.UNKNOWN)
    {
        isCharging = false;
        HideChargeBar();
    }
}
