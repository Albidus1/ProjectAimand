using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;



public class BossAbility : MonoBehaviour
{
    [Header("보스 세팅")]
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

    public virtual IEnumerator UseAbility()
    {
        yield return null;
    }

    //protected void AbilityRangeVisualizer()
    //{
        
    //}
}
