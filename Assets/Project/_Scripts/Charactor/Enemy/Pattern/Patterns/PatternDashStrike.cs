using Unity.VisualScripting;
using UnityEngine;



public class PatternDashStrike : EnemyPatternBase
{
    private bool m_hasAttacked;

    private Vector2 dashEndPosition;
    protected Vector2 bounds;
    protected Vector2 boundsCenter;
    protected Vector2 boundsTopLeftCorner;
    protected Vector2 boundsBottomLeftCorner;
    protected Vector2 boundsTopRightCorner;
    protected Vector2 boundsBottomRightCorner;
    protected float boundsWidth;
    protected float boundsHeight;


    public override void Execute()
    {
        m_hasAttacked = false;

        if (target != null)
        {
            Vector3 dashDirection = enemyTransform.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
            dashEndPosition = enemyTransform.position + dashDirection * patternData.attackRange;
        }

        base.PlayAnimation();
    }

    public override void Update()
    {
        base.Update();
        if (false == isPatternReady)
        {
            return;
        }

        base.MoveTowards(dashEndPosition, patternData.moveSpeed);

        //Vector2 boxSize = new Vector2(patternData.areaEffectSize.x, patternData.areaEffectSize.y);

        //RaycastHit2D hit = MyDebug.BoxCast(
        //    enemyTransform.position,
        //    boxSize,
        //    0f,
        //    Vector2.zero,
        //    0f,
        //    LayerManager.playerLayerMask | LayerManager.obstacleLayerMask,
        //    Color.red,
        //    true
        //);

        if (base.InTarget(patternData, LayerManager.playerLayerMask | LayerManager.obstacleLayerMask) || Mathf.Abs(dashEndPosition.x - enemyTransform.position.x) < 0.1f)
        {
            if (base.m_targetHealth == null)
            {
                base.m_targetHealth = base.target.GetComponent<Health>();
            }

            if (base.m_targetHealth != null && InTarget(patternData, LayerManager.playerLayerMask))
            {
                base.m_targetHealth.Damage(patternData.damage, patternData.invincibilityDuration);
            }

            m_hasAttacked = true;
        }
    }

    public override bool isFinished()
    {
        base.StopAnimation();
        return m_hasAttacked || Mathf.Abs(dashEndPosition.x - enemyTransform.position.x) < 0.1f;
    }
}
