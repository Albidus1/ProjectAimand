using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Range(1f, 360f)] public float viewAngle = 90f;     // 부채꼴 각도
    public float viewRadius = 5f;                        // 부채꼴 반지름
    public int resolution = 30;                          // 부채꼴 세분화 정도
    public int damage = 20;                              // 데미지량

    public float attackInterval = 0.5f;                  // 공격 주기
    private float lastAttackTime = 0f;                   // 마지막 공격 시간

    private GameObject fovMeshObject;                    // 부채꼴 시각화 오브젝트
    private FieldOfViewMesh2D fovScript;                 // 부채꼴 메쉬 그리는 스크립트

    void Start()
    {
        // 부채꼴 시각화용 오브젝트 생성 및 설정
        fovMeshObject = new GameObject("FOV_Mesh");
        fovMeshObject.transform.SetParent(transform);
        fovMeshObject.transform.localPosition = new Vector3(0, 0, -0.1f);

        // MeshRenderer 및 Filter 구성
        var mf = fovMeshObject.AddComponent<MeshFilter>();
        var mr = fovMeshObject.AddComponent<MeshRenderer>();
        fovScript = fovMeshObject.AddComponent<FieldOfViewMesh2D>();

        // 머티리얼 설정 (반투명 빨강)
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0f, 0f, 0.3f);
        mr.material = mat;

        // SpriteRenderer 뒤에 렌더링되도록 정렬
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        mr.sortingOrder = sr != null ? sr.sortingOrder - 1 : -10;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            // 실시간으로 반지름, 각도, 해상도 전달
            fovScript.DrawFOV(viewRadius, viewAngle, resolution);

            // 공격 쿨타임 체크
            if (Time.time >= lastAttackTime + attackInterval)
            {
                DetectAndDamageEnemies();
                Debug.Log("근거리 공격 완료");
                lastAttackTime = Time.time;
            }
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            // 키에서 손 뗐을 때 Mesh 숨기기
            fovScript.ClearMesh();
        }
    }


    // 범위 내 적을 감지하고 데미지를 주는 함수
    void DetectAndDamageEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewRadius);

        // 여기서 실제 바라보는 방향을 계산
        Vector3 facingDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(facingDirection, dirToTarget); // ← 수정된 부분

                if (angleToTarget < viewAngle / 2f)
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.blue, 0.5f);

                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.gray, 0.5f);
                }
            }
        }
    }
}
