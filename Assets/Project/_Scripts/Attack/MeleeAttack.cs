using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Range(1f, 360f)] public float viewAngle = 90f;
    public float viewRadius = 5f;
    public int resolution = 30;
    public int damage = 20;

    private GameObject fovMeshObject;
    private FieldOfViewMesh2D fovScript;

    private float lastAttackTime = 0f;          // 마지막 공격 시간 기록용
    public float attackInterval = 0.5f;         // 공격 주기 (초)

    void Start()
    {
        // FOV Mesh 시각화용 자식 생성
        fovMeshObject = new GameObject("FOV_Mesh");
        fovMeshObject.transform.SetParent(transform);
        fovMeshObject.transform.localPosition = new Vector3(0, 0, -0.1f);

        var mf = fovMeshObject.AddComponent<MeshFilter>();
        var mr = fovMeshObject.AddComponent<MeshRenderer>();
        fovScript = fovMeshObject.AddComponent<FieldOfViewMesh2D>();

        // 시야 설정 전달
        fovScript.viewAngle = viewAngle;
        fovScript.viewRadius = viewRadius;
        fovScript.resolution = resolution;

        // 머티리얼
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0f, 0f, 0.3f); // 붉은 반투명
        mr.material = mat;

        // Sprite보다 뒤에 그리기
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        mr.sortingOrder = sr != null ? sr.sortingOrder - 1 : -10;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            // 부채꼴 표시
            fovScript.DrawFOV();

            // 마지막 공격으로부터 0.5초가 지났다면 다시 공격
            if (Time.time >= lastAttackTime + attackInterval)
            {
                DetectAndDamageEnemies();
                Debug.Log("근거리 공격 완료"); // 콘솔에 출력됨
                lastAttackTime = Time.time;
            }
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            // 키에서 손 떼면 부채꼴 숨기기
            fovScript.ClearMesh();
        }
    }

    void DetectAndDamageEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.right, dirToTarget); // 오른쪽 기준

                if (angleToTarget < viewAngle / 2f)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }
            }
        }
    }
}
