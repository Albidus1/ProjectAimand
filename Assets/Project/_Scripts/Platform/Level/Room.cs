using Unity.Cinemachine;
using UnityEngine;

public class Room : MonoBehaviour
{
    public CMCameraMove cm;

    private BoxCollider2D m_col;

    private void Awake()
    {
        cm = FindAnyObjectByType<CMCameraMove>();
        m_col = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("방 바뀜");
            cm.confiner.BoundingShape2D = m_col;
            cm.confiner.InvalidateBoundingShapeCache();

            cm.confiner.Damping = 3f;
            cm.confiner.SlowingDistance = 2f;

            cm.cinemachineCamera.ForceCameraPosition(cm.cinemachineCamera.State.GetFinalPosition(), cm.cinemachineCamera.State.GetFinalOrientation());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }

    private void OnDrawGizmos()
    {
        if (m_col == null)
        {
            m_col = (BoxCollider2D)GetComponent<Collider2D>();
        }

        Vector3 pos = m_col.bounds.center;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos, m_col.bounds.size);
    }
}
