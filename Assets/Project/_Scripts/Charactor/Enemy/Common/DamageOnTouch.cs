using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    public bool killPlayer = false;

    [Header("대상")]
    public LayerMask targetLayerMask;
    public bool applyDamageOnTriggerEnter = true;
    public bool applyDamageOnTriggerStay = true;

    [Header("주는 데미지")]
    public float damage = 10f;

    [Header("받는 데미지")]
    public float damageTakenEveryTime = 0f;
    public float damageTakenDamageable = 0f;
    public float damageTakenNonDamageable = 0f;
    public float invincibilityDuration = 0.5f;

    [Header("넉백")]
    public Vector2 damageCausedKnockbackForce = new Vector2(10, 2);

    [MyReadOnly]
    public GameObject owner;


    public delegate void OnHitDelegate();
    public OnHitDelegate OnHit;
    public OnHitDelegate OnHitDamageable;
    public OnHitDelegate OnHitNonDamageable;
    public OnHitDelegate OnKill;

    private Vector2 m_lastPosition;
    private Vector2 m_lastDamagePosition;
    private Vector2 m_velocity;
    private Vector2 m_knockbackForce;
    private Vector2 m_damageDirection;
    private float m_startTime;
    private List<GameObject> m_ignoredGameObjects;
    private Health m_colliderHealth;
    private Health m_health;
    private CharacterMovement m_characterMovement;
    private Collider2D m_collideingCollider;
    private BoxCollider2D m_boxCollider2D;
    private CircleCollider2D m_circleCollider2D;
    private bool m_doKnockback;


    private void Awake()
    {
        if (m_ignoredGameObjects == null)
        {
            m_ignoredGameObjects = new List<GameObject>();
        }

        owner = this.gameObject;
        m_health = GetComponent<Health>();
        m_characterMovement = GetComponent<CharacterMovement>();

        m_boxCollider2D = GetComponent<BoxCollider2D>();
        m_circleCollider2D = GetComponent<CircleCollider2D>();

        m_lastPosition = transform.position;
        m_lastDamagePosition = transform.position;
    }

    private void OnEnable()
    {
        m_startTime = Time.time;
        m_lastPosition = transform.position;
        m_lastDamagePosition = transform.position;
    }

    private void OnDisable()
    {
        ClearIgnoreGameObject();
    }

    public void IgnoreGameObject(GameObject _newIgnoredGameObject)
    {
        if (m_ignoredGameObjects == null)
        {
            m_ignoredGameObjects = new List<GameObject>();
        }

        m_ignoredGameObjects.Add(_newIgnoredGameObject);
    }

    public void RemoveIgnoringObject(GameObject _ignoredGameObject)
    {
        m_ignoredGameObjects.Remove(_ignoredGameObject);
    }

    public void ClearIgnoreGameObject()
    {
        if (m_ignoredGameObjects != null)
        {
            m_ignoredGameObjects.Clear();
        }
    }

    private void Update()
    {
        if (m_doKnockback)
        {
            m_startTime -= Time.deltaTime;

            if (m_startTime < 0)
            {
                m_doKnockback = false;
            }
        }

        ComputeVelocity();
    }

    private void ComputeVelocity()
    {
        m_velocity = (m_lastPosition - (Vector2)transform.position) / Time.deltaTime;

        if (Vector2.Distance(m_lastDamagePosition, transform.position) > 0.1f)
        {
            m_damageDirection = (Vector2)transform.position - m_lastDamagePosition;
            m_lastDamagePosition = transform.position;
        }

        m_lastPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (false == applyDamageOnTriggerEnter)
        {
            return;
        }

        Colliding(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (false == applyDamageOnTriggerStay)
        {
            return;
        }

        Colliding(collider);
    }

    private void Colliding(Collider2D _col)
    {
        if (false == this.isActiveAndEnabled)
        {
            return;
        }

        if (m_ignoredGameObjects.Contains(_col.gameObject))
        {
            //Debug.Log($"[DamageOnTouch] 무시된 오브젝트: {_col.gameObject.name}");
            return;
        }

        if (false == MyLayers.LayerInLayerMask(_col.gameObject.layer, targetLayerMask))
        {
            //Debug.Log($"[DamageOnTouch] 타겟 레이어가 아님: {_col.gameObject.name}");
            return;
        }

        //Debug.Log($"[DamageOnTouch] 충돌: {_col.gameObject.name}");

        m_collideingCollider = _col;
        m_colliderHealth = _col.GetComponent<Health>();

        OnHit?.Invoke();

        if (m_colliderHealth != null && m_colliderHealth.enabled)
        {
            if (m_colliderHealth.currentHP > 0)
            {
                OnCollideWithDamageable(m_colliderHealth);
            }
        }
        else
        {
            OnCollideWithNonDamageable();
        }
        
        ApplyDamageCausedKnockback();
    }

    private void OnCollideWithDamageable(Health _health)
    {
        if (_health.invulnerable)
        {
            return;
        }

        ApplyDamageCausedKnockback();

        OnHitDamageable?.Invoke();

        m_colliderHealth.Damage(damage, this.gameObject, invincibilityDuration, m_damageDirection);

        if (m_colliderHealth.currentHP <= 0)
        {
            OnKill?.Invoke();
        }

        SelfDamage(damageTakenEveryTime + damageTakenDamageable);
    }

    private void OnCollideWithNonDamageable()
    {
        OnHitNonDamageable?.Invoke();
        
        if (damageTakenEveryTime + damageTakenNonDamageable > 0)
        {
            //Debug.Log("[DamageOnTouch] OnCollideWithNonDamageable값: " + (damageTakenEveryTime + damageTakenNonDamageable));
            SelfDamage(damageTakenEveryTime + damageTakenNonDamageable);
        }
    }

    private void ApplyDamageCausedKnockback()
    {
        //Debug.Log("넉백");

        if (damageCausedKnockbackForce == Vector2.zero)
        {
            return;
        }

        m_knockbackForce.x = damageCausedKnockbackForce.x;

        if (owner == null)
        {
            owner = this.gameObject;
        }


        if (m_collideingCollider.TryGetComponent<PlayerMovement>(out var p))
        {
            Vector2 relativePosition = p.transform.position - owner.transform.position;
            m_knockbackForce.x *= Mathf.Sign(relativePosition.x);
            m_knockbackForce.y = damageCausedKnockbackForce.y;
            p.ApplyKnockback(m_knockbackForce);
        }
        else if (m_collideingCollider.TryGetComponent<EnemyMovementControl>(out var e))
        {
            Vector2 relativePosition = e.transform.position - owner.transform.position;
            m_knockbackForce.x *= Mathf.Sign(relativePosition.x);
            m_knockbackForce.y = damageCausedKnockbackForce.y;
            e.ApplyKnockback(m_knockbackForce);
        }

        m_startTime = invincibilityDuration;
        m_doKnockback = true;
    }

    private void SelfDamage(float _damage)
    {
        if (m_health != null)
        {
            //Debug.Log("[DamageOnTouch] 자해 데미지: " + _damage);
            m_damageDirection = Vector2.up;
            m_health.Damage(_damage, this.gameObject, invincibilityDuration, m_damageDirection);
        }
    }
}
