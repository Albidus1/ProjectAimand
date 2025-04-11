using UnityEngine;

public class PlatformMagnetic : MonoBehaviour
{
    public enum PoleType { NPole, SPole }
    [Header("극성")]
    public PoleType Pole;
    public bool isActive { get; set; }

    [Header("중력")]
    public float gravityScale = 1;
    private float gravityStrength = 0;

    [Header("이동")]
    public float maxSpeed = 5f;
    public float acceleration = 1.5f;
    public float decceleration = 3f;

    private PlayerMovement playerData;
    [HideInInspector] public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerData = FindAnyObjectByType<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Pole == PoleType.NPole ? Color.red : Color.blue; // 🔴 N극 = 빨강, 🔵 S극 = 파랑
    }

    private void Update()
    {
        Gravity();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.x != 0 && false == isActive)
        {
            Move(Vector2.zero);
        }
    }

    public void Gravity()
    {
        gravityStrength = gravityScale * playerData.data.gravityScale;

        if (rb.linearVelocity.y < 0)
        {
            SetGravityScale(gravityStrength * playerData.data.fallGravityMult);

            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -playerData.data.maxFallSpeed));
        }
        else
        {
            SetGravityScale(gravityStrength);
        }
    }

    public void Move(Vector2 _force)
    {
        float targetSpeed = _force.x * maxSpeed;

        targetSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, 1);

        float accelerate = SetAccelerate(targetSpeed);

        

        if (Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) &&
            Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelerate = 0;
        }

        float speedDif = targetSpeed - rb.linearVelocity.x;
        float movement = speedDif * accelerate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    #region GENERAL METHODS
    public void SetGravityScale(float _scale)
    {
        rb.gravityScale = _scale;
    }

    public float SetAccelerate(float _speed)
    {
        float runAccelAmount = (50 * acceleration) / maxSpeed;
        float runDeccelAmount = (50 * acceleration) / maxSpeed;

        float acc = (Mathf.Abs(_speed) > 0.01f) ? runAccelAmount : runDeccelAmount;

        return acc;
    }
    #endregion

    private void OnValidate()
    {
        //gravityScale = 1;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Pole == PoleType.NPole ? Color.red : Color.blue; // 🔴 N극 = 빨강, 🔵 S극 = 파랑
    }
}
