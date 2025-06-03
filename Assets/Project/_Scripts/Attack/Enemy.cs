using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 10; // 최대 체력
    private int currentHealth;  // 현재 체력

    void Start()
    {
        currentHealth = maxHealth; // 최대 체력 초기화
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
}
