using System.Collections;
using UnityEngine;

public class BossAbilityRangeAttack : BossAbility
{
    protected override void Start()
    {
        base.Start();
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public override IEnumerator UseAbility()
    {
        //if (abilityPrefab == null)
        //{
        //    Debug.LogWarning("Ability Prefab is not assigned.");
        //    yield break;
        //}

        base.isAbilityActive = true;
        base.isOnCooldown = (false == base.afterCooldown);


        Debug.Log("스폰 능력 사용_" + transform.name);

        //GameObject abilityInstance = Object.Instantiate(abilityPrefab, spawnPoint, Quaternion.identity);

        //abilityInstance.transform.DOMove(spawnPoint + direction, disableTime)
        //    .SetEase(moveEase)
        //    .OnComplete(() => Object.Destroy(abilityInstance));

        yield return new WaitForSeconds(5f);

        base.isAbilityActive = false;
        if (false == base.isOnCooldown)
        {
            base.isOnCooldown = true;
        }
    }
}
