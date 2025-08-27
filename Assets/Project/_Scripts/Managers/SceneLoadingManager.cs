using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class SceneLoadingManager : MonoBehaviour
{
    public enum LoadingStatus
    {
        LoadStart,
        LoadComplete
    }

    public struct LoadingSceneEvent
    {
        static LoadingSceneEvent e;

        public string sceneName;
        public LoadingStatus status;

        public LoadingSceneEvent(string _sceneName, LoadingStatus _status)
        {
            sceneName = _sceneName;
            status = _status;
        }

        public static void Trigger(string _sceneName, LoadingStatus _status)
        {
            e.sceneName = _sceneName;
            e.status = _status;
            EventManager.TriggerEvent(e);
        }
    }



    public static string loadingScreenSceneName = "LoadingScreen";

    public TextMeshProUGUI loadingText;

    public float startFadeDuration = 0.2f;
    public float exitFadeDuration = 0.2f;
    public float loadCompleteDelay = 0.5f;

    private AsyncOperation m_asyncOperation;
    private static string m_sceneToLoad = "";
    private float m_fadeDuration = 0.5f;
    private string m_loadingTextValue;
    private static Tween m_tween;



    public static void LoadScene(string _sceneToLoad)
    {
        m_sceneToLoad = _sceneToLoad;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        if (loadingScreenSceneName != null)
        {
            LoadingSceneEvent.Trigger(_sceneToLoad, LoadingStatus.LoadStart);
            SceneManager.LoadScene(loadingScreenSceneName);
        }
    }

    public static void LoadScene(string _sceneToLoad, string _loadingSceneName)
    {
        m_sceneToLoad = _sceneToLoad;
        Application.backgroundLoadingPriority = ThreadPriority.High;
        SceneManager.LoadScene(_loadingSceneName);
    }

    private void Start()
    {
        Debug.Log($"확인용 : {m_sceneToLoad}");

        m_tween = m_tween.SetEase(Ease.OutCubic);
        m_loadingTextValue = loadingText.text;

        if (false == string.IsNullOrEmpty(m_sceneToLoad))
        {
            Debug.Log("로딩중");
            StartCoroutine(LoadAsynchronously());
        }
    }

    private void Update()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator LoadAsynchronously()
    {
        LoadingSetup();

        yield return new WaitForSeconds(startFadeDuration);

        m_asyncOperation = SceneManager.LoadSceneAsync(m_sceneToLoad, LoadSceneMode.Single);
        m_asyncOperation.allowSceneActivation = false;

        while (m_asyncOperation.progress < 0.9f)
        {
            Debug.Log(m_asyncOperation.progress);
            yield return null;
        }

        LoadingComplete();
        yield return new WaitForSeconds(loadCompleteDelay);

        yield return new WaitForSeconds(exitFadeDuration);

        m_asyncOperation.allowSceneActivation = true;
        LoadingSceneEvent.Trigger(m_sceneToLoad, LoadingStatus.LoadComplete);
    }

    private void LoadingSetup()
    {
        loadingText.text = m_loadingTextValue;
    }

    private void LoadingComplete()
    {

    }
}
