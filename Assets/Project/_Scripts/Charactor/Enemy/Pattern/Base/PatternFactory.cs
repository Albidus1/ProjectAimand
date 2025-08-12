using System.Collections.Generic;
using UnityEngine;

public static class PatternFactory
{
    private static readonly Dictionary<PatternType, System.Type> m_patternType = new Dictionary<PatternType, System.Type>
    {
        { PatternType.MeleeAttack,  typeof(PatternMeleeAttack) },
        { PatternType.Dash,         typeof(PatternDashStrike) },
        { PatternType.RangedAttack, typeof(PatternRangedAttack) },

        // 패턴 추가
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

public class PatternCooldownTracker
{
    private Dictionary<string, float> m_cooldownTimers = new Dictionary<string, float>();



    public void InitializeCooldowns(EnemyPattern _patterns)
    {
        if (false == m_cooldownTimers.ContainsKey(_patterns.patternID))
        {
            m_cooldownTimers[_patterns.patternID] = _patterns.cooldown;
        }
    }

    public void UpdateCooldowns(float _deltaTime)
    {
        List<string> keys = new List<string>(m_cooldownTimers.Keys);

        foreach (var key in keys)
        {
            m_cooldownTimers[key] -= _deltaTime;
        }
    }

    public void StartCooldown(string _type, float _cooldownDuration)
    {
        if (m_cooldownTimers.ContainsKey(_type))
        {
            m_cooldownTimers[_type] = _cooldownDuration;
        }
    }

    public bool IsCooldownReady(string _type)
    {
        return m_cooldownTimers.ContainsKey(_type) && m_cooldownTimers[_type] <= 0f;
    }

    public float GetRemainingCooldown(string _type)
    {
        return m_cooldownTimers.ContainsKey(_type) ? m_cooldownTimers[_type] : 0f;
    }

    public float GetCooldownProgress(string _type)
    {
        if (false == m_cooldownTimers.ContainsKey(_type) || m_cooldownTimers[_type] <= 0f)
        {
            return 1f;
        }

        return 0f;
    }
}