using TMPro;
using UnityEngine;

public class RankingManager : MySingleton<RankingManager>
{
    public string titleName = "Title";

    public TextMeshProUGUI playTime;
    public TextMeshProUGUI deathCount;


    private string m_recordedPlaytime;
    private string m_recordedDeathCount;



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
        if (GameManager.HasInstance)
        {
            m_recordedPlaytime = GameManager.Instance.GetPlayTime();
            m_recordedDeathCount = GameManager.Instance.deathCount.ToString();
            Debug.Log(m_recordedPlaytime + " " + m_recordedDeathCount);

            if (playTime != null)
            {
                playTime.text = m_recordedPlaytime;
                MySaveLoadManager.Save(GameManager.Instance.GetPlayTime(), "PlayTime");
            }
            if (deathCount != null)
            {
                deathCount.text = m_recordedDeathCount;
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadLevel()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.ResetStatus();
        }

        if (!string.IsNullOrEmpty(titleName))
        {
            SceneLoadingManager.LoadScene(titleName);
        }
    }
}
