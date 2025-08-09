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

        Vector3 dashDirection = enemyTransform.localScale.x > 0 ? Vector3.right : Vector3.left;
        dashEndPosition = enemyTransform.position + dashDirection * patternData.attackRange;

    }

    public override void Update()
    {
        base.Update();
        if (false == isPatternReady)
        {
            return;
        }

        base.MoveTowards(dashEndPosition, patternData.moveSpeed);

        SetRaysParameters();

        Vector2 boxSize = new Vector2(boundsWidth * 1.1f, boundsHeight * 0.9f);

        RaycastHit2D hit = MyDebug.BoxCast(
            boundsCenter,
            boxSize,
            0f,
            Vector2.zero,
            0f,
            LayerManager.obstacleLayerMask,
            Color.red,
            true
        );

        if (hit.collider != null || Vector2.Distance(dashEndPosition, enemyTransform.position) < 1f)
        {
            if (base.m_targetHealth == null && base.IsTargetInRange(base.m_collider.bounds.size))
            {
                base.m_targetHealth = base.target.GetComponent<Health>();
            }

            if (base.m_targetHealth != null)
            {
                base.m_targetHealth.Damage(patternData.damage, patternData.invincibilityDuration);
            }

            m_hasAttacked = true;
        }
    }

    public override bool isFinished()
    {
        return m_hasAttacked || base.controller.patternExecutionTimer <= 0;
    }

    private void SetRaysParameters()
    {
        float x = m_collider.bounds.size.x;
        float y = m_collider.bounds.size.y;

        float right = x * 0.5f;
        float left = -x * 0.5f;
        float top = y * 0.5f;
        float bottom = -y * 0.5f;


        boundsCenter = m_collider.bounds.center;

        boundsTopLeftCorner.x = left;
        boundsTopLeftCorner.y = top;

        boundsBottomLeftCorner.x = left;
        boundsBottomLeftCorner.y = bottom;

        boundsTopRightCorner.x = right;
        boundsTopRightCorner.y = top;

        boundsBottomRightCorner.x = right;
        boundsBottomRightCorner.y = bottom;

        boundsTopLeftCorner = enemyTransform.TransformPoint(boundsTopLeftCorner);
        boundsBottomLeftCorner = enemyTransform.TransformPoint(boundsBottomLeftCorner);
        boundsTopRightCorner = enemyTransform.TransformPoint(boundsTopRightCorner);
        boundsBottomRightCorner = enemyTransform.TransformPoint(boundsBottomRightCorner);

        boundsWidth = Vector2.Distance(boundsTopLeftCorner, boundsTopRightCorner);
        boundsHeight = Vector2.Distance(boundsTopLeftCorner, boundsBottomLeftCorner);
    }
}
