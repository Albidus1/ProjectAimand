using UnityEngine;
using UnityEngine.EventSystems;



public class GUIManager : MySingleton<GUIManager>
{
    [Header("바인딩")]
    public GameObject HUD;
    public MyProgressBar healthBar;
    public GameObject pauseScreen;

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

        healthBar.UpdateBar(_currentHealth, _minHealth, _maxHealth);
    }
}
