using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagneticObject : MonoBehaviour
{
    public enum MagneticFlag
    {
        NONE = 0,
        PULL = 1 << 0,
        PUSH = 1 << 1,
        HOLD = 1 << 2,
        ALL = PULL | PUSH | HOLD
    }

    [SerializeField]
    public MagneticFlag MagneticType;

    public bool IsPullable => (MagneticType & MagneticFlag.PULL) > 0;
    public bool IsPushable => (MagneticType & MagneticFlag.PUSH) > 0;
    public bool IsHoldable => (MagneticType & MagneticFlag.HOLD) > 0;
}
