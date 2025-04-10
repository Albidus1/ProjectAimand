using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


namespace CodeGraph.Editor
{
    [CustomEditor(typeof(CodeGraphAsset))]
    public class CodeGraphAssetEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int _instanceId, int _index)
        {
            Object asset = EditorUtility.InstanceIDToObject(_instanceId);
            if (asset.GetType() == typeof(CodeGraphAsset))
            {
                CodeGraphEditorWindow.Open((CodeGraphAsset)asset);
                return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open"))
            {
                CodeGraphEditorWindow.Open(target as CodeGraphAsset);

            }
        }
    }
}
