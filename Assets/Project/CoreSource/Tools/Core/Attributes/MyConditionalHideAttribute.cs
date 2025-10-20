using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class MyConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSoureField = "";
    public bool hideInInspector = false;
    public bool negative = false;



    public MyConditionalHideAttribute(string _name, bool _hide = false)
    {
        conditionalSoureField = _name;
        hideInInspector = _hide;
        negative = false;
    }

    public MyConditionalHideAttribute(string _name, bool _hide, bool _negative)
    {
        conditionalSoureField = _name;
        hideInInspector = _hide;
        negative = _negative;
    }
}
