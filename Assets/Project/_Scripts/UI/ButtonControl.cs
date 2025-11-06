using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonControl : MonoBehaviour
{
    public Button firstButton;

    private void OnEnable()
    {
        if (firstButton != null)
        {
            firstButton.Select();
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    private void OnDisable()
    {
        if (GUIManager.HasInstance)
        {
            GUIManager.Instance.resumeButton.Select();
            EventSystem.current.SetSelectedGameObject(GUIManager.Instance.resumeButton.gameObject);
        }
    }
}
