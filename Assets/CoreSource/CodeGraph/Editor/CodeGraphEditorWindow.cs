using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CodeGraph.Editor
{
    public class CodeGraphEditorWindow : EditorWindow
    {
        public CodeGraphAsset currentGraph => m_currentGraph;

        [SerializeField] private CodeGraphAsset     m_currentGraph;
        [SerializeField] private SerializedObject   m_serializedObject;
        [SerializeField] private CodeGraphView      m_currentView;



        private void OnEnable()
        {
            if (m_currentGraph != null)
            {
                DrawGraph();
            }
        }

        private void OnGUI()
        {
            if (m_currentGraph != null)
            {
                if (EditorUtility.IsDirty(m_currentGraph))
                {
                    this.hasUnsavedChanges = true;
                }
                else
                {
                    this.hasUnsavedChanges = false;
                }
            }
        }

        public static void Open(CodeGraphAsset _target)
        {
            CodeGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<CodeGraphEditorWindow>();

            foreach (CodeGraphEditorWindow w in windows)
            {
                if (w.currentGraph == _target)
                {
                    w.Focus();
                    return;
                }
            }

            CodeGraphEditorWindow window = CreateWindow<CodeGraphEditorWindow>(typeof(CodeGraphEditorWindow), typeof(SceneView));

            window.titleContent = new GUIContent($"{_target.name}", EditorGUIUtility.ObjectContent(null, typeof(CodeGraphAsset)).image);
            window.Load(_target);
        }

        public void Load(CodeGraphAsset _target)
        {
            m_currentGraph = _target;
            DrawGraph();
        }

        private void DrawGraph()
        {
            m_serializedObject = new SerializedObject(m_currentGraph);
            m_currentView = new CodeGraphView(m_serializedObject, this);
            m_currentView.graphViewChanged += OnChange;
            rootVisualElement.Add(m_currentView);
        }

        private GraphViewChange OnChange(GraphViewChange _graphViewChange)
        {
            EditorUtility.SetDirty(m_currentGraph);
            return _graphViewChange;
        }

        public override void SaveChanges()
        {
            //Debug.Log("변경 저장");
        }
    }
}
