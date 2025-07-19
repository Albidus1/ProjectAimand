using UnityEngine;
using UnityEngine.UI;

// MVP 패턴으로 작성
public class ChargeBarView : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    public void UpdateChargeBar(float value)
    {
        slider.value = value;
    }

    public void EnableChageBar()
    {
        slider.gameObject.SetActive(true);
    }

    public void DisableChageBar()
    {
        slider.gameObject.SetActive(false);
    }
}
