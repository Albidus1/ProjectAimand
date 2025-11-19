using UnityEngine;



public class FinishLevel : MonoBehaviour
{
    public string levelName;
   
    public bool completeLevel;

    protected PlayerMovement m_player;
    protected Collider2D m_collider2D;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GoToLevel();
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
        {
            GoToLevel();
        }
    }

    public void GoToLevel()
    {
        if (LevelManager.HasInstance)
        {
            if (completeLevel)
            {
                LevelManager.Instance.LevelComplete();
            }
            else
            {
                if (levelName == "Title")
                {
                    GameManager.Instance.ResetStatus();
                }

                LevelManager.Instance.GoToLevel(levelName);
            }
        }
    }
}
