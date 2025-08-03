using Unity.AppUI.Core;
using UnityEngine;

public class JumpPadBox : MonoBehaviour
{
    [Header("점프 설정")]
    [Range(1f, 3f)] public float jumpMultiplier;
    public float cooldownTime = 0.3f;

    private bool isReady = true;
    private float m_detectionOffset = 0.2f;
    private Vector2 m_lastPlayerPosition;

    private PlayerMovement m_player;
    private BoxCollider2D m_collider;



    private void Awake()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_collider.offset = new Vector2(m_collider.offset.x, (m_collider.size.y + m_detectionOffset) * 0.5f);
        m_collider.size = new Vector2(m_collider.size.x, m_detectionOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (false == isReady)
            return;

        if (false == collision.CompareTag("Player"))
            return;

        if (m_player == null)
        {
            m_player = collision.GetComponent<PlayerMovement>();
        }

        if (m_player.movementState.currentState != PlayerStates.MovementStates.Falling)
        {
            return;
        }
        
        m_player.isOnJumpPad = true;
        m_player.padDirection = Vector2.up;
        m_player.padForce = jumpMultiplier;

        isReady = false;
        Invoke(nameof(ResetCooldown), cooldownTime);
    }

    private void ResetCooldown() => isReady = true;
}
