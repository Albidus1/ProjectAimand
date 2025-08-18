using UnityEngine;



[RequireComponent(typeof(CircleCollider2D))]
public class Explosion : MyPoolableObject
{
    public float explosionRadius = 2f;
    [MyReadOnly]
    public bool animationCompleted = false;

    protected CircleCollider2D m_collider2D;
    protected Animator m_animator;



    private void Awake()
    {
        m_collider2D = GetComponent<CircleCollider2D>();
        m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        animationCompleted = false;

        transform.localScale = explosionRadius * Vector2.one;
        m_collider2D.radius = 2f;
    }

    protected override void OnEnable()
    {
        m_collider2D.enabled = true;

        if (m_animator != null)
        {
            animationCompleted = false;
            m_animator.Play("Explosion");
        }
    }

    protected override void OnDisable()
    {
        m_collider2D.enabled = false;
    }

    // 애니메이션 이벤트
    public void OnAnimationEnd()
    {
        if (m_animator != null)
        {
            m_animator.StopPlayback();
            animationCompleted = true;
        }

        this.gameObject.SetActive(false);
    }
}
