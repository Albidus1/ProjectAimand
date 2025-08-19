using System;
using System.Collections;
using UnityEngine;

public class Projectile : MyPoolableObject
{
    [Header("이동 설정")]
    public bool faceDirection = true;
    public bool faceMovementDirection = true;
    public float moveSpeed = 200f;
    public Vector3 direction = Vector3.up;
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


    protected GameObject m_owner;
    protected Vector3 m_movement;
    protected float m_initialSpeed;
    protected SpriteRenderer m_spriteRenderer;
    protected DamageOnTouch m_damageOnTouch;
    protected WaitForSeconds m_initialInvulnerabilityDurationWFS;

    protected BoxCollider2D m_boxCollider2D;
    protected bool m_facingRight;
    protected bool m_initialFlipX;
    protected Vector3 m_initialLocalScale;
    protected RaycastHit2D m_hit2D;



    protected virtual void Awake()
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

    protected virtual IEnumerator InitialInvulnerability()
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

    protected virtual void Initialization()
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

    protected virtual void CheckForCollider()
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

    protected virtual void FixedUpdate()
    {
        Movement();
        HandleFaceMovement();
    }

    protected virtual void Movement()
    {
        m_movement = direction * (moveSpeed * 0.1f) * Time.deltaTime;
        transform.Translate(m_movement, Space.World);
    }

    protected virtual void HandleFaceMovement()
    {
        if (false == faceMovementDirection)
        {
            return;
        }

        if (m_movement != Vector3.zero)
        {
            //float angle = Mathf.Atan2(m_movement.y, m_movement.x) * Mathf.Rad2Deg;

            //if (m_movement.x < 0)
            //{
            //    transform.right = -m_movement.normalized;
            //}
            //else
            //{
            //    transform.right = m_movement.normalized;
            //}

            transform.rotation = Quaternion.LookRotation(Vector3.forward, m_movement);
        }
    }

    public virtual void SetDirection(Vector3 _newDirection, Quaternion _newRotation, bool _spawnerIsFacingRight = true)
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

    protected virtual void Flip()
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

    public virtual void SetOwner(GameObject _newOwner)
    {
        m_owner = _newOwner;


        if (TryGetComponent<DamageOnTouch>(out var damageOnTouch))
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
}