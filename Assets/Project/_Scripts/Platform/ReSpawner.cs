using UnityEngine;

public class ReSpawner : MonoBehaviour
{
    public Transform respawnPosition;

    private GameObject m_Object;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_Object = collision.gameObject;
            collision.enabled = false;

            Invoke(nameof(OnReSpawn), 2f);
        }
    }

    private void OnReSpawn()
    {
        m_Object.transform.position = respawnPosition.position;

        Collider2D col = m_Object.GetComponent<Collider2D>();
        col.enabled = true;
    }
}
