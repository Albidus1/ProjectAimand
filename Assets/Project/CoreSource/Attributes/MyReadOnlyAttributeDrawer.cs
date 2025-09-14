#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(MyReadOnlyAttribute))]
public class MyReadOnlyAttributeDrawer : PropertyDrawer
{

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(_position, _property, _label, true);
        GUI.enabled = true;
    }


    public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
    {
        return EditorGUI.GetPropertyHeight(_property, _label);
    }
}
#endif