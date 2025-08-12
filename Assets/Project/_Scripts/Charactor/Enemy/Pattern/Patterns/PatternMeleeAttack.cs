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
        MoveTowards(base.target.position, patternData.moveSpeed);

        if (false == m_hasAttacked && base.IsTargetInRange(base.patternData, LayerManager.playerLayerMask))
        {
            if (base.m_targetHealth == null)
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
        return m_hasAttacked;
    }
}
