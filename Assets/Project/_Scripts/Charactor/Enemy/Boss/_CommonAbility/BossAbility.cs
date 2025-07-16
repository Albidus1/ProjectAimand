using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;



public class BossAbility : MonoBehaviour
{
    [Header("스킬 세팅")]
    public bool singleSkill = true;
    [Space(10)]

    public GameObject abilityPrefab;
    public float damage = 10f;
    public float InitailCooldownTime = 5f;
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
            
        }
        else
        {
            isAbilityActive = false;
            if (false == isOnCooldown)
            {
                isOnCooldown = true;
            }
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
}
