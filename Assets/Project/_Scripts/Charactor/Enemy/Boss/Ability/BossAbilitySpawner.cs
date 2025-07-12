using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;



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


    protected List<GameObject> m_spawnedObjects = new List<GameObject>();
    protected int m_spawnedObjectCount = 3;


    private void Awake()
    {
        m_collider2D = GetComponent<Collider2D>();
    }

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
        if (abilityPrefab == null)
        {
            Debug.LogWarning("Ability Prefab is not assigned.");
            yield break;
        }

        base.isAbilityActive = true;
        base.isOnCooldown = (false == base.afterCooldown);

        m_spawnedObjectCount = base.abilityActCount;

        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);
        MoveObjects();
        yield return new WaitForSeconds(0.5f);

        base.isAbilityActive = false;
        if (false == base.isOnCooldown)
        {
            base.isOnCooldown = true;
        }
    }

    private Vector2 SetSpawnPoint()
    {
        float posX = m_collider2D.bounds.center.x;
        float posY = m_collider2D.bounds.center.y;
        if (randomSpawnPoint)
        {
            posX = Random.Range(m_collider2D.bounds.min.x, m_collider2D.bounds.max.x);
            posY = Random.Range(m_collider2D.bounds.min.y, m_collider2D.bounds.max.y);
        }

        return new Vector2(posX, posY);
    }

    protected void AbilityRangeVisualizer()
    {
        if (base.abilityRangePrefab == null)
        {
            //return DOVirtual.DelayedCall(0.01f, () => { });
            return;
        }

        m_spawnedObjects.Clear();

        for (int i = 0; i < m_spawnedObjectCount; i++)
        {
            Vector2 spawnPos = SetSpawnPoint();

            GameObject indicator = Instantiate(base.abilityRangePrefab, spawnPos, Quaternion.identity);
            SpriteRenderer indicatorRenderer = indicator.GetComponent<SpriteRenderer>();

            if (base.autoResize)
            {
                BoxCollider2D spawnObj = abilityPrefab.GetComponent<BoxCollider2D>();

                indicator.transform.localScale = new Vector2(spawnObj.bounds.size.x, spawnObj.bounds.size.y);
                //abilityRangeSprite.size = new Vector2(m_collider2D.bounds.size.x, m_collider2D.bounds.size.y);
            }
            else
            {
                indicator.transform.localScale = new Vector2(base.abilityRangeSize.x, base.abilityRangeSize.y);
                //abilityRangeSprite.size = new Vector2(base.abilityRangeSize.x, base.abilityRangeSize.y);
            }

            Vector2 newPosition = indicator.transform.position;
            if (AxisXLock)
            {
                newPosition.x = base.initialAbilityRangePosition.transform.position.x;
            }
            if (AxisYLock)
            {
                newPosition.y = base.initialAbilityRangePosition.transform.position.y;
            }
            indicator.transform.position = newPosition;

            indicatorRenderer.DOFade(0, base.fadeDuration)
                .OnComplete(() => Destroy(indicator, base.fadeDuration));

            GameObject obj = Instantiate(abilityPrefab, spawnPos, Quaternion.identity);
            obj.SetActive(false);
            m_spawnedObjects.Add(obj);
        }
    }

    private void MoveObjects()
    {
        foreach (GameObject obj in m_spawnedObjects)
        {
            obj.SetActive(true);

            if (direction != Vector2.zero)
            {
                obj.transform.DOMove((Vector2)obj.transform.position + direction, moveSpeed)
                    .SetEase(moveEase)
                    .OnComplete(() => Object.Destroy(obj));
            }
            else
            {
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

                spriteRenderer.DOFade(0.4f, disableTime)
                    .OnComplete(() => Object.Destroy(obj));
            }
        }
    }

    //private void SpawnObject()
    //{
    //    //Debug.Log("생성 위치: " + m_spawnPoint);

    //    GameObject gameObject = Object.Instantiate(abilityPrefab, m_spawnPoint, Quaternion.identity);

    //    gameObject.transform.DOMove(m_spawnPoint + direction, moveSpeed)
    //        .SetEase(moveEase)
    //        .OnComplete(() => Object.Destroy(gameObject));
    //}
}
