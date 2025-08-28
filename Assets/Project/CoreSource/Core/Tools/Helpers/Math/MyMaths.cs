using UnityEngine;



public static class MyMaths
{
    public static Quaternion LookAt2D(Vector2 _direction)
    {
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle, _direction);
    }

    /// <summary>
    /// From에서 To까지 Amount만큼 이동하고 해당 값을 반환
    /// </summary>
    public static float Approach(float _from, float _to, float _amount)
    {
        if (Mathf.Approximately(_from, _to))
        { 
            return _from; 
        }

        if (_from < _to)
        {
            _from += _amount;
            if (_from > _to)
            {
                return _to;
            }
        }

        if (_from > _to)
        {
            _from -= _amount;
            if (_from < _to)
            {
                return _to;
            }
        }

        return _from;
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


    public static Vector3 GetRandomPointInCircle(float _radius)
    {
        float theta = Random.Range(0f, 2f * Mathf.PI);

        float r = _radius * Mathf.Sqrt(Random.Range(0f, 1f));

        float x = r * Mathf.Cos(theta);
        float y = r * Mathf.Sin(theta);

        return new Vector3(x, y);
    }
}
