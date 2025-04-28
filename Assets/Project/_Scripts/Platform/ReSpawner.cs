using UnityEngine;

public class ReSpawner : MonoBehaviour
{
    public Transform respawnPosition;

    private GameObject m_owner;
    private Collider2D m_collider;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnDestroyObject(collision);

            Invoke(nameof(OnReSpawn), 2f);
        }
    }

    protected virtual void OnDestroyObject(Collider2D _col)
    {
        m_owner = _col.gameObject;
        m_collider = _col;
        m_collider.enabled = false;
    }

    public virtual void OnReSpawn()
    {
        m_owner.transform.position = respawnPosition.position;

        m_collider.enabled = true;

        Health health = m_owner.GetComponent<Health>();

        if (health != null)
        {
            health.InitializeCurrentHealth();
        }
    }
}
