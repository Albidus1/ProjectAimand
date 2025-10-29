using UnityEngine;

public class ObstacleMoving : MyPath, IEventListener<TriggerEvent>
{
    [Header("이동 설정")]
    [Range(0f, 1f)]
    public float speedReductionRatio = 0.6f;
    public float speedTransitionTime = 3f;
    public bool isObstacleICamera;

    [Header("이벤트")]
    public bool useTriggerEvent = false;
    [MyConditionalHide("useTriggerEvent", true)]
    public string eventID;


    private bool m_triggered = false;
    private float m_waitTimer;


    protected override void Start()
    {
        Initialization();
    }

    public override void Initialization()
    {
        if (false == base.Initialized)
        {
            base.Initialization();
        }

        base.canMove = false;
    }

    protected override void Update()
    {
        if (base.pathElements == null
            || base.pathElements.Count < 1
            || base.m_endReached
            || false == canMove)
        {
            return;
        }


        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        isObstacleICamera = IsViewportPointInOrthographicCamera(viewportPoint);

        float maxSpeed = base.pathElements[base.m_previousIndex].speed;
        float minSpeed = maxSpeed * speedReductionRatio;
        float targetSpeed = isObstacleICamera ? minSpeed : maxSpeed;

        base.m_currentSpeed = Mathf.Lerp(base.m_currentSpeed, targetSpeed, speedTransitionTime * Time.deltaTime);

        Vector3 position = base.originalTransformPosition + base.m_currentPoint.Current;
        transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * base.m_currentSpeed);

        base.m_distanceToNextPoint = (transform.position - position).magnitude;
        if (base.m_distanceToNextPoint < base.minDistanceToGoal)
        {
            if (base.pathElements.Count > base.currentIndex)
            {
                m_waitTimer = base.pathElements[base.currentIndex].delay;
            }

            base.m_previousPoint = base.m_currentPoint.Current;
            base.m_previousIndex = base.currentIndex;
            m_currentPoint.MoveNext();

            transform.position = position;
        }
    }

    private bool IsViewportPointInOrthographicCamera(Vector3 _point)
    {
        return _point.x >= 0f && _point.x <= 1f &&
               _point.y >= 0f && _point.y <= 1f;
    }

    public void OnEvent(TriggerEvent e)
    {
        if (e.eventID != this.eventID)
        {
            return;
        }

        base.canMove = true;
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<TriggerEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<TriggerEvent>();
    }
}
