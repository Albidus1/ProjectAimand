using System.Collections;
using DG.Tweening;
using UnityEngine;



[RequireComponent(typeof(Collider2D))]
public class BossAbilitySpawner : BossAbility
{
    [Header("스폰 오브젝트")]
    public GameObject abilityPrefab;
    public bool randomSpawnPoint = false;
    public float appendInterval = 0.5f;

    [Header("이동 설정")]
    public Ease moveEase = Ease.Linear;
    public Vector2 direction;
    public float moveSpeed;
    public float disableTime;

    private Collider2D m_collider2D;



    private void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
    }

    protected override void Start()
    {
        base.Start();

        Initialization();
        base.isOnCooldown = true;
    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public override IEnumerator UseAbility()
    {
        if (abilityPrefab == null)
        {
            Debug.LogWarning("Ability Prefab is not assigned.");
            yield break;
        }

        base.isAbilityActive = true;
        base.isOnCooldown = (false == base.afterCooldown);

        //Debug.Log("스폰 능력 사용_" + transform.name);

        yield return null;

        var sequence = DOTween.Sequence();
        for (int i = 0; i < 1; i++)
        {
            SetSpawnPoint();
            Vector2 currentSpawnPoint = base.m_spawnPoint;

            sequence.Append(AbilityRangeVisualizer(currentSpawnPoint));
            sequence.AppendInterval(0.1f);
            sequence.Append(SpawnObject(currentSpawnPoint));
            sequence.AppendInterval(appendInterval);
        }

        yield return new WaitForSeconds(0.5f);

        base.isAbilityActive = false;
        if (false == base.isOnCooldown)
        {
            base.isOnCooldown = true;
        }
    }

    private void SetSpawnPoint()
    {
        float posX = m_collider2D.bounds.center.x;
        float posY = m_collider2D.bounds.center.y;
        if (randomSpawnPoint)
        {
            posX = Random.Range(m_collider2D.bounds.min.x, m_collider2D.bounds.max.x);
            //posY = Random.Range(m_collider2D.bounds.min.x, m_collider2D.bounds.max.y);
        }

        base.m_spawnPoint = new Vector2(posX, posY);
        Debug.Log("스폰 위치: " + base.m_spawnPoint);
    }

    protected Tween AbilityRangeVisualizer(Vector2 _spawnPoint)
    {
        if (base.abilityRangePrefab == null)
            return DOVirtual.DelayedCall(0.01f, () => { }); 


        GameObject abilityRange = Object.Instantiate(base.abilityRangePrefab, _spawnPoint, Quaternion.identity);
        SpriteRenderer abilityRangeSprite = abilityRange.GetComponent<SpriteRenderer>();

        Color color = abilityRangeSprite.color;
        color.a = 1f;
        abilityRangeSprite.color = color;

        if (base.autoResize)
        {
            abilityRange.transform.localScale = new Vector2(m_collider2D.bounds.size.x, m_collider2D.bounds.size.y);
            //abilityRangeSprite.size = new Vector2(m_collider2D.bounds.size.x, m_collider2D.bounds.size.y);
        }
        else
        {
            abilityRange.transform.localScale = new Vector2(base.abilityRangeSize.x, base.abilityRangeSize.y);
            //abilityRangeSprite.size = new Vector2(base.abilityRangeSize.x, base.abilityRangeSize.y);
        }

        Vector2 newPosition = abilityRange.transform.position;
        if (AxisXLock)
        {
            newPosition.x = base.initialAbilityRangePosition.transform.position.x;
        }
        if (AxisYLock)
        {
            newPosition.y = base.initialAbilityRangePosition.transform.position.y;
        }
        abilityRange.transform.position = newPosition;

        return abilityRangeSprite.DOFade(0, fadeDuration)
            .OnComplete(() => Object.Destroy(abilityRange));
    }

    private Tween SpawnObject(Vector2 _spawnPoint)
    {
        Debug.Log("생성 위치: " + _spawnPoint);

        GameObject gameObject = Object.Instantiate(abilityPrefab, _spawnPoint, Quaternion.identity);

        return gameObject.transform.DOMove(_spawnPoint + direction, moveSpeed)
            .SetEase(moveEase)
            .OnComplete(() => Object.Destroy(gameObject));
    }
}
