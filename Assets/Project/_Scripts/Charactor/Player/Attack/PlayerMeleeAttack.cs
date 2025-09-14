using System.Collections;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public GameObject attackCollider2DGameObject;
    public float inputDelay = 0.5f;
    public float comboInputTime = 0.5f;
    public float firstAttackDelay = 0.1f;
    public float secondAttackDelay = 0.1f;
    public float firstDamage = 10f;
    public float secondDamage = 20f;

    private Collider2D m_attackCollider2D;
    private DamageOnTouch m_damageOnTouch;
    private SpriteRenderer m_spriteRenderer;
    private float m_lastAttackTime;
    private float m_attackDelayTimer;
    private float m_attackDelay;
    private float m_comboTimer;
    private bool m_isAttacking;
    private bool m_isFirstAttackDone;


    private void Awake()
    {
        if (attackCollider2DGameObject == null)
        {
            attackCollider2DGameObject = transform.Find("AttackCollider2D").gameObject;
        }

        if (attackCollider2DGameObject != null )
        {
            m_attackCollider2D = attackCollider2DGameObject.GetComponent<Collider2D>();
            m_damageOnTouch = attackCollider2DGameObject.GetComponent<DamageOnTouch>();
            m_spriteRenderer = attackCollider2DGameObject.GetComponent<SpriteRenderer>();
        }

        attackCollider2DGameObject.SetActive(false);
    }

    private void Start()
    {
        if (m_damageOnTouch != null)
        {
            m_damageOnTouch.damage = firstDamage;
            m_damageOnTouch.invincibilityDuration = 0.1f;
        }
    }

    private void Update()
    {
        if (Time.time - m_attackDelayTimer > inputDelay)
        {
            m_comboTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.A) && false == m_isAttacking)
            {
                bool doSecondAttack = m_isFirstAttackDone && m_comboTimer > 0;

                int scaleY = doSecondAttack ? -1 : 1;
                Vector3 scale = new Vector3(1, scaleY, 1);
                attackCollider2DGameObject.transform.localScale = scale;

                if (doSecondAttack)
                {
                    Debug.Log("2타");

                    m_damageOnTouch.damage = secondDamage;
                    m_attackDelay = secondAttackDelay;
                    StartCoroutine(AttackStart());
                    m_isFirstAttackDone = false;
                }
                else
                {
                    Debug.Log("1타");

                    m_damageOnTouch.damage = firstDamage;
                    m_attackDelay = firstAttackDelay;
                    StartCoroutine(AttackStart());
                    m_isFirstAttackDone = true;
                }

                m_lastAttackTime = Time.time;
            }
        }
    }

    private IEnumerator AttackStart()
    {
        Debug.Log(m_damageOnTouch.damage);

        m_isAttacking = true;

        yield return new WaitForSeconds(m_attackDelay);

        attackCollider2DGameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        attackCollider2DGameObject.SetActive(false);

        yield return new WaitForSeconds(0.05f);

        m_attackDelayTimer = Time.time;
        m_comboTimer = m_isFirstAttackDone ? comboInputTime : 0f;
        m_isAttacking = false;
    }
}
