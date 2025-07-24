using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerRespawn : MonoBehaviour
{
    private Collider2D m_collider;
    private Health m_health;
    private PlayerMovement m_playerMovement;

    void Start()
    {
        m_collider = GetComponent<Collider2D>();
        m_health = GetComponent<Health>();
        m_playerMovement = GetComponent<PlayerMovement>();

        m_health.OnDeathEvent.AddListener(OnDeathEventFunc);
    }

    private IEnumerator OnRespawn()
    {
        //m_collider.enabled = false;
        m_collider.excludeLayers = ~(1 << LayerMask.NameToLayer("Platform") | 1 << LayerMask.NameToLayer("Platform_Magnet")); // 플랫폼 외 충돌 무시

        yield return new WaitForSeconds(3f);

        m_health.InitializeCurrentHealth();
        if (LevelManager.Instance.currentCheckPoint != null)
            SetTransformToCheckPoint(LevelManager.Instance.currentCheckPoint);

        //m_collider.enabled = true;
        m_collider.excludeLayers = 0; // 모든 충돌 재 활성화

        if (m_playerMovement != null)
        {
            m_playerMovement.movementState.StateChange(PlayerStates.MovementStates.Idle);
        }
    }

    private void SetTransformToCheckPoint(CheckPoint checkpoint)
    {
        this.transform.position = checkpoint.transform.position;
        GetComponent<PlayerMovement>().CheckDirectionToFace(checkpoint.isFacingRight);
    }

    private void OnDeathEventFunc()
    {
        StartCoroutine(OnRespawn());
    }
}
