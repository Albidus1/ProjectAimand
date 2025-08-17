using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[CustomEditor(typeof(EnemyWave))]
[InitializeOnLoad]
public class EnemyWaveEditor : Editor
{
    public EnemyWave targetWave
    {
        get
        {
            return (EnemyWave)target;
        }
    }

    public int waveIndex = 0;
    public bool doChange = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        waveIndex = EditorGUILayout.IntField("수정할 인덱스", waveIndex);
        EditorGUILayout.Space();

        bool exitGUIRequested = false;

        using (new EditorGUI.DisabledScope(waveIndex < 0 || waveIndex >= targetWave.waves.Count))
        {
            if (GUILayout.Button($"웨이브_{waveIndex} 수정"))
            {
                doChange = true;
                exitGUIRequested = true;
            }

            if (GUILayout.Button($"수정 완료"))
            {
                doChange = false;
                exitGUIRequested = true;
            }
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector();
        
        serializedObject.ApplyModifiedProperties();

        if (exitGUIRequested)
        {
            GUIUtility.ExitGUI();
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        EnemyWave t = (EnemyWave)target;

        Vector3 snap = new Vector3(0.25f, 0.25f, 0);

        if (doChange)
        {
            var wave = t.waves[waveIndex];
            for (int i = 0; i < wave.enemyInfo.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 oldPoint = wave.enemyInfo[i].spawnPosition;
                GUIStyle style = new GUIStyle();

                style.normal.textColor = Color.yellow;
                Handles.Label(oldPoint + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i + $" {wave.enemyInfo[i].enemyPrefab.name}", style);

                Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Free Move Handle");
                    wave.enemyInfo[i].spawnPosition = newPoint;
                }
            }
        }
    }
}
#endif