using UnityEngine;
using static PlayerStates;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 범위 설정")]
    [Range(1f, 360f)] public float viewAngle = 90f;
    public float viewRadius = 5f;
    public int resolution = 30;
    public int damage = 20;

    public float attackInterval = 0.5f;
    private float lastAttackTime = 0f;

    private GameObject fovMeshObject;
    private FieldOfViewMesh2D fovScript;
    [SerializeField]
    private float fovDisplayTime = 0.5f;

    private PlayerMovement playerMovement;

    [Header("하강 공격 설정")]
    public float downwardAttackDamage = 10f;
    public float downwardAttackSpeed = -15f;
    public float maxFallSpeed = -40f;
    private bool downwardAttacking = false;

    [SerializeField] private float invincibleDurationAfterHit = 0.2f;
    private Health playerHealth;

    private float pressButtonTime;

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

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        mr.sortingOrder = sr != null ? sr.sortingOrder - 1 : -10;

        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponentInParent<Health>();
    }

    void Update()
    {
        MovementStates playerState = GetPlayerMovementState();

        if (Input.GetKeyDown(KeyCode.A))
        {
            // 버튼 누른 시각
            pressButtonTime = Time.time;

            if (playerState == MovementStates.Jumping)
            {
                PerformUpwardAttack();
                Debug.Log("점프 중 위쪽 타격");
                return;
            }

            if (playerState == MovementStates.Falling)
            {
                PerformDownwardAttack();
                Debug.Log("낙하 공격 실행");
                return;
            }

            // 지상 근거리 공격
            Vector2 attackDir = transform.localScale.x > 0 ? Vector2.right : Vector2.right;
            fovScript.DrawFOV(viewRadius, viewAngle, resolution, attackDir);
            fovMeshObject.transform.position = transform.position;

            if (Time.time >= lastAttackTime + attackInterval)
            {
                DetectAndDamageEnemies();
                Debug.Log("지상 근거리 공격 완료");
                lastAttackTime = Time.time;

                playerMovement.isAttacking = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            fovScript.ClearMesh();
        }

        // 근접 공격 범위가 활성화 되어 있을때 버튼을 누르고 있어도 일정 시간 지나면 비활성화
        if (fovScript.IsActivating)
        {
            if (Time.time - pressButtonTime > fovDisplayTime)
            {
                fovScript.ClearMesh();
            }
        }
    }

    // PlayerMovement 스크립트에 점프와 떨어지는 스테이트가 구현이 안되어 있어서 임시 조치 함수
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

    private void DetectAndDamageEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, viewRadius);
        Vector3 facingDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(facingDirection, dirToTarget);

                if (angleToTarget < viewAngle / 2f)
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.blue, 0.5f);
                    Health enemyHealth = hit.GetComponent<Health>();
                    if (enemyHealth != null) 
                    {
                        enemyHealth.Damage(damage, 0.5f); 
                    }
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.transform.position, Color.gray, 0.5f);
                }
            }
        }
    }

    private void PerformUpwardAttack()
    {
        float attackRange = 2f;
        float attackAngle = 60f;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 1.0f;
        Vector2 attackDirection = Vector2.up;

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
                        enemyHealth.Damage(damage, 0.5f);
                        Debug.DrawLine(origin, hit.transform.position, Color.red, 0.5f);
                        Debug.Log("위쪽 적에게 공격 성공!");
                    }
                }
                else
                {
                    Debug.DrawLine(origin, hit.transform.position, Color.gray, 0.5f);
                }
            }
        }
    }

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
            playerHealth.invincible = true;
            Debug.Log("낙하 공격 중 무적 활성화");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(downwardAttacking == true)
        {
            if (other.CompareTag("Enemy"))
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                    {
                    enemyHealth.Damage(downwardAttackDamage, 0.1f); ; // 몬스터는 데미지 받음

                        // 하강 공격시 무적 상태 {invincibleDurationAfterHit}초 후 풀리게
                        if (downwardAttacking) {
                        if (playerHealth != null && playerHealth.invincible)
                        {
                            Debug.Log("적과 부딪힘 → 무적 해제 시작");
                            StartCoroutine(DisableInvincibilityAfterDelay());
                        }
                    }
                }
                return; // 공격을 1회만 하기 위한 강제 종료
            }
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

        playerHealth.invincible = false;
        downwardAttacking = false;
    }

    private IEnumerator DisableInvincibilityAfterDelay()
    {
        yield return new WaitForSeconds(invincibleDurationAfterHit);
        playerHealth.invincible = false;

        Debug.Log("무적 해제 및 점프 상태 초기화됨");
    }
}
