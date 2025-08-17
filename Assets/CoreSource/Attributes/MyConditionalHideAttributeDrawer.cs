using System;
using UnityEditor;
using UnityEngine;



#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(MyConditionalHideAttribute))]
public class MyConditionalHideAttributeDrawer : UnityEditor.PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        MyConditionalHideAttribute conditionAttribute = attribute as MyConditionalHideAttribute;
        bool enabled = GetConditionAttributeResult(conditionAttribute, _property);
        bool previousEnabled = GUI.enabled;
        bool shouldDisplay = ShouldDisplay(conditionAttribute, enabled);

        if (shouldDisplay)
        {
            GUI.enabled = enabled;
            EditorGUI.PropertyField(_position, _property, _label, true);
            GUI.enabled = previousEnabled;
        }
    }

    private bool GetConditionAttributeResult(MyConditionalHideAttribute _conditionAttribute, SerializedProperty _property)
    {
        bool enabled = true;
        string propertyPath = _property.propertyPath;
        string conditionPath = propertyPath.Replace(_property.name, _conditionAttribute.conditionalSoureField);
        SerializedProperty sourcePropertyValue = _property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = sourcePropertyValue.boolValue;
        }
        else
        {
            Debug.LogWarning($"Could not find property '{conditionPath}' for conditional attribute on '{_property.name}'.");
        }

        if (_conditionAttribute.negative)
        {
            enabled = !enabled;
        }

        return enabled;
    }

    private bool ShouldDisplay(MyConditionalHideAttribute _conditionAttribute, bool _enabled)
    {
        return !_conditionAttribute.hideInInspector || _enabled;
    }

    public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
    {
        MyConditionalHideAttribute connAtt = attribute as MyConditionalHideAttribute;
        bool enabled = GetConditionAttributeResult(connAtt, _property);

        bool shouldDisplay = ShouldDisplay(connAtt, enabled);
        if (shouldDisplay)
        {
            return EditorGUI.GetPropertyHeight(_property, _label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif