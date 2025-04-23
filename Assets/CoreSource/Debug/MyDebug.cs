using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq;
using Debug = UnityEngine.Debug;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.RuleTile.TilingRuleOutput;

using System.Drawing;
using Color = UnityEngine.Color;


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

    public static RaycastHit2D BoxCast(Vector2 _rayOriginPoint, Vector2 _boxSize, float _angle, Vector2 _rayDirection, float _rayDistance, LayerMask _mask, Color _color, bool _drawGizmo = false)
    {
        if (_drawGizmo)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, _angle);

            Vector3[] points = new Vector3[8];

            float halfSizeX = _boxSize.x * 0.5f;
            float halfSizeY = _boxSize.y * 0.5f;

            points[0] = rotation * (_rayOriginPoint + (Vector2.left  * halfSizeX) + (Vector2.up * halfSizeY));
            points[1] = rotation * (_rayOriginPoint + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY));
            points[2] = rotation * (_rayOriginPoint + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY));
            points[3] = rotation * (_rayOriginPoint + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY));

            points[4] = rotation * ((_rayOriginPoint + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)) + _rayDistance * _rayDirection);
            points[5] = rotation * ((_rayOriginPoint + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)) + _rayDistance * _rayDirection);
            points[6] = rotation * ((_rayOriginPoint + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)) + _rayDistance * _rayDirection);
            points[7] = rotation * ((_rayOriginPoint + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)) + _rayDistance * _rayDirection);

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

        return Physics2D.BoxCast(_rayOriginPoint, _boxSize, _angle, _rayDirection, _rayDistance, _mask);
    }

    public static RaycastHit2D CircleCast(Vector2 _rayOriginPoint, float _radius, Vector2 _rayDirection, float _rayDistance, LayerMask _mask, Color _color, bool _drawGizmo = false)
    {
        if (true == _drawGizmo)
        {
            int segments = 16;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angleCurrent = Mathf.Deg2Rad * angleStep * i;
                float angleNext = Mathf.Deg2Rad * angleStep * (i + 1);

                Vector2 pointCurrent = new Vector3(Mathf.Cos(angleCurrent), Mathf.Sin(angleCurrent), 0f) * _radius;
                Vector2 pointNext = new Vector3(Mathf.Cos(angleNext), Mathf.Sin(angleNext), 0f) * _radius;

                Vector2 start = _rayOriginPoint + pointCurrent;
                Vector2 direction = pointNext - pointCurrent;

                Debug.DrawRay(start, direction, _color);
            }
        }

        return Physics2D.CircleCast(_rayOriginPoint, _radius, _rayDirection, _rayDistance, _mask);
    }
    #endregion
}

