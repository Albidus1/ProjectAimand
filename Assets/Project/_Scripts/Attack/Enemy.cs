using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    [Header("공격력 설정")]
    public int attackDamage = 5; // 플레이어에게 줄 데미지

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"[Enemy] 체력: {currentHealth}");


        if (currentHealth <= 0)
        {
            Debug.Log("[Enemy] 사망");
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            // ✅ 플레이어 체력 감소
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.currentHP -= attackDamage;
                Debug.Log("[Enemy] 플레이어에게 피해를 줌");
            }
        }
    }
}
