using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class StartScreen : MonoBehaviour
{
    public Button startButton; 
    public string nextLevel;
    public string loadingSceneName = "LoadingScreen";


    private void Awake()
    {
        GUIManager.Instance.SetHUDActive(false);
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (startButton != null )
        {
            startButton.Select();
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    private void Update()
    {
        //ButtonPressed();
    }

    public void ButtonPressed()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (!string.IsNullOrEmpty(nextLevel))
        {
            SceneLoadingManager.LoadScene(nextLevel);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }
#else
        Application.Quit();
#endif
    }
}
