using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PlatformMoving), true)]
[InitializeOnLoad]
public class PlatformMovingEditor : Editor
{
    public PlatformMoving pathTarget
    {
        get
        {
            return (PlatformMoving)target;
        }
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObjects(targets, "Reset Platform Positions");

        if (GUILayout.Button("위치 재설정"))
        {
            PlatformMoving t = (PlatformMoving)target;

            if (t.pathElements[0].pathElementPosition != Vector3.zero)
            {
                Vector3 pathPosition_0 = t.pathElements[0].pathElementPosition;
                t.transform.position += pathPosition_0;

                foreach (MyPathMovementElement element in t.pathElements)
                {
                    element.pathElementPosition -= pathPosition_0;
                }
            }

            EditorUtility.SetDirty(t);
        }

        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        PlatformMoving t = (target as PlatformMoving);

        Vector3 snap = new Vector3(0.25f, 0.25f, 0.25f);

        for (int i = 0; i < t.pathElements.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 oldPoint = t.originalTransformPosition + t.pathElements[i].pathElementPosition;
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.yellow;
            Handles.Label(
                t.originalTransformPosition +
                t.pathElements[i].pathElementPosition +
                (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);
            newPoint = ApplyAxisLock(oldPoint, newPoint);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Free Move Handle");
                t.pathElements[i].pathElementPosition = newPoint - t.originalTransformPosition;
            }
        }
    }

    private Vector3 ApplyAxisLock(Vector3 _oldPoint, Vector3 _newPoint)
    {
        PlatformMoving t = (target as PlatformMoving);

        if (t.LockHandlesOnXAxis)
        {
            _newPoint.x = _oldPoint.x;
        }
        if (t.LockHandlesOnYAxis)
        {
            _newPoint.y = _oldPoint.y;
        }
        if (t.LockHandlesOnZAxis)
        {
            _newPoint.z = _oldPoint.z;
        }

        return _newPoint;
    }
}