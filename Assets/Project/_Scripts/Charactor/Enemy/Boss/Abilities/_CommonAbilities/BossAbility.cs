using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;



public struct AbilityEvent
{
    static AbilityEvent e;

    public bool singleUsage;
    public bool isAcivate;
    public BossAbility ab;

    public AbilityEvent(bool _flag,  bool _act, BossAbility _ab)
    {
        singleUsage = _flag;
        isAcivate = _act;
        ab = _ab;
    }

    public static void Trigger(bool _flag, bool _act, BossAbility _ab)
    {
        e.singleUsage = _flag;
        e.isAcivate = _act;
        e.ab = _ab;
        EventManager.TriggerEvent(e);
    }
}

public class BossAbility : MonoBehaviour
{
    [Header("스킬 세팅")]
    public bool singleSkill = true;
    [Space(10)]

    public GameObject abilityPrefab;
    public float InitailCooldownTime = 5f;
    public float cooldownTime = 5f;
    public int abilityActCount = 3;
    public bool afterCooldown = false;

    [Header("스킬 범위 표시")]
    public GameObject abilityRangePrefab;
    public Transform initialAbilityRangePosition;
    public float fadeDuration = 2f;
    [Space(5)]

    public bool autoResize;
    [MyConditionalHide("autoResize", true, true)]
    public Vector2 abilityRangeSize;
    [Space(5)]

    public bool AxisXLock;
    public bool AxisYLock;


    public float cooldownTimer { get; set; }
    public bool isOnCooldown { get; protected set; }
    public bool isAbilityActive { get; protected set; }
    protected Vector2 m_spawnPoint;



    [Header("디버그용 텍스트")]
    public TextMeshProUGUI debugText;



    protected virtual void Start()
    {
        Initialization();
    }

    public virtual void Initialization()
    {
        cooldownTimer = InitailCooldownTime;
        isOnCooldown = true;
        isAbilityActive = false;
    }

    public virtual void SkillReset()
    {
        cooldownTimer = cooldownTime;
        isOnCooldown = false;
        isAbilityActive = false;
    }

    private void Update()
    {
        if (debugText != null)
        {
            debugText.text = $"Name : {gameObject.name}     Ability Active: {isAbilityActive}    Cooldown: {cooldownTimer:F2}s";
        }
    }

    protected void SetAbilityActive(bool _OnOff)
    {
        if (_OnOff)
        {
            isAbilityActive = true;
            isOnCooldown = (false == afterCooldown);

            AbilityEvent.Trigger(singleSkill, true, this);

        }
        else
        {
            isAbilityActive = false;

            if (false == isOnCooldown)
            {
                isOnCooldown = true;
            }

            AbilityEvent.Trigger(singleSkill, false, null);
        }
    }

    public virtual IEnumerator UseAbility()
    {
        yield return new WaitForSeconds(0.5f);
    }

    protected virtual void AbilityRangeVisualizer()
    {
        GameObject indicator = Instantiate(abilityRangePrefab, m_spawnPoint, Quaternion.identity);
        SpriteRenderer indicatorRenderer = indicator.GetComponent<SpriteRenderer>();

        if (autoResize)
        {
            BoxCollider2D spawnObj = abilityPrefab.GetComponent<BoxCollider2D>();

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

    public virtual void PhaseChange()
    {
        
    }
}
