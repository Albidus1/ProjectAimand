using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class MagneticPull : MagneticAblilityControl
{
    [Header("자력 능력")]
    public float pullForce;


    private void Update()
    {
        base.lastPressedAbilityInputTime -= Time.deltaTime;

        #region INPUT HANDLER
        if (Input.GetKey(KeyCode.S))
        {
            lastPressedAbilityInputTime = 0.1f;

            if (false == isScanning)
            {
                OnOff(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            OnOff(false);
        }
        #endregion

        if (lastPressedAbilityInputTime > 0)
        {
            ScanForTargets();
            PullMagnet();
        }
        if (lastPressedAbilityInputTime < 0 && base.visibleTargets.Count > 0)
        {
            base.visibleTargets.Clear();
        }

        if (base.m_previousFacingDirection != playerController.isFacingRight)
        {
            //Debug.Log("방향 전환");
            base.UpdateDirection();
            base.m_previousFacingDirection = playerController.isFacingRight;
        }
    }

    private void PullMagnet()
    {
        foreach (Transform col in base.visibleTargets)
        {
            PlatformMagnetic pole = col.GetComponent<PlatformMagnetic>();

            if (pole == null)
                continue;

            float distance = Vector2.Distance(col.transform.position, transform.position);

            if (distance > base.visionRadius || false == isScanning)
            {
                pole.MagneticActivate(false, Vector3.zero, 0, true);
                continue;
            }

            Vector2 direction = (base.m_playerFrontPosition - (Vector2)col.transform.position).normalized;

            float t = 1f - Mathf.Clamp01(distance / base.visionRadius);
            float rawForce = DOVirtual.EasedValue(0, pullForce, t, moveEase);
            float easedForce = Mathf.Max(rawForce, 1);

            //Debug.DrawRay(transform.position, easedForce * Vector2.up, Color.red);

            pole.MagneticActivate(true, direction, easedForce, true);
        }
    }
}
