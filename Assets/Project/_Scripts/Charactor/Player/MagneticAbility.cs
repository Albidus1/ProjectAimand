using UnityEngine;

public class MagneticAbility : ConeOfVision2D
{
    [Header("플레이어")]
    [SerializeField] private PlayerMovement playerController;
    private bool previousFacingDirection = true;

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
            Debug.Log(base.direction);
        }
    }
    


    protected override void Awake()
    {
        base.Awake();
        playerController = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (previousFacingDirection != playerController.isFacingRight)
        {
            Debug.Log("방향 전환");
            UpdateDirection();
            previousFacingDirection = playerController.isFacingRight;
        }
    }

    public void UpdateDirection()
    {
        if (transform.localScale.x > 0)
        {
            abilityDirection = Vector3.right;
        }
        else
        {
            abilityDirection = Vector3.left;
        }
    }
}
