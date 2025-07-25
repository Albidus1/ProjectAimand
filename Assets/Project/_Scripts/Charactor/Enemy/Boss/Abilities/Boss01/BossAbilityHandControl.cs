using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;




public class BossAbilityHandControl : BossAbility
{
    [Header("손 오브젝트")]
    public BossHands hands;
    public GameObject lazerBeamObject;
    public float lazerBeamDuration = 2f;

    [Header("지정 좌표")]
    [MyReadOnly]
    public List<MoveElement> moveElements = new List<MoveElement>();
    public List<MoveElement> moveLeftElements = new List<MoveElement>();
    public List<MoveElement> moveRightElements = new List<MoveElement>();

    [Header("이동 패턴")]
    public List<MovePattern> patterns = new List<MovePattern>();

    [Header("이동 설정")]
    public float moveSpeed;
    public Ease moveEase = Ease.Linear;
    public bool lazerPattern;

    private bool isRightFirst = false;
    private GameObject m_currentHand;
    private MovePattern m_currentPattern;

    private bool isRightHand = false;
    private Vector2 m_initialLeftHandPosition;
    private Vector2 m_initialRightHandPosition;
    private int m_currentPositionIndex = 0;
    private int m_currnetPatternIndex = 0;



    private void Awake()
    {
        //hands = transform.parent.GetComponent<BossHands>();
    }

    protected override void Start()
    {
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();

        m_initialLeftHandPosition = hands.leftHand.transform.position;
        m_initialRightHandPosition = hands.rightHand.transform.position;
    }

    public override void SkillReset()
    {
        base.SkillReset();
    }

    public override IEnumerator UseAbility()
    {
        if (isAbilityActive || hands.isAbilityActive)
        {
            yield break;
        }

        hands.isAbilityActive = true;

        base.SetAbilityActive(true);

        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);

        yield return StartCoroutine(ObjectsActivate());
        yield return new WaitForSeconds(0.2f);
        base.SetAbilityActive(false);

        hands.isAbilityActive = false;
    }

    protected override void AbilityRangeVisualizer()
    {
        if (lazerPattern)
        {
            RandomPosition();

            base.abilityPrefab = isRightHand ?
                hands.rightHand : hands.leftHand;

            base.AbilityRangeVisualizer();
        }
        else
        {
            SetHandPosition();

            m_currentPattern = patterns[m_currnetPatternIndex];

            moveElements = moveLeftElements;
            m_spawnPoint = moveLeftElements[0].movePosition;

            if (moveLeftElements.Count > 1)
            {
                Vector3 dir1 = (moveLeftElements[1].movePosition - moveLeftElements[0].movePosition).normalized;
                Vector3 dir2 = (moveRightElements[1].movePosition - moveRightElements[0].movePosition).normalized;

                Vector3 pos1 = GetVectorCenter(moveLeftElements[0].movePosition, moveLeftElements[1].movePosition);
                Vector3 pos2 = GetVectorCenter(moveRightElements[0].movePosition, moveRightElements[1].movePosition);

                if (m_currentPattern.randomMirroring)
                {
                    isRightFirst = Random.Range(0, 2) == 0;
                    if (isRightFirst)
                    {
                        HandIndicatorRender(base.abilityRangePrefab, pos2, dir2);
                    }
                    else
                    {
                        HandIndicatorRender(base.abilityRangePrefab, pos1, dir1);
                    }
                }
                else
                {
                    HandIndicatorRender(base.abilityRangePrefab, pos1, dir1);
                    HandIndicatorRender(base.abilityRangePrefab, pos2, dir2);
                }
            }
        }
    }

    private void RandomPosition()
    {
        isRightHand = Random.Range(0, 2) == 0;

        if (isRightHand)
        {
            m_currentPositionIndex = Random.Range(0, moveRightElements.Count);
            moveElements = moveRightElements;
            m_spawnPoint = moveRightElements[m_currentPositionIndex].movePosition;
        }
        else
        {
            m_currentPositionIndex = Random.Range(0, moveLeftElements.Count);
            moveElements = moveLeftElements;
            m_spawnPoint = moveLeftElements[m_currentPositionIndex].movePosition;
        }
    }

    private void SetHandPosition()
    {
        m_currnetPatternIndex = Random.Range(0, patterns.Count);

        var pattern = patterns[m_currnetPatternIndex];

        moveLeftElements.Clear();
        moveRightElements.Clear();
        for (int i = 0; i < pattern.moveElements.Count; i++)
        {
            moveLeftElements.Add((MoveElement)pattern.moveElements[i].Clone());
            moveRightElements.Add((MoveElement)pattern.moveElements[i].Clone());
            moveRightElements[i].movePosition = GetMirrorXPosition(base.initialAbilityRangePosition.position, moveLeftElements[i].movePosition);
        }
    }

    private void HandIndicatorRender(GameObject _obj, Vector3 _pos, Vector3 _dir)
    {
        GameObject indicator = Instantiate(_obj, m_spawnPoint, Quaternion.identity);

        float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
        indicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        indicator.transform.position = _pos;

        SpriteRenderer indicatorRenderer = indicator.GetComponent<SpriteRenderer>();

        if (autoResize)
        {
            BoxCollider2D spawnObj = _obj.GetComponent<BoxCollider2D>();

            indicator.transform.localScale = new Vector2(spawnObj.bounds.size.x, spawnObj.bounds.size.y);
            //abilityRangeSprite.size = new Vector2(m_collider2D.bounds.size.x, m_collider2D.bounds.size.y);
        }
        else
        {
            indicator.transform.localScale = new Vector2(abilityRangeSize.x, abilityRangeSize.y);
            //abilityRangeSprite.size = new Vector2(base.abilityRangeSize.x, base.abilityRangeSize.y);
        }

        Vector2 newPosition = indicator.transform.position;
        if (AxisXLock)
        {
            newPosition.x = initialAbilityRangePosition.transform.position.x;
        }
        if (AxisYLock)
        {
            newPosition.y = initialAbilityRangePosition.transform.position.y;
        }
        indicator.transform.position = newPosition;

        indicatorRenderer.DOFade(0, fadeDuration)
            .OnComplete(() => Destroy(indicator, fadeDuration));
    }

    private IEnumerator ObjectsActivate()
    {
        var sequence = DOTween.Sequence();

        if (lazerPattern)
        {
            var hand = isRightHand ? hands.rightHand : hands.leftHand;
            var initialPosition = isRightHand ?
                m_initialRightHandPosition : m_initialLeftHandPosition;
            var element = moveElements[m_currentPositionIndex];

            sequence.Append(MoveTo(hand, element.movePosition, element.moveSpeed, element.moveEase));
            sequence.AppendInterval(0.1f);
            sequence.AppendCallback(SpawnLazerBeam);
            sequence.AppendInterval(lazerBeamDuration);
            sequence.Append(MoveTo(hand, initialPosition, moveSpeed, moveEase));
            sequence.AppendInterval(0.1f);
        }
        else
        {
            var currentPattern = patterns[m_currnetPatternIndex];
            if (currentPattern.isSyncMoving)
            {
                foreach (var e in moveElements)
                {
                    Vector2 mirrorPosition = new Vector2((2 * initialAbilityRangePosition.position.x) - e.movePosition.x, e.movePosition.y);

                    sequence.Append(hands.leftHand.transform.DORotate(new Vector3(0, 0, e.rotate), 0));
                    sequence.Join(hands.rightHand.transform.DORotate(new Vector3(0, 0, -e.rotate), 0));

                    sequence.Append(MoveTo(hands.leftHand, e.movePosition, e.moveSpeed, e.moveEase));
                    sequence.Join(MoveTo(hands.rightHand, mirrorPosition, e.moveSpeed, e.moveEase)
                        .OnComplete(() => SetHandsCollision(e.isOnCollision)));
                    sequence.AppendInterval(e.waitTime);
                }
            }
            else
            {
                int index = 2;

                if (currentPattern.randomMirroring)
                {
                    index = 1;
                }

                for (int i = 0; i < index; i++)
                {
                    m_currentHand = i == 0 ? 
                        (isRightFirst ? hands.rightHand : hands.leftHand)
                        : (isRightFirst ? hands.leftHand : hands.rightHand);

                    foreach (var e in moveElements)
                    {
                        Vector2 position = m_currentHand == hands.leftHand ?
                            e.movePosition :
                            new Vector2((2 * initialAbilityRangePosition.position.x) - e.movePosition.x, e.movePosition.y);

                        float rotate = m_currentHand == hands.leftHand ? e.rotate : -e.rotate;

                        sequence.Append(m_currentHand.transform.DORotate(new Vector3(0, 0, rotate), 0));
                        sequence.Join(MoveTo(m_currentHand, position, e.moveSpeed, e.moveEase)
                            .OnComplete(() => SetHandsCollision(e.isOnCollision)));
                        sequence.AppendInterval(e.waitTime);
                    }
                }
            }

            sequence.AppendCallback(() => SetHandsCollision(false));
            sequence.Append(hands.leftHand.transform.DORotate(Vector3.zero, 0));
            sequence.Join(hands.rightHand.transform.DORotate(Vector3.zero, 0));
            sequence.Append(MoveTo(hands.leftHand, m_initialLeftHandPosition, moveSpeed, moveEase));
            sequence.Join(MoveTo(hands.rightHand, m_initialRightHandPosition, moveSpeed, moveEase));
            sequence.AppendInterval(0.1f);
        }

        yield return sequence.WaitForCompletion();
    }

    private void SetHandsCollision(bool _OnOff)
    {
        Debug.Log($"손 콜리전 {_OnOff}");
        hands.isOnCollision = _OnOff;
    }

    private Tween MoveTo(GameObject _obj, Vector2 _target, float _moveSpeed, Ease _moveEase)
    {
        return _obj.transform.DOMove(_target, _moveSpeed)
            .SetEase(_moveEase);
    }

    private void SpawnLazerBeam()
    {     
        if (lazerBeamObject == null)
        {
            return;
        }

        Vector3 half = new Vector2(lazerBeamObject.transform.localScale.x * 0.5f, 0);
        Vector2 position = isRightHand ?
            hands.rightHand.transform.position - half :
            hands.leftHand.transform.position + half;


        BossVanishingObject obj = Instantiate(lazerBeamObject, position, Quaternion.identity).GetComponent<BossVanishingObject>();
        obj.disableTime = lazerBeamDuration;
        obj.UseSkill();
    }

    #region GENERAL METHODS
    private Vector3 GetVectorCenter(Vector3 _pos1, Vector3 _pos2)
    {
        float x = (_pos1.x + _pos2.x) * 0.5f;
        float y = (_pos1.y + _pos2.y) * 0.5f;
        return new Vector3(x, y, 0);
    }

    private Vector3 GetMirrorXPosition(Vector3 _orgin, Vector3 _target)
    {
        return new Vector3((2 * _orgin.x) - _target.x, _target.y, 0);
    }
    #endregion
}
