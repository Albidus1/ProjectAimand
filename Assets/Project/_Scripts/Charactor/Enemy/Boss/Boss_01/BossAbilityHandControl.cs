using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BossAbilityHandControl : BossAbility
{
    [Header("손 오브젝트")]
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject lazerBeamObject;

    [Header("지정 좌표")]
    [MyReadOnly]
    public Transform[] movePosition = new Transform[6];
    public Transform[] leftHandMovePosition = new Transform[3];
    public Transform[] rightHandMovePosition = new Transform[3];

    [Header("이동 설정")]
    public Ease moveEase = Ease.Linear;
    public float moveSpeed;

    private bool isRightHand = false;
    private Vector2 m_initialLeftHandPosition;
    private Vector2 m_initialRightHandPosition;
    private int m_currentPositionIndex = 0;
 



    private void Awake()
    {
        
    }

    protected override void Start()
    {
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
        m_initialLeftHandPosition = leftHand.transform.position;
        m_initialRightHandPosition = rightHand.transform.position;

        lazerBeamObject.SetActive(false);
    }

    public override IEnumerator UseAbility()
    {
        if (isAbilityActive)
        {
            yield break;
        }

        base.SetAbilityActive(true);
        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);

        yield return StartCoroutine(ObjectsActivate());
        base.SetAbilityActive(false);

        yield return new WaitForSeconds(0.1f);
    }

    private void RandomPosition()
    {
        isRightHand = Random.Range(0, 2) == 0;

        if (isRightHand)
        {
            m_currentPositionIndex = Random.Range(0, rightHandMovePosition.Length);
            movePosition = rightHandMovePosition;
            m_spawnPoint = movePosition[m_currentPositionIndex].position;
        }
        else
        {
            m_currentPositionIndex = Random.Range(0, leftHandMovePosition.Length);
            movePosition = leftHandMovePosition;
            m_spawnPoint = movePosition[m_currentPositionIndex].position;
        }
    }

    protected override void AbilityRangeVisualizer()
    {
        RandomPosition();

        base.abilityPrefab = (m_currentPositionIndex < movePosition.Length * 0.5f) ?
            leftHand : rightHand;

        base.AbilityRangeVisualizer();
    }

    private IEnumerator ObjectsActivate()
    {
        var sequence = DOTween.Sequence();

        var hand = isRightHand ? rightHand : leftHand;
        var initialPosition = isRightHand ?
            m_initialRightHandPosition : m_initialLeftHandPosition;

        sequence.Append(MoveTo(hand, movePosition[m_currentPositionIndex].position));
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(LazerBeam);
        sequence.AppendInterval(3f);
        sequence.AppendCallback(LazerBeam);
        sequence.Append(MoveTo(hand, initialPosition));

        yield return sequence.WaitForCompletion();
    }

    private Tween MoveTo(GameObject _obj, Vector2 _target)
    {
        return _obj.transform.DOMove(_target, moveSpeed)
            .SetEase(moveEase);
    }

    private void LazerBeam()
    {     
        if (lazerBeamObject == null)
        {
            return;
        }

        if (false == lazerBeamObject.activeSelf)
        {
            Vector3 half = new Vector2(lazerBeamObject.transform.localScale.x * 0.5f, 0);
            Vector2 position = isRightHand ?
                rightHand.transform.position - half :
                leftHand.transform.position + half;
            
            lazerBeamObject.transform.position = position;

            lazerBeamObject.SetActive(true);
        }
        else
        {
            lazerBeamObject.SetActive(false);
        }
    }
}
