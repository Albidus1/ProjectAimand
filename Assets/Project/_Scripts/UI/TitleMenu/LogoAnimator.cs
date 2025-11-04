using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;



public class LogoAnimator : MonoBehaviour
{
    public float duration = 1f;
    public Ease ease = Ease.OutQuad;

    private Material m_logoMaterial;
    private const string m_splitAmount = "_SplitAmount";



    private void Awake()
    {
        m_logoMaterial = GetComponent<Image>().material;
    }

    private void Start()
    {
        m_logoMaterial.SetFloat(m_splitAmount, 0);
        DOTween.To(
            () => m_logoMaterial.GetFloat(m_splitAmount),
            x => m_logoMaterial.SetFloat(m_splitAmount, x),
            1f, duration)
            .SetEase(ease)
            .SetDelay(0.1f);
    }
}
