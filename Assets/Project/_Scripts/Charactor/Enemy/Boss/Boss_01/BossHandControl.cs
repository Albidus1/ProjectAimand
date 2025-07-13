using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BossHandControl : BossAbility
{
    [Header("손 오브젝트")]
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject lazerBeamObject;

    [Header("지정 좌표")]
    public Transform[] movePosition = new Transform[6];

    [Header("이동 설정")]
    public Ease moveEase = Ease.Linear;
    public float moveSpeed;

    private Vector2 m_initialLeftHandPosition;
    private Vector2 m_initialRightHandPosition;
    private int m_currentPositionIndex = 0;
    private BoxCollider2D m_col;



    private void Awake()
    {
        m_col = lazerBeamObject.GetComponent<BoxCollider2D>();
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

        base.isAbilityActive = true;
        base.isOnCooldown = (false == base.afterCooldown);


        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);
        yield return StartCoroutine(ObjectsActivate());
        yield return new WaitForSeconds(0.5f);

        base.isAbilityActive = false;
        if (false == base.isOnCooldown)
        {
            base.isOnCooldown = true;
        }
    }

    private void RandomPosition()
    {
        m_currentPositionIndex = Random.Range(0, movePosition.Length);
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

        var hand = (m_currentPositionIndex < movePosition.Length * 0.5f) ? leftHand : rightHand;
        var initialPosition = (m_currentPositionIndex < movePosition.Length * 0.5f) ?
            m_initialLeftHandPosition : m_initialRightHandPosition;


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
        if (false == lazerBeamObject.activeSelf)
        {
            bool isLeftHand = (m_currentPositionIndex < movePosition.Length * 0.5f);

            Vector3 half = new Vector2(lazerBeamObject.transform.localScale.x * 0.5f, 0);
            Vector2 position = isLeftHand ?
                leftHand.transform.position + half :
                rightHand.transform.position - half;

            lazerBeamObject.transform.position = position;

            lazerBeamObject.SetActive(true);
        }
        else
        {
            lazerBeamObject.SetActive(false);
        }
    }
}
