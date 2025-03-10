using UnityEngine;

public class PlatformCollider : MonoBehaviour
{
    public Transform playerFeet;
    public Collider2D col;
    public float top;


    private void Awake()
    {
        playerFeet = GameObject.Find("ground_check").GetComponent<Transform>();
        col = GetComponent<Collider2D>();
        top = col.bounds.max.y;
    }

    private void Update()
    {
        if (playerFeet.position.y >= transform.position.y + top)
        {
            col.enabled = true;
        }
        else
        {
            col.enabled = false;
        }
    }

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
        Vector2 size = transform.localScale;
        top = col.bounds.max.y;

        if (playerFeet.position.y >= transform.position.y + top)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, size);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
    #endregion
}
