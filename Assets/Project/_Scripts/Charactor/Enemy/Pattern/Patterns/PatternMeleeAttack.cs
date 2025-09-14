using UnityEngine;

public class PatternMeleeAttack : EnemyPatternBase
{
    private bool m_hasAttacked;



    public override void Execute()
    {
        m_hasAttacked = false;
    }

    public override void Update()
    {
        base.Update();
        if (false == isPatternReady)
        {
            return;
        }


        if (base.IsTargetInRange(patternData.attackRange, LayerManager.playerLayerMask))
        {
            if (false == m_hasAttacked && base.InTarget(base.patternData, LayerManager.playerLayerMask))
            {
                if (base.m_targetHealth == null)
                {
                    base.m_targetHealth = base.target.GetComponent<Health>();
                }

                if (base.m_targetHealth != null)
                {
                    base.m_targetHealth.Damage(patternData.damage, patternData.invincibilityDuration);
                }

                //Debug.Log($"PatternMeleeAttack: {base.patternData.patternID} - Target: {base.target.name} - Damage: {patternData.damage}");
                base.StopAnimation();
                m_hasAttacked = true;
            }
        }
        else
        {
            MoveTowards(base.target.position, patternData.moveSpeed);
        }
    }

    public override bool isFinished()
    {
        base.StopAnimation();

        return m_hasAttacked;
    }
}
