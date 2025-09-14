using UnityEngine;


public interface IEnemyPattern
{
    string patternID { get; }
    void Initialization(EnemyPatternController _controller, EnemyPattern _data);
    void Execute();
    void Update();
    void Finish();
    bool isFinished();
    void SetAnimationTrigger(string _triggerName);
    void SetAnimationBool(string _boolName, bool _value);
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
    protected Animator m_animator;

    protected bool m_initialized = false;
    protected bool isAnimationPlaying;
    protected float m_animationTimer;
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
        isAnimationPlaying = false;
        m_animationTimer = 0f;


        if (m_collider == null)
        {
            m_collider = _controller.gameObject.GetComponent<Collider2D>();
        }

        if (m_animator == null)
        {
            m_animator = _controller.gameObject.GetComponent<Animator>();
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
    public void SetAnimationTrigger(string _triggerName)
    {
        if (m_animator != null)
        {
            m_animator.SetTrigger(_triggerName);
        }
    }
    public void SetAnimationBool(string _boolName, bool _value)
    {
        if (m_animator != null)
        {
            m_animator.SetBool(_boolName, _value);
        }
    }
    public virtual void PlayAnimation()
    {
        if (m_animator == null)
            return;


        if (false == string.IsNullOrEmpty(patternData.animationTrigger))
        {
            SetAnimationTrigger(patternData.animationTrigger);
        }

        if (false == string.IsNullOrEmpty(patternData.animationBool))
        {
            //Debug.Log($"PlayAnimation: {patternData.animationBool}");
            SetAnimationBool(patternData.animationBool, true);
        }

        isAnimationPlaying = true;
        m_animationTimer = 0f;
    }

    public virtual void StopAnimation()
    {
        if (m_animator == null)
            return;


        if (false == string.IsNullOrEmpty(patternData.animationBool))
        {
            SetAnimationBool(patternData.animationBool, false);
        }

        isAnimationPlaying = false;
        m_animationTimer = 0f;
    }


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
        Vector2 newPosition = new Vector2
            (enemyTransform.position.x + patternData.areaEffectOffset.x * enemyTransform.localScale.x,
             enemyTransform.position.y + patternData.areaEffectOffset.y);

        RaycastHit2D hit = MyDebug.BoxCast(
            newPosition,
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
