using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif



public static class MyDebug
{
    #region DEBUG_DRAW
    public static void DrawGizmoArrow(Vector2 _origin, Vector2 _direction, Color _color, float _arrowHeadLength = 3f, float _arrowHeadAngle = 25f)
    {
        Gizmos.color = _color;
        Gizmos.DrawRay(_origin, _direction);

        DrawArrowEnd(_origin, _direction, _color, _arrowHeadLength, _arrowHeadAngle);
    }

    private static void DrawArrowEnd(Vector3 _arrowEndPosition, Vector3 _direction, Color _color, float _arrowHeadLength = 3f, float _arrowHeadAngle = 25f)
    {
        if (_direction == Vector3.zero)
        {
            return;
        }

        Vector3 right = Quaternion.LookRotation(_direction) * Quaternion.Euler(_arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(_direction) * Quaternion.Euler(-_arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(_direction) * Quaternion.Euler(0, _arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(_direction) * Quaternion.Euler(0, -_arrowHeadAngle, 0) * Vector3.back;

        Gizmos.color = _color;
        Gizmos.DrawRay(_arrowEndPosition + _direction, right * _arrowHeadLength);
        Gizmos.DrawRay(_arrowEndPosition + _direction, left * _arrowHeadLength);
        Gizmos.DrawRay(_arrowEndPosition + _direction, up * _arrowHeadLength);
        Gizmos.DrawRay(_arrowEndPosition + _direction, down * _arrowHeadLength);
    }
    #endregion
}

