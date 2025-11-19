using UnityEngine;

public class ReSpawner : MonoBehaviour
{
    private PlayerMovement m_owner;
    private Collider2D m_collider;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnDestroyObject(collision);
            OnReSpawn();
        }
    }

    protected virtual void OnDestroyObject(Collider2D _col)
    {
        m_owner = _col.GetComponent<PlayerMovement>();
        m_collider = _col;
        m_collider.enabled = false;
    }

    public virtual void OnReSpawn()
    {
        LevelManager.Instance.currentCheckPoint.SpawnPlayer(LevelManager.Instance.player);
    }
}
