using UnityEngine;


public interface IEnemyPattern
{
    string patternID { get; }
    void Initialization(EnemyPatternController _controller, EnemyPattern _data);
    void Execute();
    void Update();
    void Finish();
    bool isFinished();
}

public abstract class EnemyPatternBase : IEnemyPattern
{
    protected EnemyPatternController controller;
    protected EnemyPattern patternData;
    protected Transform target;
    protected Transform enemyTransform;
    protected Health m_health;
    protected Health m_targetHealth;
    protected Collider2D m_collider;

    protected bool isPatternReady;
    protected float preparationTimer;


    public string patternID => patternData.patternID;

    public virtual void Initialization(EnemyPatternController _controller, EnemyPattern _data)
    {
        controller = _controller;
        patternData = _data;
        target = _controller.target;
        enemyTransform = _controller.transform;

        preparationTimer = patternData.preparationTime;
        isPatternReady = false;

        if (m_collider == null)
        {
            m_collider = enemyTransform.gameObject.GetComponent<Collider2D>();
        }
    }

    public virtual void Execute() { }
    public virtual void Update() 
    {
        preparationTimer -= Time.deltaTime;

        if (preparationTimer <= 0f && false == isPatternReady)
        {
            //Debug.Log("준비됨");
            isPatternReady = true;
        }
    }
    public virtual void Finish() { }
    public virtual bool isFinished() => true;

    protected void MoveTowards(Vector3 _targetPosition, float _moveSpeed, bool _moveHorizontal = true)
    {
        _targetPosition = _moveHorizontal ? new Vector3(_targetPosition.x, enemyTransform.position.y, 0) : _targetPosition;

        Vector2 direction = (_targetPosition - enemyTransform.position).normalized;

        enemyTransform.transform.Translate(direction * _moveSpeed * Time.deltaTime);
    }

    protected bool InTarget(EnemyPattern _pattern, LayerMask _mask)
    {
        if (_pattern.useColliderBounds)
        {
            return IsTargetInRange(m_collider.bounds.size, _mask);
        }

        return _pattern.rangeType switch
        {
            EnemyPattern.RangeType.Circle   => IsTargetInRange(_pattern.attackRange, _mask),
            EnemyPattern.RangeType.Box      => IsTargetInRange(_pattern.areaEffectSize, _mask),
            _ => false,
        };
    }

    protected bool IsTargetInRange(float _range, LayerMask _mask)
    {
        RaycastHit2D hit = MyDebug.CircleCast(
            enemyTransform.position,
            _range,
            Vector2.zero,
            0f,
            _mask,
            Color.red,
            true);

        return hit.collider != null;
    }

    protected bool IsTargetInRange(Vector2 _size, LayerMask _mask)
    {
        RaycastHit2D hit = MyDebug.BoxCast(
            enemyTransform.position,
            _size,
            0f,
            Vector2.zero,
            0f,
            _mask,
            Color.red,
            true);

        return hit.collider != null;
    }

    protected bool IsTargetInRange(EnemyPattern _pattern, LayerMask _mask)
    {
        Vector2 newPosition =  new Vector2
            (enemyTransform.position.x + _pattern.areaEffectOffset.x * enemyTransform.localScale.x, 
             enemyTransform.position.y + _pattern.areaEffectOffset.y);

        RaycastHit2D hit = MyDebug.BoxCast(
            newPosition,
            _pattern.areaEffectSize,
            0f,
            Vector2.zero,
            0f,
            _mask,
            Color.red,
            true);

        return hit.collider != null;
    }
}
