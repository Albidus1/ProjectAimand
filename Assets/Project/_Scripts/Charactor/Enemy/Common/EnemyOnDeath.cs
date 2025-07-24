using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyOnDeath : MonoBehaviour
{
    [SerializeField]
    private bool m_spawnable = false; // 리스폰 가능 여부
    [SerializeField]
    private Transform m_spawnPoint; // 몬스터용 리스폰 스크립트 필요 (checkpoint 같은)

    void Start()
    {
        GetComponent<Health>().OnDeathEvent.AddListener(OnDeathEventFunc);
    }

    private IEnumerator OnDeath()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.excludeLayers = ~(1 << LayerMask.NameToLayer("Platform") | 1 << LayerMask.NameToLayer("Platform_Magnet"));

        yield return new WaitForSeconds(3f);

        // 리스폰할 경우
        if (m_spawnable && m_spawnPoint != null)
        {
            this.transform.position = m_spawnPoint.position;
            col.excludeLayers = 0;
            yield break;
        }

        Destroy(this.gameObject);
    }

    private void OnDeathEventFunc()
    {
        StartCoroutine(OnDeath());
    }
}
