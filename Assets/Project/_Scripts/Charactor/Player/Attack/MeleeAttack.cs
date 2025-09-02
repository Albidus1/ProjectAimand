using UnityEngine;
using static PlayerStates;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Range(1f, 360f)] public float viewAngle = 90f;
    public float viewRadius = 5f;
    public int resolution = 30;

    [Header("지상 근거리 공격 데미지 (더블 공격)")]
    [SerializeField] private float baseDamage = 20f;      // 기본 1타
    [SerializeField] private float enhancedDamage = 40f;  // 강화 2타
    private float currentDamage = 0f;

    [Header("근거리 공격 유지창 설정 (더블 공격 윈도우)")]
    [SerializeField] private float sustainDuration = 0.6f;
    private bool sustainActive = false;
    private float sustainEndTime = 0f;
    private bool upgradedThisSustain = false;

    private GameObject fovMeshObject;
    private FieldOfViewMesh2D fovScript;

    private PlayerMovement playerMovement;
    private Health playerHealth;

    // flipX 기준 좌/우 판별용
    private SpriteRenderer sr;

    [Header("하강 공격 설정")]
    public float downwardAttackDamage = 10f;
    public float downwardAttackSpeed = -15f;
    public float maxFallSpeed = -40f;
    private bool downwardAttacking = false;

    [SerializeField] private float invincibleDurationAfterHit = 0.2f; // 낙하 타격 후 무적 해제 딜레이

    [Header("넉백 설정")]
    public Vector2 knockbackForce = new Vector2(10f, 2f);

    private Health m_health;


    void Start()
    {
        fovMeshObject = new GameObject("FOV_Mesh");
        fovMeshObject.transform.SetParent(transform);
        fovMeshObject.transform.localPosition = new Vector3(0, 0, -0.1f);

        var mf = fovMeshObject.AddComponent<MeshFilter>();
        var mr = fovMeshObject.AddComponent<MeshRenderer>();
        fovScript = fovMeshObject.AddComponent<FieldOfViewMesh2D>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0f, 0f, 0.3f);
        mr.material = mat;

        sr = GetComponent<SpriteRenderer>();
        mr.sortingOrder = sr != null ? sr.sortingOrder - 1 : -10;

        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponentInParent<Health>();
    }

    void Update()
    {
        MovementStates playerState = GetPlayerMovementState();

        if (Input.GetKeyDown(KeyCode.A))
        {
            // 비활성화: 위공격
            /*
            if (playerState == MovementStates.Jumping)
            {
                PerformUpwardAttack();
                Debug.Log("점프 중 위쪽 타격");
                return;
            }
            */

            if (playerState == MovementStates.Falling)
            {
                PerformDownwardAttack();
                Debug.Log("낙하 공격 실행");
                return;
            }

            // 지상 상태: 유지창 로직(더블 공격)
            if (!sustainActive)
            {
                StartSustainGroundAttack();  // 유지창 시작(1타)
            }
            else
            {
                TryUpgradeSustain();         // 유지창 동안 1회 강화 입력 허용(2타)
            }
        }

        // 유지창 종료 처리
        if (sustainActive && Time.time >= sustainEndTime)
        {
            EndSustain();
        }
    }

    // PlayerMovement 스크립트에 점프/낙하 스테이트가 구현이 안되어 있어서 임시 조치 함수
    private MovementStates GetPlayerMovementState()
    {
        if (playerMovement == null) throw new System.Exception("PlayerMovement 스크립트가 없습니다.");
        float verticalSpeed = playerMovement.rb.linearVelocity.y;

        MovementStates tempState = verticalSpeed switch
        {
            > 0.1f => MovementStates.Jumping,
            < -0.1f => MovementStates.Falling,
            _ => MovementStates.Idle
        };

        return tempState;
    }

    // flipX 우선, 없으면 localScale.x 백업
    private Vector2 GetFacingDir()
    {
        if (sr != null)
        {
            // flipX == true 면 왼쪽을 봄
            return sr.flipX ? Vector2.left : Vector2.right;
        }
        return transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
    }

    // 메쉬 오브젝트를 방향 벡터에 맞게 회전(기본이 오른쪽 가정)
    private void AlignFOVToDirection(Vector2 dir)
    {
        if (fovMeshObject == null) return;
        if (dir == Vector2.zero) dir = Vector2.right;
        fovMeshObject.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        fovMeshObject.transform.position = transform.position;
    }

    // 지상 공격: 유지창 시작(1타)
    private void StartSustainGroundAttack()
    {
        sustainActive = true;
        upgradedThisSustain = false;
        sustainEndTime = Time.time + sustainDuration;

        // 기본 공격력 세팅
        currentDamage = baseDamage;

        // 현재 바라보는 방향 사용 + 메쉬 회전
        Vector2 attackDir = GetFacingDir();
        AlignFOVToDirection(attackDir);
        fovScript.DrawFOV(viewRadius, viewAngle, resolution, attackDir);

        // 1회 판정
        DetectAndDamageEnemies(currentDamage);

        // 모션 플래그(사용 중인 시스템에 맞게)
        if (playerMovement != null) playerMovement.isAttacking = true;

        Debug.Log("지상 근거리 공격 1회 발동");
    }

    // 지상 공격: 유지창 강화(2타)
    private void TryUpgradeSustain()
    {
        if (upgradedThisSustain) return;

        upgradedThisSustain = true;
        currentDamage = enhancedDamage;

        // 강화 입력 시점의 바라보는 방향으로 재정렬
        Vector2 attackDir = GetFacingDir();
        AlignFOVToDirection(attackDir);
        fovScript.DrawFOV(viewRadius, viewAngle, resolution, attackDir);

        // 강화 판정 1회
        DetectAndDamageEnemies(currentDamage);

        Debug.Log("지상 근거리 공격 2회(강화) 발동");
    }

    // 지상 공격: 유지창 종료
    private void EndSustain()
    {
        sustainActive = false;
        upgradedThisSustain = false;
        currentDamage = 0f;

        fovScript.ClearMesh();

        if (playerMovement != null) playerMovement.isAttacking = false;

        Debug.Log("지상 근거리 유지창 종료");
    }

    // 공통 판정: FOV 내 타격 + 넉백 적용
    private void DetectAndDamageEnemies(float dmg)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewRadius);

        // 판정용 바라보는 방향도 동일 함수 사용
        Vector3 facingDirection = (Vector3)GetFacingDir();
        float facingSign = Mathf.Sign(facingDirection.x == 0 ? 1f : facingDirection.x);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(facingDirection, dirToTarget);

                if (angleToTarget <= viewAngle / 2f)
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.blue, 0.5f);

                    Health enemyHealth = hit.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Damage(dmg, 0.1f);
                        enemyHealth.ApplyKnockback(this.gameObject, knockbackForce);
                    }

                    // 공격을 1회만 하기 위한 강제 종료(요구사항 유지)
                    // 바로 빠지면 다중 공격이 안됩니다.
                    //return;
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.gray, 0.5f);
                }
            }
        }
    }

    // 위공격 함수 전체 주석 처리
    /*
    private void PerformUpwardAttack()
    {
        float attackRange = 2f;
        float attackAngle = 60f;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 1.0f;
        Vector2 attackDirection = Vector2.up;

        AlignFOVToDirection(attackDirection);
        fovScript.DrawFOV(attackRange, attackAngle, resolution, attackDirection);
        fovMeshObject.transform.position = transform.position + Vector3.up * 1.0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector2 toTarget = ((Vector2)hit.transform.position - origin).normalized;
                float dot = Vector2.Dot(attackDirection, toTarget);
                float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

                if (angle <= attackAngle / 2f)
                {
                    Health enemyHealth = hit.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Damage(baseDamage, 0.1f);
                        Debug.DrawLine(origin, hit.transform.position, Color.red, 0.5f);
                        Debug.Log("위쪽 적에게 공격 성공!");
                    }
                }
                else
                {
                    Debug.DrawLine(origin, hit.transform.position, Color.gray, 0.5f);
                }
                return; // 공격을 1회만 하기 위한 강제 종료
            }
        }
    }
    */

    // 하강 공격
    private void PerformDownwardAttack()
    {
        if (downwardAttacking) return;

        Debug.Log("낙하 공격 시작");

        downwardAttacking = true;
        StartCoroutine(CheckAfterPerformDownardAttack());

        Vector2 currentVelocity = playerMovement.rb.linearVelocity;
        float boostedFallSpeed = currentVelocity.y + downwardAttackSpeed;
        boostedFallSpeed = Mathf.Max(boostedFallSpeed, maxFallSpeed);
        playerMovement.rb.linearVelocity = new Vector2(currentVelocity.x, boostedFallSpeed);

        if (playerHealth != null)
        {
            playerHealth.invulnerable = true;
            Debug.Log("낙하 공격 중 무적 활성화");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!downwardAttacking) return;

        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.Damage(downwardAttackDamage, 0.1f); // 몬스터는 데미지 받음

                // 넉백(하강 충돌 시에도 적용)
                if (other.attachedRigidbody != null)
                {
                    float facingSign = Mathf.Sign(GetFacingDir().x == 0 ? 1f : GetFacingDir().x);
                    Vector2 force = new Vector2(knockbackForce.x * facingSign, knockbackForce.y);
                    other.attachedRigidbody.AddForce(force, ForceMode2D.Impulse);
                }

                // 하강 공격시 무적 상태 invincibleDurationAfterHit초 후 풀리게
                if (playerHealth != null && playerHealth.invulnerable)
                {
                    Debug.Log("적과 부딪힘 → 무적 해제 시작");
                    StartCoroutine(DisableInvincibilityAfterDelay());
                }
            }

            // 공격을 1회만 하기 위한 강제 종료
            return;
        }
    }

    // 하강 공격 후 지면에 닿았는지 체크용
    private IEnumerator CheckAfterPerformDownardAttack()
    {
        while (true)
        {
            if (playerMovement.lastOnGroundTime > 0 && GetPlayerMovementState() == MovementStates.Idle)
                break;
            yield return null;
        }

        if (playerHealth != null) playerHealth.invulnerable = false;
        downwardAttacking = false;
        Debug.Log("낙하 공격 종료");
    }

    private IEnumerator DisableInvincibilityAfterDelay()
    {
        yield return new WaitForSeconds(invincibleDurationAfterHit);
        if (playerHealth != null) playerHealth.invulnerable = false;
        Debug.Log("무적 해제 및 점프 상태 초기화됨");
    }
}
