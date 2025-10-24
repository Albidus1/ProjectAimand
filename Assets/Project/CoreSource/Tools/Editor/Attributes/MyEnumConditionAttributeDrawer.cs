using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MyEnumConditionAttribute))]
public class MyEnumConditionAttributeDrawer : PropertyDrawer
{
    private static Dictionary<string, string> cachedPaths = new Dictionary<string, string>();



    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MyEnumConditionAttribute enumConditionAttribute = (MyEnumConditionAttribute)attribute;

        bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
        bool previousEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (false == enumConditionAttribute.hidden || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = previousEnabled;
    }

    private bool GetConditionAttributeResult(MyEnumConditionAttribute _enumConditionAttribute, SerializedProperty _property)
    {
        bool enabled = true;

        string enumPropPath;
        string propertyPath = _property.propertyPath;
        if (!cachedPaths.TryGetValue(propertyPath, out enumPropPath))
        {
            enumPropPath = propertyPath.Replace(_property.name, _enumConditionAttribute.conditionEnum);
            cachedPaths.Add(propertyPath, enumPropPath);
        }

        SerializedProperty enumProp = _property.serializedObject.FindProperty(enumPropPath);
        if (enumProp != null)
        {
            int currentEnum = enumProp.enumValueIndex;
            enabled = _enumConditionAttribute.ContainsBitFlag(currentEnum);
        }
        else
        {
            Debug.LogWarning("ConditionAttribute에 대해 객체에서 일치하는 bool 값을 찾을 수 없음: " + enumPropPath);
        }

        return enabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        MyEnumConditionAttribute enumConditionAttribute = (MyEnumConditionAttribute)attribute;
        bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
        
        if (false == enumConditionAttribute.hidden || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif
