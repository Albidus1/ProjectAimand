using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(PlatformDoor), true)]
[InitializeOnLoad]
public class MyPathEditor2 : Editor
{
    static MyPathEditor2()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        PlatformDoor[] platforms = FindObjectsByType<PlatformDoor>(FindObjectsSortMode.None);

        foreach (var platform in platforms)
        {
            if (false == platform.Initialized)
            {
                platform.InitializePoints();
                EditorUtility.SetDirty(platform);
            }
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        PlatformDoor platform = (PlatformDoor)target;
   
        Vector3 gridOffset = new Vector3(1, 1, 0);

        // pointA의 위치 변경 가능
        EditorGUI.BeginChangeCheck();
        Vector3 new_pointA_position = Handles.FreeMoveHandle(
            platform.pointA,
            1f,
            gridOffset,
            Handles.CircleHandleCap);


        bool isshift = Input.GetKey(KeyCode.LeftShift);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(platform, "Move Point A");
            platform.transform.position = new_pointA_position;
            platform.pointA = new_pointA_position;
            platform.DirectionCalculate(true);
            EditorUtility.SetDirty(platform);
        }

        
        // pointB의 위치 변경 가능
        EditorGUI.BeginChangeCheck();
        Vector3 new_pointB_position = Handles.FreeMoveHandle(
            platform.pointB,
            1f,
            gridOffset,
            Handles.CircleHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(platform, "Move Point B");
            platform.pointB = new_pointB_position;
            platform.DirectionCalculate(true);
            EditorUtility.SetDirty(platform);
        }
    }
}
