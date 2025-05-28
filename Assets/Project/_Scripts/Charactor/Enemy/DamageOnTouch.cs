using System;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    public bool killPlayer = false;

    [Header("대상")]
    //public LayerMask targetLayerMask;
    public bool applyDamageOnTriggerEnter = true;
    public bool applyDamageOnTriggerStay = true;

    [Header("넉백")]
    public Vector2 damageCausedKnockbackForce = new Vector2(10, 2);

    [Header("무적 상태")]
    public float invincibilityDuration = 0.5f;

    [MyReadOnly]
    public GameObject owner;


    private Health m_health;
    private Vector2 m_lastPosition;
    private Vector2 m_lastDamagePosition;
    private Vector2 m_velocity;
    private Vector2 m_knockbackForce;
    private Vector2 m_damageDirection;
    private float m_startTime;
    private EnemyMovement m_enemyMovement;
    private PlayerMovement m_playerMovement;
    private Collider2D m_playerCollider;
    private bool m_doKnockback;


    private void Awake()
    {
        m_enemyMovement = GetComponent<EnemyMovement>();

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (false == applyDamageOnTriggerEnter)
        {
            return;
        }

        ApplyDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (false == applyDamageOnTriggerStay)
        {
            return;
        }

        ApplyDamage(other);
    }

    private void ApplyDamage(Collider2D _col)
    {
        if (false == this.isActiveAndEnabled)
        {
            return;
        }

        if (false == _col.CompareTag("Player"))
        {
            return;
        }

        if (m_doKnockback)
        {
            return;
        }

        if (m_playerMovement == null)
        {
            m_playerMovement = _col.GetComponent<PlayerMovement>();
        }

        m_playerCollider = _col;

        if (m_health == null)
        {
            m_health = _col.GetComponent<Health>();
        }

        if (false == killPlayer)
        {
            m_health.currentHP -= 10;
        }
        else
        {
            m_health.currentHP -= 99999;
        }


        ApplyDamageCausedKnockback();
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

        Vector2 relativePosition = m_playerMovement.transform.position - owner.transform.position;
        m_knockbackForce.x *= Mathf.Sign(relativePosition.x);
        m_knockbackForce.y = damageCausedKnockbackForce.y;

        m_playerMovement.Knockback(m_knockbackForce);

        m_startTime = invincibilityDuration;
        m_doKnockback = true;
    }
}
