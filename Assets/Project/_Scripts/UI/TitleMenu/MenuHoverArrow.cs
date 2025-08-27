using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuHoverArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI buttonText;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public float arrowOffset = 50f;


    private void Awake()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (leftArrow == null)
        {
            leftArrow = transform.Find("LeftSelectArrow").GetComponent<RectTransform>();
        }
        if (rightArrow == null)
        {
            rightArrow = transform.Find("RightSelectArrow").GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (leftArrow == null || rightArrow == null)
        {
            return;
        }

        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (leftArrow == null || rightArrow == null)
        {
            return;
        }

        if (buttonText != null)
        {
            float textWidth = buttonText.preferredWidth;

            Vector2 textPos = buttonText.rectTransform.localPosition;

            if (leftArrow != null)
            {
                leftArrow.localPosition = new Vector3(textPos.x - (textWidth * 0.5f) - arrowOffset, textPos.y, 0f);
                leftArrow.gameObject.SetActive(true);
            }

            if (rightArrow != null)
            {
                rightArrow.localPosition = new Vector3(textPos.x + (textWidth * 0.5f) + arrowOffset, textPos.y, 0f);
                rightArrow.gameObject.SetActive(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (leftArrow == null || rightArrow == null)
        {
            return;
        }

        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);
    }
}
