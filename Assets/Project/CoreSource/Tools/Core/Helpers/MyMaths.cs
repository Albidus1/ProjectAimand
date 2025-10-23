using UnityEngine;



public static class MyMaths
{
    public static float SpringVelocity(float _currentValue, float _targetValue, float _velocity, 
        float _damping, float _frequency, float _speed, float _deltaTime)
    {
        float _maxDeltaTime = Mathf.Min(1f / (_frequency * 10f), _deltaTime);
        _frequency = _frequency * 2f * Mathf.PI;
        _deltaTime = Mathf.Min(_deltaTime, _maxDeltaTime);

        return 
            _velocity + 
            (_deltaTime * _frequency * _frequency * (_targetValue - _currentValue) + 
            (-2f * _deltaTime * _frequency * _damping * _velocity));
    }

    public static void Spring(ref Vector3 _currentValue, Vector3 _targetValue, ref Vector3 _velocity, float _damping, float _frequency, float _speed, float _deltaTime)
    {
        Vector3 initialVelocity = _velocity;
        _velocity.x = SpringVelocity(_currentValue.x, _targetValue.x, _velocity.x, _damping, _frequency, _speed, _deltaTime);
        _velocity.y = SpringVelocity(_currentValue.y, _targetValue.y, _velocity.y, _damping, _frequency, _speed, _deltaTime);
        _velocity.z = SpringVelocity(_currentValue.z, _targetValue.z, _velocity.z, _damping, _frequency, _speed, _deltaTime);
        _velocity.x = MyMaths.Lerp(initialVelocity.x, _velocity.x, _speed, Time.deltaTime);
        _velocity.y = MyMaths.Lerp(initialVelocity.y, _velocity.y, _speed, Time.deltaTime);
        _velocity.z = MyMaths.Lerp(initialVelocity.z, _velocity.z, _speed, Time.deltaTime);
        _currentValue += _deltaTime * _velocity;
    }

    private static float LerpRate(float _rate, float _deltaTime)
    {
        _rate = Mathf.Clamp01(_rate);
        float invRate = -Mathf.Log(1.0f - _rate, 2.0f) * 60f;

        return Mathf.Pow(2.0f, -invRate * _deltaTime);
    }

    public static float Lerp(float _value, float _target, float _rate, float _deltaTime)
    {
        if (_deltaTime == 0f) 
        { 
            return _value; 
        }

        return Mathf.Lerp(_target, _value, LerpRate(_rate, _deltaTime));
    }

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

    /// <summary>
    /// 점을 선에 수직으로 내렸을 때의 거리 반환
    /// </summary>
    /// <param name="_point"></param>
    /// <param name="_lineStart"></param>
    /// <param name="_lineEnd"></param>
    /// <returns></returns>
    public static float DistanceBetweenPointAndLine(Vector3 _point, Vector3 _lineStart, Vector3 _lineEnd)
    {
        Vector3 rhs = _point - _lineStart;
        Vector3 vector2 = (_lineEnd - _lineStart);
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;

        if (magnitude > 1E-06f)
        {
            lhs /= magnitude;
        }

        float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude); 
        float distance = Vector3.Magnitude(_lineStart + (Vector3)(lhs * num2));

        return distance;
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
