using System.Collections;
using UnityEngine;

public class TriggerTrap : MonoBehaviour
{
    [Tooltip("함정 작동 전 대기")]
    public float preActiveDelay;
    [Tooltip("함정 활성화 시간")]
    public float effectLifeTime;
    [Tooltip("재사용 대기 시간")]
    public float reactivationCooldown;

    private BoxCollider2D m_boxCollider2D;
    private DamageOnTouch m_damageOnTouch;
    private SpriteRenderer m_spriteRenderer;
    private Color m_initialColor;

    private bool isActive = false;


    private void Awake()
    {
        m_boxCollider2D = GetComponent<BoxCollider2D>();
        m_damageOnTouch = GetComponent<DamageOnTouch>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        m_damageOnTouch.enabled = false;

        m_initialColor = m_spriteRenderer.color;
    }

    private IEnumerator TrapActivate()
    {
        if (isActive)
        {
            yield break;
        }

        isActive = true;
        Debug.Log("대기");
        m_spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(preActiveDelay);
        Debug.Log("활성화");
        m_damageOnTouch.enabled = true;
        m_spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(effectLifeTime);
        Debug.Log("종료");
        m_damageOnTouch.enabled = false;
        m_spriteRenderer.color = Color.white;

        Invoke(nameof(ResetCooldown), reactivationCooldown);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DetectPlayer(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DetectPlayer(collision);
    }

    private void DetectPlayer(Collider2D _collision)
    {
        if (_collision.CompareTag("Player") && false == isActive)
        {
            StartCoroutine(TrapActivate());
        }
    }

    private void ResetCooldown()
    {
        isActive = false;
        m_spriteRenderer.color = m_initialColor;
    }
}
