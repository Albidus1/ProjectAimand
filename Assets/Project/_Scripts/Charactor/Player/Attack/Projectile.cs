using System;
using System.Collections;
using UnityEngine;

public class Projectile : MyPoolableObject
{
    [Header("이동 설정")]
    public bool faceDirection = true;
    public bool faceMovementDirection = true;
    public float moveSpeed = 200f;
    public Vector3 direction = Vector3.left;
    public Vector3 flipValue = new Vector3(-1, 1, 1);
    public bool isFacingRight = true;
    public bool directionCanBeChangedBySpawner = true;

    [Header("스폰")]
    [Tooltip("초기 무적 시간 (0이면 무적 없음)")]
    public float initialInvulerablilityDuration = 0f;
    [Tooltip("총알이 소유자에게 피해를 줄지 여부")]
    public bool damageOwner = false;
    [Tooltip("초기화 시 장애물 내부에 있는지 추가 확인 후 조건에 따라 비활성화")]
    public bool spawnSecurtyCheck = false;
    [Tooltip("총알이 충돌할 때 사용할 레이어 마스크")]
    public LayerMask spawnSecurityCheckLayerMask;

    public DamageOnTouch targerDamageOnTouch { get; }


    private GameObject m_owner;
    private Vector3 m_movement;
    private float m_initialSpeed;
    private SpriteRenderer m_spriteRenderer;
    private DamageOnTouch m_damageOnTouch;
    private WaitForSeconds m_initialInvulnerabilityDurationWFS;

    private BoxCollider2D m_boxCollider2D;
    private bool m_facingRight;
    private bool m_initialFlipX;
    private Vector3 m_initialLocalScale;
    private RaycastHit2D m_hit2D;



    private void Awake()
    {
        m_facingRight = isFacingRight;
        m_initialSpeed = moveSpeed;

        m_boxCollider2D = GetComponent<BoxCollider2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_damageOnTouch = GetComponent<DamageOnTouch>();
        m_initialInvulnerabilityDurationWFS = new WaitForSeconds(initialInvulerablilityDuration);

        if (m_spriteRenderer != null)
        {
            m_initialFlipX = m_spriteRenderer.flipX;
        }

        m_initialLocalScale = transform.localScale;
    }

    private IEnumerator InitialInvulnerability()
    {
        if (m_damageOnTouch == null)
        {
            yield break; 
        }

        m_damageOnTouch.ClearIgnoreGameObject();
        m_damageOnTouch.IgnoreGameObject(gameObject);

        yield return m_initialInvulnerabilityDurationWFS;

        if (damageOwner)
        {
            m_damageOnTouch.RemoveIgnoringObject(gameObject);
        }
    }

    private void Initialization()
    {
        moveSpeed = m_initialSpeed;
        isFacingRight = m_facingRight;

        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.flipX = m_initialFlipX;
        }

        transform.localScale = m_initialLocalScale;

        CheckForCollider();
    }

    private void CheckForCollider()
    {
        if (false == spawnSecurtyCheck)
        {
            return;
        }

        if (m_boxCollider2D == null)
        {
            return;
        }

        m_hit2D = Physics2D.BoxCast(transform.position, m_boxCollider2D.bounds.size, transform.eulerAngles.z, Vector3.forward, 1f, spawnSecurityCheckLayerMask);
        if (m_hit2D)
        {
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        Movement();
        HandleFaceMovement();
    }

    private void Movement()
    {
        m_movement = direction * (moveSpeed * 0.1f) * Time.deltaTime;
        transform.Translate(m_movement, Space.World);
    }

    private void HandleFaceMovement()
    {
        if (false == faceMovementDirection)
        {
            return;
        }

        if (m_movement != Vector3.zero)
        {
            float angle = Mathf.Atan2(m_movement.y, m_movement.x) * Mathf.Rad2Deg;

            if (m_movement.x < 0)
            {
                transform.right = -m_movement.normalized;
            }
            else
            {
                transform.right = m_movement.normalized;
            }
        }
    }

    public void SetDirection(Vector3 _newDirection, Quaternion _newRotation, bool _spawnerIsFacingRight = true)
    {
        if (directionCanBeChangedBySpawner)
        {
            direction = _newDirection;
        }
        if (isFacingRight != _spawnerIsFacingRight)
        {
            Flip();
        }
        if (faceDirection)
        {
            transform.rotation = _newRotation;
        }
    }

    private void Flip()
    {
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }
        else
        {
            transform.localScale = Vector3.Scale(transform.localScale, flipValue);
        }
    }

    public void SetOwner(GameObject _newOwner)
    {
        m_owner = _newOwner;
        
        DamageOnTouch damageOnTouch = gameObject.GetComponent<DamageOnTouch>();

        if (damageOnTouch != null)
        {
            damageOnTouch.owner = _newOwner;

            if (false == damageOwner)
            {

            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Initialization();

        if (initialInvulerablilityDuration > 0f)
        {
            StartCoroutine(InitialInvulnerability());
        }

        if (m_damageOnTouch != null)
        {
            
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        if (m_damageOnTouch != null)
        {

        }
    }



    /*    public LayerMask obstacleLayerMask;

   public float speed = 10f; // 총알 이동 속도
   public float damage = 10;

   public bool isPlayerBullet = false;

   public float maxLifeTime = 5f; // 총알 자동 삭제 시간

   private Vector3 moveDirection; // 이동 방향

   public void SetDirection(Vector3 dir)
   {
       moveDirection = dir.normalized;
       transform.right = moveDirection; // Sprite가 바라보는 방향도 조정
       Debug.Log("[Bullet] SetDirection: " + moveDirection);
   }

   //private void Start()
   //{
   //    Destroy(gameObject, maxLifeTime); // 최대 생존 시간 초과 시 제거
   //}

   //private void Update()
   //{
   //    transform.position += moveDirection * speed * Time.deltaTime;

   //    // 화면 밖이면 제거
   //    Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
   //    if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
   //    {
   //        Destroy(gameObject);
   //    }
   //}

   private void OnTriggerEnter2D(Collider2D collision)
   {
       string tag = isPlayerBullet ? "Enemy" : "Player";

       if (collision.CompareTag(tag))
       {
           Health enemy = collision.GetComponent<Health>();
           if (enemy != null)
           {
               enemy.currentHP -= damage;
           }

           gameObject.SetActive(false);
           return;
       }

       if (MyLayers.LayerInLayerMask(collision.gameObject.layer, obstacleLayerMask))
       {
           gameObject.SetActive(false);
       }
   }*/
}
