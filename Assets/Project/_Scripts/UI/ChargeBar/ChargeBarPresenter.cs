using UnityEngine;

// MVP 패턴으로 작성
public class ChargeBarPresenter : MonoBehaviour
{
    [SerializeField]
    private ChargeBarView chargeBarView;

    public void UpdateChargeBar(float value)
    {
        chargeBarView.UpdateChargeBar(value);
    }

    public void EnableChageBar()
    {
        chargeBarView.EnableChageBar();
    }

    public void DisableChageBar()
    {
        chargeBarView.DisableChageBar();
    }
}
