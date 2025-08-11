using UnityEngine;



public enum PatternType
{
    Patrol,
    MeleeAttack,
    RangeAttack,
    Dash,
    AreaEffect,
}

[CreateAssetMenu(fileName = "적 패턴", menuName = "AI/EnemyPattern")]
public class EnemyPattern : ScriptableObject
{
    public string patternID;
    public PatternType patternType;

    public int priority = 1;
    public float minTriggerDistance = 0f;
    public float maxTriggerDistance = 5f;

    [Header("쿨타임")]
    public float preparationTime = 1f;
    public float cooldown = 3f;
    public float executionTime = 2f;

    [Header("이동")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("공격")]
    public float damage = 10f;
    public float attackRange = 1.5f;
    public float invincibilityDuration = 0.5f;

    [Header("범위")]
    public Vector2 areaEffectSize;
    public Vector2 areaEffectOffset = Vector2.zero;

    [Header("애니메이션")]
    public string animationTrigger;

}
