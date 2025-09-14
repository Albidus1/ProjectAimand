using UnityEngine;



public class FinishLevel : MonoBehaviour
{
    public string levelName;

    protected PlayerMovement m_player;
    protected Collider2D m_collider2D;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GoToLevel();
        }
    }

    public void GoToLevel()
    {
        if (LevelManager.HasInstance)
        {
            LevelManager.Instance.GoToLevel(levelName);
        }
    }
}
