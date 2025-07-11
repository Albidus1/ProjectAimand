using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;



public class BossAbility : MonoBehaviour
{
    public float damage = 10f;
    public float InitailCooldownTime = 5f;
    public bool afterCooldown = false;

    public float cooldownTimer { get; set; }
    public bool isOnCooldown { get; protected set; }
    public bool isAbilityActive { get; protected set; }


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
            debugText.text = $"Ability Active: {isAbilityActive}    Cooldown: {cooldownTimer:F2}s";
        }
    }

    public virtual IEnumerator UseAbility()
    {
        yield return null;
    }
}
