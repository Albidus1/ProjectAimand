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

    private void Awake()
    {
        if (chargeBarPresenter == null)
        {
            chargeBarPresenter = FindFirstObjectByType<ChargeBarPresenter>();
            if (chargeBarPresenter == null) Debug.LogError("차지바 presenter를 찾을 수 없습니다.");
        }
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
        if (this.transform.localScale.x < 0) bullet.transform.Rotate(Vector3.up, 180f, Space.Self);

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

        chargeBarPresenter.EnableChageBar();
        float chargeBarValue = Mathf.Lerp(0, 1,
        holdTime - attackData.rangeAttackHoldMinTime /
        attackData.rangeAttackHoldMaxTime - attackData.rangeAttackHoldMinTime);
        chargeBarPresenter.UpdateChargeBar(chargeBarValue);
    }

    private void HideChargeBar()
    {
        chargeBarPresenter.DisableChageBar();
        chargeBarPresenter.UpdateChargeBar(0);
    }

    private void OnDamaged(float damage)
    {
        isCharging = false;
        HideChargeBar();
    }
}
