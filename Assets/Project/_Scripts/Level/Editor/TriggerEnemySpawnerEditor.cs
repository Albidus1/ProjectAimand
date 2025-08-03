#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(TriggerEnemySpawner), true)]
[InitializeOnLoad]
public class TriggerEnemySpawnerEditor : Editor
{
    public TriggerEnemySpawner enemySpawner
    {
        get
        {
            return (TriggerEnemySpawner)target;
        }
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        TriggerEnemySpawner t = (target as TriggerEnemySpawner);

        Vector3 snap = new Vector3(0.25f, 0.25f, 0);

        for (int i = 0; i < t.enemyList.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 oldPoint = t.transform.position + t.enemyList[i].spawnPosition;
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.yellow;
            Handles.Label(
                t.transform.position +
                t.enemyList[i].spawnPosition +
                (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

            Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, 0.5f, snap, Handles.CircleHandleCap);
            newPoint = ApplyAxisLock(oldPoint, newPoint);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Free Move Handle");
                t.enemyList[i].spawnPosition = newPoint - t.transform.position;
            }
        }
    }

    private Vector3 ApplyAxisLock(Vector3 _oldPoint, Vector3 _newPoint)
    {
        //_newPoint.x = _oldPoint.x;
        //_newPoint.y = _oldPoint.y;
        _newPoint.z = 0;

/*        TriggerEnemySpawner t = (target as TriggerEnemySpawner);

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
        }*/

        return _newPoint;
    }
}
#endif