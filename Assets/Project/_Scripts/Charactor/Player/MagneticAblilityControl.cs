using DG.Tweening;
using UnityEngine;

public class MagneticAblilityControl : ConeOfVision2D
{
    [Header("플레이어")]
    public PlayerMovement playerController;
    public SpriteRenderer spriteRenderer;

    [Header("이동")]
    public Ease moveEase;

    [MyReadOnly]
    public Vector3 abilityDirection
    {
        get
        {
            return base.direction;
        }
        set
        {
            base.direction = value;
            base.angleOffset = (base.direction == Vector3.right) ? 0 : 180;
            base.SetDirectionAndAngles(base.direction, transform.eulerAngles);
        }
    }


    protected bool isScanning = false;
    protected bool m_previousFacingDirection = true;
    protected float m_halfXSize;
    protected float m_halfYSize;
    protected Vector2 m_playerFrontPosition;
    protected float lastPressedAbilityInputTime;


    protected override void Awake()
    {
        base.Awake();
        OnOff(isScanning);

        playerController = GetComponent<PlayerMovement>();


    }

    protected virtual void OnOff(bool _trigger)
    {
        isScanning = _trigger;

        base.shouldDrawMesh = _trigger;
        base.visionMeshFilter.gameObject.SetActive(_trigger);
    }

    protected virtual void UpdateDirection()
    {
        m_halfXSize = playerController.col.bounds.size.x * 0.5f;
        m_halfYSize = playerController.col.bounds.size.y * 0.5f;

        if (transform.localScale.x > 0)
        {
            abilityDirection = Vector3.right;
            m_playerFrontPosition = new Vector2(transform.position.x + m_halfXSize, transform.position.y);
        }
        else
        {
            abilityDirection = Vector3.left;
            m_playerFrontPosition = new Vector2(transform.position.x - m_halfXSize, transform.position.y);
        }
    }
}
