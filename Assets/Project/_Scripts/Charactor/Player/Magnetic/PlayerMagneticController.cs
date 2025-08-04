using Unity.IntegerTime;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerMagneticController : MonoBehaviour
{
    [SerializeField] private PlayerMovement m_playerMovement;
    [SerializeField] private PlayerMagneticData m_playerMagneticData;

    [Header("Control Settings")]
    [SerializeField] private KeyCode m_pullKey = KeyCode.S;
    [SerializeField] private KeyCode m_pushKey = KeyCode.D;
    [Space(2)]
    [SerializeField] private float m_pullDelayTime = 1f;
    [SerializeField] private float m_pushDelayTime = 1f;
    [SerializeField] private float m_timeToRiftToPull = 2f;

    [Space(5)]
    [Header("Visualization")]
    [SerializeField] private MagneticVisualization m_pullMagneticRangeVisualization;
    [SerializeField] private MagneticVisualization m_pushMagneticRangeVisualization;

    [Space(5)]
    [Header("Magnetic Settings")]
    [SerializeField] private Transform m_holdPos;
    [SerializeField] private float m_vibrateAmplitude = 0.05f;  // 진동 크기
    [SerializeField] private float m_vibrateFrequencyX = 3f;    // X축 진동 속도  
    [SerializeField] private float m_vibrateFrequencyY = 2.3f;  // Y축 진동 속도
    [SerializeField] private float m_phaseOffset = 1.57f;       // 위상 차이 (π/2)

    private MagneticObject m_pullingObject;
    private bool m_isHoldingPullingObject;
    private Vector3 m_riftPos;
    private MagneticObject[] m_pushingObjects;
    private float m_startPullTime;
    private float m_lastPullTime;
    private float m_startPushTime;
    private float m_lastPushTime;

    // 장애물 레이어 마스크 (*어디까지가 장애물 레이어인지 명확히할 필요 있음)
    private LayerMask _obstacleMask =>
        1 << LayerMask.NameToLayer("Platform") |
        1 << LayerMask.NameToLayer("Platform_Moving") |
        1 << LayerMask.NameToLayer("Platform_OneWay");
    private LayerMask _magneticMask => ~_obstacleMask &
        (~LayerMask.NameToLayer("Player")); // 기타 레이어 추가 제외 (플레이어)

    public bool IsPulling => m_pullingObject != null || m_isHoldingPullingObject;
    public bool IsPushing => false;

    private void Awake()
    {
        SetupVisualization();
    }

    void Update()
    {
        PullControlUpdate();
    }

    private void FixedUpdate()
    {
        PullControlFixedUpdate();
    }

    // 반드시 Awake에서 호출 (Visualization은 Start에서 호출하므로 먼저 호출)
    private void SetupVisualization()
    {
        m_pullMagneticRangeVisualization.gameObject.transform.localPosition = Vector3.zero;
        m_pushMagneticRangeVisualization.gameObject.transform.localPosition = Vector3.zero;

        m_pullMagneticRangeVisualization.Range = m_playerMagneticData.PullRange;
        m_pullMagneticRangeVisualization.ConeAngle = m_playerMagneticData.PullAngle;
        m_pushMagneticRangeVisualization.Range = m_playerMagneticData.PushRange;

        m_pullMagneticRangeVisualization.gameObject.SetActive(false);
        m_pushMagneticRangeVisualization.gameObject.SetActive(false);
    }

    #region PULL_CONTROL
    private void PullControlUpdate()
    {
        if (Input.GetKeyDown(m_pullKey))
        {
            HandlePullKeyDown();
        }
        else if (Input.GetKey(m_pullKey))
        {
            HandlePullKeyHold();
        }
        else if (Input.GetKeyUp(m_pullKey))
        {
            HandlePullKeyUp();
        }
    }

    private void PullControlFixedUpdate()
    {
        if (m_pullingObject != null)
        {
            // 리프트 타임 동안은 들어올리기만
            if (Time.time - m_startPullTime <= m_timeToRiftToPull)
            {
                RiftObject(m_pullingObject);
            }
            // 리프트 타임이 끝난 이후로는 끌어오기
            else if (!m_isHoldingPullingObject)
            {
                PullingObject(m_pullingObject);
            }
        }
    }

    private void HandlePullKeyDown()
    {
        if (!CanStartPull()) return;

        UpdatePullVisualization();
        TryFindNewTarget();
        m_pullMagneticRangeVisualization.gameObject.SetActive(true);
    }

    private void HandlePullKeyHold()
    {
        if (!IsPullVisualizationActive()) return;

        UpdatePullVisualization();

        if (m_pullingObject != null)
        {
            HandleExistingPullingObject();
            HandleHoldingObject();
        }
        else
        {
            TryFindNewTarget();
        }
    }

    private void HandlePullKeyUp()
    {
        if (m_pullingObject == null) return;

        m_pullMagneticRangeVisualization.gameObject.SetActive(false);
        ReleaseHoldingObject();
        ReleasePullingObject();
        m_lastPullTime = Time.time;
    }

    private bool CanStartPull()
    {
        return Time.time - m_lastPullTime >= m_pullDelayTime;
    }

    private bool IsPullVisualizationActive()
    {
        return m_pullMagneticRangeVisualization.gameObject.activeSelf;
    }

    private void HandleExistingPullingObject()
    {
        if (!m_isHoldingPullingObject && ShouldReleasePullingObject())
        {
            ReleasePullingObject();
            m_lastPullTime = Time.time;
        }
    }

    private void HandleHoldingObject()
    {
        if (m_isHoldingPullingObject)
        {
            m_pullingObject.transform.parent = m_holdPos;
            m_pullingObject.transform.localPosition =
                Vector3.Lerp(m_pullingObject.transform.localPosition, Vector3.zero, 10f * Time.deltaTime);
            m_pullingObject.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        }
    }

    private bool ShouldReleasePullingObject()
    {
        return !m_isHoldingPullingObject && !IsInPullRange(m_pullingObject);
    }

    private void TryFindNewTarget()
    {
        if (!CanFindNewTarget()) return;

        m_pullingObject = FindNearestMagneticTarget();
        m_startPullTime = Time.time;
    }

    private bool CanFindNewTarget()
    {
        return Time.time - m_lastPullTime >= m_pullDelayTime;
    }
#endregion

    #region PULL
    private void UpdatePullVisualization()
    {
        // Pull Visualization 반대로 뒤집기
        if ((m_holdPos.transform.localPosition.x > 0) != m_playerMovement.isFacingRight ||
            m_pullMagneticRangeVisualization.DirToRight != m_playerMovement.isFacingRight)
            FlipPullVisualization();
    }

    private void FlipPullVisualization()
    {
        Vector3 holdLocalPos = m_holdPos.transform.localPosition;
        holdLocalPos.x *= (holdLocalPos.x > 0) == m_playerMovement.isFacingRight ? 1 : -1;
        m_holdPos.transform.localPosition = holdLocalPos;

        m_pullMagneticRangeVisualization.DirToRight = m_playerMovement.isFacingRight;
    }

    private MagneticObject FindNearestMagneticTarget()
    {
        float pullRange = m_playerMagneticData.PullRange;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(this.transform.position, pullRange, _magneticMask);

        if (hits.Length == 0) return null;

        Vector3 playerDirection = m_playerMovement.isFacingRight ? Vector3.right : -Vector3.right;
        float dotAbs = Mathf.Cos(m_playerMagneticData.PullAngle * 0.5f * Mathf.Deg2Rad);

        MagneticObject result = null;
        float distance = m_playerMagneticData.PullRange;
        for (int i = 0; i < hits.Length; ++i)
        {
            MagneticObject magneticObject;
            if (hits[i].TryGetComponent<MagneticObject>(out magneticObject))
            {
                // 인력이 적용 안되는 대상일 경우 패스
                if (!magneticObject.IsPullable) continue;

                Vector2 vec2Object = hits[i].transform.position - this.transform.position;
                Vector2 dir2Object = vec2Object.normalized;
                float distance2 = vec2Object.magnitude;
                float dotResult = Vector2.Dot(playerDirection, dir2Object);

                // Pull 범위 안에 있을때
                if (dotResult > dotAbs)
                {
                    // 중간에 장애물이 없을 경우
                    if (!IsBlockByObstacleToPoint(this.transform.position, dir2Object, distance2))
                    {
                        // 더 가까운지 체크
                        if (result == null || distance2 < distance)
                        {
                            distance = distance2;
                            result = magneticObject;
                            m_riftPos = magneticObject.transform.position +
                                Vector3.up * m_playerMagneticData.RiftDistance;
                        }
                    }
                }
            }
        }

        return result;
    }

    private bool IsInPullRange(MagneticObject target)
    {
        Vector3 playerDirection = m_playerMovement.isFacingRight ? Vector3.right : -Vector3.right;
        float dotAbs = Mathf.Cos(m_playerMagneticData.PullAngle * 0.5f * Mathf.Deg2Rad);

        Vector2 vec2Object = target.transform.position - this.transform.position;
        Vector2 dir2Object = vec2Object.normalized;
        float distance = vec2Object.magnitude;
        float dotResult = Vector2.Dot(playerDirection, dir2Object);

        // Pull 범위 안에 있을때
        if (dotResult > dotAbs)
        {
            // 중간에 장애물이 없을 경우
            if (!IsBlockByObstacleToPoint(this.transform.position, dir2Object, distance))
            {
                return true;
            }
        }
        return false;
    }

    private void RiftObject(MagneticObject target)
    {
        if (target == null) return;

        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // offset 제거, 원래 목표점 사용
        Vector3 dir = m_riftPos - target.transform.position;
        float distance = dir.magnitude;

        // 목표점에 너무 가까으면 힘을 적용하지 않음 (데드존)
        float deadZone = 0.15f;
        if (distance < deadZone)
        {
            // 목표점 근처에서는 속도 감소
            rb.linearVelocity *= 0.95f; // 점진적 속도 감소

            return;
        }

        dir.Normalize();

        // 속도 기반 댐핑 계산
        Vector2 velocityTowardTarget = Vector3.Project(rb.linearVelocity, dir);
        float velocityMagnitude = velocityTowardTarget.magnitude;
        bool isMovingTowardTarget = Vector2.Dot(rb.linearVelocity, dir) > 0;

        // 거리 기반 힘 계산
        float dampingFactor = Mathf.Clamp01(distance / 0.5f);
        float basePullForce = Mathf.Lerp(m_playerMagneticData.PullPowerMin, m_playerMagneticData.PullPowerMax, dampingFactor);

        float velocityDampingFactor = 1f;
        if (isMovingTowardTarget && distance < 0.3f) // 가까이 있고 목표점으로 향하고 있을 때
        {
            // 속도가 빠를수록 힘을 줄임 (브레이크 효과)
            velocityDampingFactor = Mathf.Clamp01(1f - (velocityMagnitude / 5f));
        }

        // 최종 힘 계산
        Vector3 force = dir * basePullForce * velocityDampingFactor;

        // 안정화 구역에서 추가 처리
        float stabilizeZone = 0.3f;
        if (distance < stabilizeZone)
        {
            // 진동 방지를 위한 추가 속도 감소
            rb.linearVelocity *= 0.9f;

            // 매우 가까울 때는 힘을 더욱 약하게
            if (distance < 0.1f)
            {
                force *= 0.3f;
            }

            Vector2 weakVibrateForce = 
                Vector3.Project(Random.insideUnitCircle, Vector2.up) * m_vibrateAmplitude * 20f;
            rb.AddForce(weakVibrateForce, ForceMode2D.Force);
        }

        rb.AddForce(force, ForceMode2D.Force);
    }

    private void ReleasePullingObject()
    {
        if (m_pullingObject == null) return;

        m_pullingObject.GetComponent<Rigidbody2D>().gravityScale = 1f;
        m_pullingObject = null;
    }

    private void ReleaseHoldingObject()
    {
        if (m_pullingObject == null) return;
        if (!m_isHoldingPullingObject) return;

        m_pullingObject.transform.parent = null;
        m_isHoldingPullingObject = false;
    }

    private void PullingObject(MagneticObject target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();

        Vector2 vec2Target = target.transform.position - this.transform.position;
        float distance = vec2Target.magnitude;
        float factor = distance / m_playerMagneticData.PullPowerMaxRange;
        float power = Mathf.Lerp(
            m_playerMagneticData.PullPowerMin,
            m_playerMagneticData.PullPowerMax,
            factor);

        // Hold 위치로의 방향
        Vector2 dir2Hold = (m_holdPos.position - target.transform.position);
        float dist2Hold = dir2Hold.magnitude;
        dir2Hold.Normalize();

        // 목표점에 너무 가까으면 힘을 적용하지 않음 (데드존)
        float deadZone = 0.15f;
        if (dist2Hold < deadZone)
        {
            // 목표점 근처에서는 속도 감소
            rb.linearVelocity *= 0.95f; // 점진적 속도 감소

            // 매우 가까울 때는 홀딩
            if (dist2Hold < 0.1f)
            {
                m_isHoldingPullingObject = true;
                rb.linearVelocity = Vector2.zero;
            }

            return;
        }

        // 속도 기반 댐핑 계산
        Vector2 velocityTowardTarget = Vector3.Project(rb.linearVelocity, dir2Hold);
        float velocityMagnitude = velocityTowardTarget.magnitude;
        bool isMovingTowardTarget = Vector2.Dot(rb.linearVelocity, dir2Hold) > 0;

        float velocityDampingFactor = 1f;
        if (isMovingTowardTarget && dist2Hold < 0.3f) // 가까이 있고 목표점으로 향하고 있을 때
        {
            // 속도가 빠를수록 힘을 줄임 (브레이크 효과)
            velocityDampingFactor = Mathf.Clamp01(1f - (velocityMagnitude / 5f));
        }

        Vector3 force = dir2Hold * power * velocityDampingFactor;

        // 안정화 구역에서 추가 처리
        float stabilizeZone = 0.3f;
        if (dist2Hold < stabilizeZone)
        {
            // 진동 방지를 위한 추가 속도 감소
            rb.linearVelocity *= 0.9f;
        }

        rb.AddForce(force, ForceMode2D.Force);
    }
    #endregion

    private bool IsBlockByObstacleToPoint(Vector3 position, Vector3 dir, float dist)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, dir, dist, _obstacleMask);
        return hit;
    }
}
