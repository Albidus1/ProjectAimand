using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class GUIManager : MySingleton<GUIManager>
{
    [Header("바인딩")]
    public GameObject HUD;
    public MyProgressBar healthBar;
    public HealthCellUI healthCell;
    public GameObject pauseScreen;

    [Header("버튼")]
    public Button resumeButton;
    public Button restartButton;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeStatics()
    {
        m_instance = null;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(GameManager.Instance.UnPause);
        }
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(GameManager.Instance.UnPause);
            restartButton.onClick.AddListener(LevelManager.Instance.KillPlayer);
        }
    }

    private void OnDisable()
    {
        if (resumeButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
        }
    }

    public void SetHUDActive(bool _flag)
    {
        if (HUD != null)
        {
            HUD.SetActive(_flag);
        }
    }

    public void SetPause(bool _flag)
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(_flag);
            EventSystem.current.sendNavigationEvents = _flag;
            Time.timeScale = _flag ? 0f : 1f;
        }
    }

    public void UpdateHealthBar(float _currentHealth, float _minHealth, float _maxHealth)
    {
        if (healthBar == null)
        {
            return;
        }

        if (false == healthBar.gameObject.activeInHierarchy)
        {
            return;
        }

        healthBar.UpdateBar(_currentHealth, _minHealth, _maxHealth);
    }

    public void UpdateHealthCell(int _currentHealth, int _maxHealth)
    {
        if (healthCell == null)
        {
            return;
        }

        if (false == healthCell.gameObject.activeInHierarchy)
        {
            return;
        }

        healthCell.UpdateHealthUI(_currentHealth, _maxHealth);
    }
}
