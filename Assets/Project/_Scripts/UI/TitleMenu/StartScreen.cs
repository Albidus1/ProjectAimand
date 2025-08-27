using UnityEngine;



public class StartScreen : MonoBehaviour
{
    public string nextLevel;
    public string loadingSceneName = "LoadingScreen";


    private void Awake()
    {
        GUIManager.Instance.SetHUDActive(false);
    }

    private void Start()
    {

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
            SceneLoadingManager.LoadScene(nextLevel, loadingSceneName);
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
