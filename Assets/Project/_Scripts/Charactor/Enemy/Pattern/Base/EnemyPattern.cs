using UnityEngine;



public enum PatternType
{
    Patrol,
    MeleeAttack,
    RangedAttack,
    Dash,
    AreaEffect,
    UnitDead
}

[CreateAssetMenu(fileName = "적 패턴", menuName = "AI/EnemyPattern")]
public class EnemyPattern : ScriptableObject
{
    public enum RangeType
    {
        Circle,
        Box,
    }

    public string patternID;
    public PatternType patternType;
    [Tooltip("우선도")]
    public int priority = 1;
    public float minTriggerDistance = 0f;
    public float maxTriggerDistance = 5f;

    [Header("쿨타임")]
    [Tooltip("패턴 준비 시간. 0이면 즉시 실행됨.")]
    public float preparationTime = 1f;
    [Tooltip("패턴 실행 후 쿨타임. 0이면 즉시 재사용 가능.")]
    public float cooldown = 3f;
    [Tooltip("패턴 실행 시간")]
    public float executionTime = 2f;

    [Header("이동")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("공격")]
    public float damage = 10f;
    [Tooltip("공격이 적중한 후 적에게 주는 무적 시간")]
    public float invincibilityDuration = 0.5f;

    [Header("범위")]
    public float attackRange = 1.5f;
    public bool useColliderBounds = false;
    [MyConditionalHide("useColliderBounds", true, true)]
    public RangeType rangeType = RangeType.Circle;
    [MyConditionalHide("useColliderBounds", true, true)]
    public Vector2 areaEffectSize;
    [MyConditionalHide("useColliderBounds", true, true)]
    public Vector2 areaEffectOffset = Vector2.zero;

    [Header("풀링")]
    public string poolName;
    public Vector2 spawnFaceDirection = Vector2.up;
    public int projectilePerShot = 3;
    public float attackInterval = 0.5f;

    [Header("애니메이션")]
    public string animationTrigger;
    public string animationBool;

}
