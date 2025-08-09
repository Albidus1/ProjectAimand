using System.Collections.Generic;
using UnityEngine;

public static class PatternFactory
{
    private static readonly Dictionary<PatternType, System.Type> m_patternType = new Dictionary<PatternType, System.Type>
    {
        { PatternType.MeleeAttack, typeof(PatternMeleeAttack) },
        { PatternType.Dash, typeof(PatternDashStrike) },

    };

    public static IEnemyPattern CreatePattern(PatternType _type, EnemyPattern _data)
    {
        if (m_patternType.TryGetValue(_type, out System.Type patternType))
        {
            return (IEnemyPattern)System.Activator.CreateInstance(patternType, _data);
        }

        return null;
    }

#if UNITY_EDITOR
    public static void RegisterNewPatternType(PatternType _newType, System.Type _newPatternType)
    {
        if (false == m_patternType.ContainsKey(_newType))
        {
            m_patternType.Add(_newType, _newPatternType);
        }
    }
#endif
}
