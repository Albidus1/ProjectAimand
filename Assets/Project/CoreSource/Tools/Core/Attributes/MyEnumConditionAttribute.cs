using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif



[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class MyEnumConditionAttribute : PropertyAttribute
{
    public string conditionEnum = "";
    public bool hidden = false;

    BitArray bitArray = new BitArray(32);
    public bool ContainsBitFlag(int _enumFlag)
    {
        return bitArray.Get(_enumFlag);
    }

    public MyEnumConditionAttribute(string _conditionBoolean, params int[] _enumValues)
    {
        conditionEnum = _conditionBoolean;
        hidden = true;

        foreach (int enumValue in _enumValues)
        {
            bitArray.Set(enumValue, true);
        }
    }
}
