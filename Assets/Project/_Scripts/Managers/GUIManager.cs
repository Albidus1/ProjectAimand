using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class GUIManager : MySingleton<GUIManager>
{
    public bool locked { get; set; } = false;

    [Header("바인딩")]
    public GameObject HUD;
    public MyProgressBar healthBar;
    public HealthCellUI healthCell;
    public GameObject pauseScreen;
    public GameObject settingPopup;
    public GameObject quitPopUp;

    [Header("버튼")]
    public Button resumeButton;
    public Button restartButton;
    public Button settingButton;
    public Button quitButton;

    public Image buttonImage;
    public Image selectImage;


    public bool enableQuitPopUp
    {
        get => m_quitPopUpEnabled;

        set
        {
            if (m_quitPopUpEnabled == value)
            {
                return;
            }

            if (value)
            {
                EnableQuitPopUp();
            }
            else
            {
                DisableQuitPopUp();
            }

            m_quitPopUpEnabled = value;
        }
    }

    public bool enableSettingPopup
    {
        get => m_settingPopupEnabled;

        set
        {
            if (m_settingPopupEnabled == value)
            {
                return;
            }

            if (value)
            {
                EnableSettingPopUp();
            }
            else
            {
                DisableSettingPopUp();
            }

            m_settingPopupEnabled = value;
        }
    }

    public bool isPaused { get; private set; }

    private List<Button> m_buttons = new List<Button>();
    private bool m_settingPopupEnabled;
    private bool m_quitPopUpEnabled;



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

    }

    private void Update()
    {
        if (locked)
        {
            return;
        }

        if (InputManager.HasInstance && InputManager.Instance.CancleButton.IsDown)
        {
            if (GameManager.HasInstance)
            {
                MainEvent.Trigger(MainEventTypes.TogglePause);

                resumeButton.Select();
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }

            if (enableQuitPopUp || enableSettingPopup)
            {
                enableQuitPopUp = false;
                enableSettingPopup = false;
            }
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

    public void EnableSettingPopUp()
    {
        m_settingPopupEnabled = true;
        settingPopup.SetActive(true);

        if (quitPopUp.activeSelf)
        {
            m_quitPopUpEnabled = false;
            quitPopUp.SetActive(false);
        }

        for (int i = 0; i < m_buttons.Count; i++)
        {
            m_buttons[i].enabled = false;
        }
    }

    public void DisableSettingPopUp()
    {
        m_settingPopupEnabled = false;
        settingPopup.SetActive(false);

        for (int i = 0; i < m_buttons.Count; i++)
        {
            m_buttons[i].enabled = true;
        }
    }

    public void EnableQuitPopUp()
    {
        m_quitPopUpEnabled = true;
        quitPopUp.SetActive(true);

        if (settingPopup.activeSelf)
        {
            m_settingPopupEnabled = false;
            settingPopup.SetActive(false);
        }

        for (int i = 0; i < m_buttons.Count; i++)
        {
            m_buttons[i].enabled = false;
        }
    }

    public void DisableQuitPopUp()
    {
        m_quitPopUpEnabled = false;
        quitPopUp.SetActive(false);

        for (int i = 0; i < m_buttons.Count; i++)
        {
            m_buttons[i].enabled = true;
        }
    }

    private void OnEnable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(GameManager.Instance.UnPause);
            m_buttons.Add(resumeButton);
        }
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(GameManager.Instance.UnPause);
            restartButton.onClick.AddListener(LevelManager.Instance.KillPlayer);
            m_buttons.Add(restartButton);
        }
        if (settingButton != null)
        {
            m_buttons.Add(settingButton);
        }
        if (quitButton != null)
        {
            m_buttons.Add(quitButton);
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
}
