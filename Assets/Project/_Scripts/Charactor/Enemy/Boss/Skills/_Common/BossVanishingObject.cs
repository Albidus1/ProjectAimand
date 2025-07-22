using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BossVanishingObject : BossSkillBase
{
    [Header("Fade 설정")]
    [Range(0f, 1f)] public float startFade = 1f;
    [Range(0f, 1f)] public float endFade = 0f;
    public Ease fadeEase = Ease.Linear;

    private SpriteRenderer m_spriteRenderer;



    protected override void OnEnable()
    {
        base.OnEnable();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void UseSkill()
    {
        StartCoroutine(Vanishing());
    }

    private IEnumerator Vanishing()
    {
        AlphaChange(startFade);

        yield return
            m_spriteRenderer.DOFade(endFade, base.disableTime)
            .SetEase(fadeEase)
            .OnComplete(() => Destroy(gameObject));
    }

    private void AlphaChange(float _a)
    {
        Color c = m_spriteRenderer.color;
        c.a = Mathf.Clamp(_a, 0f, 1f);
        m_spriteRenderer.color = c;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerStay2D(collision);
    }
}
