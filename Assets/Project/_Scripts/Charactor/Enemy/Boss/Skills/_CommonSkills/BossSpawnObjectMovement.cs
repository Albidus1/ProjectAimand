using DG.Tweening;
using UnityEngine;

public class BossSpawnObjectMovement : BossSkillBase
{
    [Header("스폰 오브젝트 이동 설정")]
    public Ease moveEase = Ease.Linear;
    public float moveSpeed = 5f;
    public Vector2 moveDirection = Vector2.down;



    protected override void OnEnable()
    {

    }

    public override void UseSkill()
    {
        transform.DOMove((Vector2)transform.position + moveDirection, moveSpeed)
            .SetEase(moveEase)
            .OnComplete(() => this.gameObject.SetActive(false));
    }
}
