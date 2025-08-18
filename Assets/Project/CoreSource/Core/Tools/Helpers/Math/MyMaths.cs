using UnityEngine;



public static class MyMaths
{
    public static Quaternion LookAt2D(Vector2 _direction)
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle, _direction);
    }

    /// <summary>
    /// 값 범위 변환 >> 어떤 값 X가 (A ~ B) 범위에 있을 때, (C ~ D) 범위로 변환
    /// </summary>
    /// <param name="_value">변환 할 값</param>
    /// <param name="_a">기존 범위1</param>
    /// <param name="_b">기존 범위2</param>
    /// <param name="_c">새 범위3</param>
    /// <param name="_d">새 범위4</param>
    /// <returns></returns>
    public static float Remap(float _value, float _a, float _b, float _c, float _d)
    {
        return _c + (_value - _a) / (_b - _a) * (_d - _c);
    }
}
