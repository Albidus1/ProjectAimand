using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using System.Reflection;


[System.AttributeUsage(System.AttributeTargets.Field)]
public class MyInspectorButtonBarAttribute : PropertyAttribute
{
    public string[] labels { get; set; }
    public string[] methods { get; set; }
    public bool[] onlyWhenPlaying { get; set; }
    public string[] useClass { get; set; }


    public MyInspectorButtonBarAttribute(string[] _labels, string[] _methods, bool[] _onlyWhenPlaying, string[] _useClass)
    {
        labels = _labels;
        methods = _methods;
        onlyWhenPlaying = _onlyWhenPlaying;
        useClass = _useClass;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MyInspectorButtonBarAttribute))]
public class MyInspectorButtonBarAttributeDrawer : PropertyDrawer
{
    private MethodInfo[] m_eventMethodInfos = null;


    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        MyInspectorButtonBarAttribute inspectorButtonBarAttribute = attribute as MyInspectorButtonBarAttribute;
        System.Type eventOwnerType = property.serializedObject.targetObject.GetType();

        var root = new VisualElement();

        Toolbar moveToControls = new Toolbar();
        moveToControls.AddToClassList("my-toolbar");

        if (m_eventMethodInfos == null)
        {
            m_eventMethodInfos = new MethodInfo[inspectorButtonBarAttribute.methods.Length];
        }

        for (var i = 0; i < inspectorButtonBarAttribute.labels.Length; i++)
        {
            var newButton = new ToolbarButton();
            newButton.text = inspectorButtonBarAttribute.labels[i];
            newButton.style.flexGrow = 1;

            if (inspectorButtonBarAttribute.useClass[i] != "")
            {
                newButton.AddToClassList(inspectorButtonBarAttribute.useClass[i]);
            }

            if (m_eventMethodInfos[i] == null)
            {
                m_eventMethodInfos[i] = eventOwnerType.GetMethod(inspectorButtonBarAttribute.methods[i], 
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
            if (m_eventMethodInfos[i] != null)
            {
                var i1 = i;
                newButton.clicked += () => m_eventMethodInfos[i1].Invoke(property.serializedObject.targetObject, null);
            }
            else
            {

            }

            if (inspectorButtonBarAttribute.onlyWhenPlaying[i] && false == Application.isPlaying)
            {
                newButton.SetEnabled(false);
            }

            moveToControls.Add(newButton);
        }

        root.Add(moveToControls);

        return root;
    }
}
#endif 