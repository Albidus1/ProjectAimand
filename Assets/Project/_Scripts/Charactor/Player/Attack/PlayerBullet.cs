using UnityEngine;

public class PlayerBullet : Product
{
    private float damage;
    private float maxFlyTime;
    private float speed;

    private float flyTime;

    private bool isHit = false;

    private void OnEnable()
    {
        flyTime = 0f;
        isHit = false;
    }

    public void Initialize(float damage, float maxFlyTime, float speed) =>
        (this.damage, this.maxFlyTime, this.speed) = (damage, maxFlyTime, speed);

    private void Update()
    {
        // X축 방향으로 날아감
        this.transform.position += this.transform.right * speed * Time.deltaTime;
        flyTime += Time.deltaTime;

        // 날아가는 최대 시간 경과시 비활성화
        if (flyTime >= maxFlyTime)
            this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit) return; // 이미 무언가와 한번 충돌되었으면 나머지 충돌은 무시

        // Physics 세팅에서 무시되는 레이어는 스킵
        if (Physics.GetIgnoreLayerCollision(collision.gameObject.layer, this.gameObject.layer)) return;
        Debug.Log($"총알 충돌: {collision.gameObject.name}");

        // 적 총알과 충돌시 상대 총알과 내 총알 비활성화
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy_Bullet"))
        {
            collision.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
            isHit = true;
        }
        // 적과 충돌시 적에게 대미지
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            enemyHealth.currentHP -= damage;
            this.gameObject.SetActive(false);
            isHit = true;
        }
        // 그 외의 물체와 충돌시 나만 비활성화
        else
        {
            this.gameObject.SetActive(false);
            // 그 외 물체에 부딪혀서 비활성화 되었을 경우 겹치는 물체가 있으면 관련 처리가 되어야 하기 때문에 isHit를 건들이진 않는다
        }
    }
}
