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
            Debug.Log("준비됨");
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

    protected bool IsTargetInRange(float _maxRange, float _minRange = 0f)
    {
        RaycastHit2D hit = MyDebug.CircleCast(
            enemyTransform.position,
            _maxRange,
            Vector2.zero,
            0f,
            LayerManager.playerLayerMask,
            Color.red,
            true);

        if (hit.collider != null)
        {
            target = hit.transform;
        }

        float distance = Vector2.Distance(enemyTransform.position, target.position);

        return distance <= _maxRange && distance >= _minRange;
    }

    protected bool IsTargetInRange(Vector2 _size)
    {
        RaycastHit2D hit = MyDebug.BoxCast(
            enemyTransform.position,
            _size,
            0f,
            Vector2.zero,
            0f,
            LayerManager.playerLayerMask,
            Color.red,
            true);

        return hit.collider != null;
    }

    protected bool IsTargetInRange(EnemyPattern _pattern)
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
            LayerManager.playerLayerMask,
            Color.red,
            true);

        return hit.collider != null;
    }
}
