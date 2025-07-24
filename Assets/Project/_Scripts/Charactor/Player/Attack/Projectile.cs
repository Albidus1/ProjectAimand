using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // 총알 이동 속도
    public int damage = 10; 
    public float maxLifeTime = 5f; // 총알 자동 삭제 시간

    private Vector3 moveDirection; // 이동 방향

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
        transform.right = moveDirection; // Sprite가 바라보는 방향도 조정
        Debug.Log("[Bullet] SetDirection: " + moveDirection);
    }

    private void Start()
    {
        Destroy(gameObject, maxLifeTime); // 최대 생존 시간 초과 시 제거
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        // 화면 밖이면 제거
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Health enemy = collision.GetComponent<Health>();
            if (enemy != null)
            {
                enemy.Damaged(damage);
            }

            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
