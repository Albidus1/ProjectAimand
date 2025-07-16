using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(BossAbilityHandControl), true)]
[InitializeOnLoad]
public class BossAbilityHandControlEditor : Editor
{
    public BossAbilityHandControl targetHandControl
    {
        get
        {
            return (BossAbilityHandControl)target;
        }
    }

    public int patternIndex = 0;
    public bool doChange = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        patternIndex = EditorGUILayout.IntField("수정할 패턴 인덱스", patternIndex);
        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(patternIndex < 0 || patternIndex >= targetHandControl.patterns.Count))
        {
            if (GUILayout.Button($"패턴_{patternIndex} 수정"))
            {
                doChange = true;

                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button($"수정 완료"))
            {
                doChange = false;

                GUIUtility.ExitGUI();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        BossAbilityHandControl t = (BossAbilityHandControl)target;

        Vector3 snap = new Vector3(0.25f, 0.25f, 0);

        if (doChange)
        {
            var pattern = t.patterns[patternIndex];
            for (int i = 0; i < pattern.moveElements.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 oldPoint = pattern.moveElements[i].movePosition;
                GUIStyle style = new GUIStyle();

                style.normal.textColor = Color.yellow;
                Handles.Label(oldPoint + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

                Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Free Move Handle");
                    pattern.moveElements[i].movePosition = newPoint;
                }
            }
        }

        for (int i = 0; i < t.moveLeftElements.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 oldPoint = t.moveLeftElements[i].movePosition;
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.yellow;
            Handles.Label(oldPoint + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Free Move Handle");
                t.moveLeftElements[i].movePosition = newPoint;
            }
        }

        for (int i = 0; i < t.moveRightElements.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 oldPoint = t.moveRightElements[i].movePosition;
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.yellow;
            Handles.Label(oldPoint + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Free Move Handle");
                t.moveRightElements[i].movePosition = newPoint;
            }
        }
    }
}
