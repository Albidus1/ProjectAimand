using UnityEngine;
using UnityEngine.UI;

public class RangedAttackChargeSlider : MonoBehaviour
{
    public GameObject sliderPrefab;         // ChargingSlider 프리팹
    public float chargeThreshold = 2f;      // 최대 충전 시간
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 머리 위 위치

    private GameObject currentSlider;       // 현재 생성된 슬라이더
    private Slider slider;                  // 슬라이더 컴포넌트
    private float chargeTime = 0f;
    private bool isCharging = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            chargeTime += Time.deltaTime;

            if (!isCharging)
            {
                // 슬라이더 생성
                currentSlider = Instantiate(sliderPrefab, transform.position + offset, Quaternion.identity);
                currentSlider.transform.SetParent(null); // 월드 공간에 그대로 두기

                slider = currentSlider.GetComponent<Slider>();
                slider.maxValue = chargeThreshold;
                slider.value = 0f;

                isCharging = true;
            }

            if (slider != null)
            {
                slider.value = Mathf.Clamp(chargeTime, 0f, chargeThreshold);
                currentSlider.transform.position = transform.position + offset;
            }
        }
        else
        {
            chargeTime = 0f;
            isCharging = false;

            if (currentSlider != null)
                Destroy(currentSlider);
        }
    }
}
