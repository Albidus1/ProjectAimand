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

    #region CASTs
    public static RaycastHit2D Raycast(Vector2 _rayOriginPoint, Vector2 _rayDirection, float _rayDistance, LayerMask _mask, Color _color, bool _drawGizmo = false)
    {
        if (true == _drawGizmo)
        {
            Debug.DrawRay(_rayOriginPoint, _rayDirection * _rayDistance, _color);
        }

        return Physics2D.Raycast(_rayOriginPoint, _rayDirection, _rayDistance, _mask);
    }

    public static RaycastHit2D BoxCast(Vector2 _origin, Vector2 _size, float _angle, Vector2 _direction, float _length, LayerMask _mask, Color _color, bool _drawGizmo = false)
    {
        if (_drawGizmo)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, _angle);

            Vector3[] points = new Vector3[8];

            float halfSizeX = _size.x * 0.5f;
            float halfSizeY = _size.y * 0.5f;

            points[0] = rotation * (_origin + (Vector2.left  * halfSizeX) + (Vector2.up * halfSizeY));
            points[1] = rotation * (_origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY));
            points[2] = rotation * (_origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY));
            points[3] = rotation * (_origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY));

            points[4] = rotation * ((_origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)) + _length * _direction);
            points[5] = rotation * ((_origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)) + _length * _direction);
            points[6] = rotation * ((_origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)) + _length * _direction);
            points[7] = rotation * ((_origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)) + _length * _direction);

            Debug.DrawLine(points[0], points[1], _color);
            Debug.DrawLine(points[1], points[2], _color);
            Debug.DrawLine(points[2], points[3], _color);
            Debug.DrawLine(points[3], points[0], _color);

            Debug.DrawLine(points[4], points[5], _color);
            Debug.DrawLine(points[5], points[6], _color);
            Debug.DrawLine(points[6], points[7], _color);
            Debug.DrawLine(points[7], points[4], _color);

            Debug.DrawLine(points[0], points[4], _color);
            Debug.DrawLine(points[1], points[5], _color);
            Debug.DrawLine(points[2], points[6], _color);
            Debug.DrawLine(points[3], points[7], _color);
        }

        return Physics2D.BoxCast(_origin, _size, _angle, _direction, _length, _mask);
    }
    #endregion
}

