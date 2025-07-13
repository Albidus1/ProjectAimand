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
        if (base.abilityPrefab == null)
        {
            Debug.LogWarning("능력 오브젝트 없음");
            yield break;
        }

        base.isAbilityActive = true;
        base.isOnCooldown = (false == base.afterCooldown);

        m_spawnedObjectCount = base.abilityActCount;

        //Debug.Log("스폰 능력 사용_" + transform.name);

        AbilityRangeVisualizer();
        yield return new WaitForSeconds(base.fadeDuration);
        ObjectsActivate();
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

    protected override void AbilityRangeVisualizer()
    {
        if (base.abilityRangePrefab == null)
        {
            //return DOVirtual.DelayedCall(0.01f, () => { });
            return;
        }

        m_spawnedObjects.Clear();

        for (int i = 0; i < m_spawnedObjectCount; i++)
        {
            m_spawnPoint = SetSpawnPoint();

            base.AbilityRangeVisualizer();

            GameObject obj = Instantiate(base.abilityPrefab, m_spawnPoint, Quaternion.identity);
            obj.SetActive(false);
            m_spawnedObjects.Add(obj);
        }
    }

    private void ObjectsActivate()
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
}
