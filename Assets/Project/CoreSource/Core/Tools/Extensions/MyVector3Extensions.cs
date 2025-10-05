using UnityEngine;



public static class MyVector3Extensions
{
    public static Vector3 MySetX(this Vector3 _vector, float _newValue)
    {
        _vector.x = _newValue;
        return _vector;
    }

    public static Vector3 MySetY(this Vector3 _vector, float _newValue)
    {
        _vector.y = _newValue;
        return _vector;
    }

    public static Vector3 MySetZ(this Vector3 _vector, float _newValue)
    {
        _vector.z = _newValue;
        return _vector;
    }
}
