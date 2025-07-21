using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BossVanishingObject : MonoBehaviour
{

    [Range(0f, 1f)] public float startFade = 1f;
    [Range(0f, 1f)]public float endFade = 0f;
    public Ease fadeEase = Ease.Linear;

    private SpriteRenderer m_spriteRenderer;
    private float m_vanishTime = 2f;


    private void OnEnable()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartVanishing(float _timer)
    {
        m_vanishTime = _timer;
        StartCoroutine(Vanishing());
    }

    private IEnumerator Vanishing()
    {
        AlphaChange(startFade);

        yield return
            m_spriteRenderer.DOFade(endFade, m_vanishTime)
            .SetEase(fadeEase)
            .OnComplete(() => Destroy(gameObject));
    }

    private void AlphaChange(float _a)
    {
        Color c = m_spriteRenderer.color;
        c.a = Mathf.Clamp(_a, 0f, 1f);
        m_spriteRenderer.color = c;
    }
}
