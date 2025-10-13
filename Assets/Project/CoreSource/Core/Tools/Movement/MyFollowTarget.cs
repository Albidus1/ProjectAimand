using System;
using UnityEngine;
using UnityEngine.PlayerLoop;



public class MyFollowTarget : MonoBehaviour
{
    public enum UpdateModes { Update, FixedUpdate, LateUpdate }
    public enum PositionSpaces { World, Local }

    [Header("위치 추적")]
    public bool followPosition = true;
    [MyConditionalHide("followPosition", true)]
    public bool followPositionX = true;
    [MyConditionalHide("followPosition", true)]
    public bool followPositionY = true;
    [MyConditionalHide("followPosition", true)]
    public bool followPositionZ = true;
    [MyConditionalHide("followPosition", true)]
    public PositionSpaces positionSpace = PositionSpaces.World;

    [Header("위치 보간")]
    public bool interpolatePosition = true;
    [MyConditionalHide("interpolatePosition", true)]
    public float followPositionSpeed = 10f;

    [Header("회전 추적")]
    public bool followRotation = false;

    [Header("목표")]
    public Transform target;
    [MyConditionalHide("followPosition", true)]
    public Vector3 offset;
    [MyConditionalHide("followPosition", true)]
    public bool addInitialDistanceXToXOffset = false;
    [MyConditionalHide("followPosition", true)]
    public bool addInitialDistanceYToYOffset = false;
    [MyConditionalHide("followPosition", true)]
    public bool addInitialDistanceZToZOffset = false;

    [Header("업데이트 모드")]
    public UpdateModes updateMode = UpdateModes.FixedUpdate;

    [Header("거리 설정")]
    public bool useMinimumDistanceBeforeFollow = false;
    public float minDistanceBeforeFollow = 1f;
    public bool useMaximumDistanceBeforeFollow = false;
    public float maxDistance = 1f;


    protected bool m_localSpace { get { return positionSpace == PositionSpaces.Local; } }

    protected Vector3 m_velocity = Vector3.zero;
    protected Vector3 m_initialPosition;
    protected Vector3 m_newTargetPosition;
    protected Vector3 m_lastTargetPosition;
    protected Vector3 m_direction;
    protected Vector3 m_newPosition;
    protected Quaternion m_newTargetRotation;
    protected Quaternion m_initialRotation;



    protected virtual void Start()
    {
        Initialization();
    }

    public virtual void Initialization()
    {
        SetInitialPosition();
        SetOffset();
    }
    protected virtual void SetInitialPosition()
    {
        m_initialPosition = m_localSpace ? transform.localPosition : transform.position;
        m_initialRotation = transform.rotation;
        m_lastTargetPosition = m_localSpace ? transform.localPosition : transform.position;
    }

    protected virtual void SetOffset()
    {
        if (target == null)
        {
            return;
        }

        Vector3 difference = transform.position - target.transform.position;
        offset.x = addInitialDistanceXToXOffset ? difference.x : offset.x;
        offset.y = addInitialDistanceYToYOffset ? difference.y : offset.y;
        offset.z = addInitialDistanceZToZOffset ? difference.z : offset.z;
    }

    public virtual void StartFollowing()
    {
        followPosition = true;
        SetInitialPosition();
    }

    public virtual void StopFollowing()
    {
        followPosition = false;
    }

    protected virtual void Update()
    {
        if (target == null)
        {
            return;
        }

        if (updateMode == UpdateModes.Update)
        {
            FollowTargetRotation();
            FollowTargetPositioin();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (updateMode == UpdateModes.FixedUpdate)
        {
            FollowTargetRotation();
            FollowTargetPositioin();
        }
    }

    protected virtual void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (updateMode != UpdateModes.LateUpdate)
        {
            FollowTargetRotation();
            FollowTargetPositioin();
        }
    }

    protected virtual void FollowTargetRotation()
    {
        if (target == null)
        {
            return;
        }
        if (false == followRotation)
        {
            return;
        }


        m_newTargetRotation = target.rotation;
        transform.rotation = m_newTargetRotation;
    }

    protected virtual void FollowTargetPositioin()
    {
        if (target == null)
        {
            return;
        }
        if (false == followPosition)
        {
            return;
        }

        m_newTargetPosition = target.position + offset;
        if (false == followPositionX)
        {
            m_newTargetPosition.x = m_initialPosition.x;
        }
        if (false == followPositionY)
        {
            m_newTargetPosition.y = m_initialPosition.y;
        }
        if (false == followPositionZ)
        {
            m_newTargetPosition.z = m_initialPosition.z;
        }

        m_direction = (m_newTargetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, m_newTargetPosition);
        float interpolatedDistance = distance;

        if (interpolatePosition)
        {
            float rate = Mathf.Clamp01(followPositionSpeed);
            float invRate = -Mathf.Log(1f - rate, 2f) * 60f;
            float LerpRate = Mathf.Pow(2f, -invRate * Time.deltaTime);

            interpolatedDistance = Mathf.Lerp(distance, 0f, LerpRate);
            interpolatedDistance = ApplyMinMaxDistancing(distance, interpolatedDistance);
            transform.Translate(m_direction * interpolatedDistance, Space.World);
        }
        else
        {
            interpolatedDistance = ApplyMinMaxDistancing(distance, interpolatedDistance);
            transform.Translate(m_direction * interpolatedDistance, Space.World);
        }
    }

    private float ApplyMinMaxDistancing(float _distance, float _interpolatedDistance)
    {
        if (useMinimumDistanceBeforeFollow && (_distance - _interpolatedDistance < minDistanceBeforeFollow))
        {
            _interpolatedDistance = 0f;
        }

        if (useMaximumDistanceBeforeFollow && (_distance - _interpolatedDistance >= maxDistance))
        {
            _interpolatedDistance = _distance - maxDistance;
        }

        return _interpolatedDistance;
    }
}
