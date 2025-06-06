using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab; // 발사할 총알 프리팹
    public float chargeTime = 2f; // 공격이 발사되기 위한 최소 차징 시간
    public Transform firePoint; // 총알이 생성되느 ㄴ위치

    private float currentCharge = 0f;
    private bool isCharging = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            if (!isCharging)
                Debug.Log("차징 시작");

            isCharging = true;
            currentCharge += Time.deltaTime;
            Debug.Log($"차징 중... {currentCharge:F2}s");
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            Debug.Log($"A 키 뗌! 총 충전 시간: {currentCharge:F2}s");

            if (isCharging && currentCharge >= chargeTime)
            {
                Debug.Log("FireProjectile() 호출됨!");
                FireProjectile();
            }

            currentCharge = 0f;
            isCharging = false;
        }
    }

    void FireProjectile()
    {
        // 총알 생성
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 바라보는 방향 설정 (localScale.x 기준으로 왼쪽/오른쪽 판단)
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // 총알에 방향 지정
        bullet.GetComponent<Bullet>().SetDirection(direction);
    }
}
