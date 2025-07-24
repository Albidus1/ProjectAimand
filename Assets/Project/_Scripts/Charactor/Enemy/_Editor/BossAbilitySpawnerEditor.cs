using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[CustomEditor(typeof(BossAbilitySpawner))]
[InitializeOnLoad]
public class BossAbilitySpawnerEditor : Editor
{
    public BossAbilitySpawner targetSpawner
    {
        get
        {
            return (BossAbilitySpawner)target;
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        BossAbilitySpawner t = (BossAbilitySpawner)target;

        Vector3 snap = new Vector3(0.25f, 0.25f, 0);

        for (int i = 0; i < t.spawnPoint.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 oldPoint = t.spawnPoint[i];
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.yellow;
            Handles.Label(oldPoint + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Free Move Handle");
                t.spawnPoint[i] = newPoint;
            }
        }
    }
}
#endif