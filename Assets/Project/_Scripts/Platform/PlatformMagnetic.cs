using UnityEngine;

public class PlatformMagnetic : MonoBehaviour
{
    public enum BoxSize
    {
        m1x1, m2x1, m3x1,
        m1x2, m2x2, m3x2,
        m1x3, m2x3, m3x3
    }

    [Header("자력블록")]
    public BoxSize boxSize = BoxSize.m2x2;
    public bool isActive { get; set; }
    public bool isPulling { get; private set; }
    public bool isPushing { get; private set; }

    //[Header("중력")]
    private float gravityScale = 1;
    private float gravityStrength = 8;

    [Header("힘")]
    public float minKillSpeed = 5f;
    [Range(0f, 2f)]
    public float forceLimit = 1f;
    //public float acceleration = 1.5f;
    //public float decceleration = 3f;


    public Rigidbody2D rb { get; private set; }
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private Vector2 m_magnetForce;
    private float m_currentSpeed;
    private float m_activateTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {     
        m_activateTimer -= Time.deltaTime;
        if (isActive && m_activateTimer < 0)
        {
            m_magnetForce = Vector2.zero;

            isActive = false;
        }

        Gravity();
    }

    private void FixedUpdate()
    {
        m_currentSpeed = Mathf.Abs(rb.linearVelocityX);

        if (isActive && isPushing)
        {
            Vector2 direction = m_magnetForce.normalized;
            float force = m_magnetForce.magnitude;

            rb.AddForce(direction * force, ForceMode2D.Impulse);
            isPushing = false;
        }
        else if (isActive && isPulling)
        {
            Move(m_magnetForce);
        }
        else
        {
            Move(Vector2.zero);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Health health = collision.gameObject.GetComponent<Health>();

            //Debug.Log($"속도1 : {Mathf.Abs(rb.linearVelocity.x)}");
            //Debug.Log($"속도2 : {m_currentSpeed}");

            if (m_currentSpeed > minKillSpeed)
            {
                health.Kill();
            }
        }

        //if (collision.collider.CompareTag("Player"))
        //{
        //    rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        //}
    }

    public void Gravity()
    {
        gravityStrength = gravityScale * 8f;

        if (isActive)
        {
            SetGravityScale(0);
        }
        else if (rb.linearVelocity.y < 0)
        {
            SetGravityScale(gravityStrength * 1.5f);

            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -20));
        }
        else
        {
            SetGravityScale(gravityStrength);
        }
    }

    public void Move(Vector2 _force)
    {
        float targetSpeed = Mathf.Lerp(rb.linearVelocity.x, _force.magnitude, 1);
        float movement = targetSpeed - rb.linearVelocity.x;

        rb.AddForce(movement * _force.normalized);

        //transform.Translate(_force * Time.deltaTime);

        //float f = _force.x > 0 ? _force.magnitude : -_force.magnitude;
        //float targetSpeed = f;

        //targetSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, 1);

        //float accelerate = SetAccelerate(targetSpeed);

        //if (Mathf.Abs(rb.linearVelocity.x) > Mathf.Abs(targetSpeed) &&
        //    Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) &&
        //    Mathf.Abs(targetSpeed) > 0.01f && isActive)
        //{
        //    accelerate = 0;
        //}

        //float speedDif = targetSpeed - rb.linearVelocity.x;
        //float movement = speedDif * accelerate;
        ////float movement = speedDif * accelerate * Time.fixedDeltaTime;

        //rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    public void MagneticActivate(bool _isActive, Vector3 _direction, float _pullForce = 0f, bool _isPulling = false)
    {
        isActive = _isActive;
        isPulling = _isPulling;
        isPushing = !_isPulling;

        if (false == _isActive)
        {
            return;
        }

        m_activateTimer = 0.1f;
        m_magnetForce = _pullForce * forceLimit * _direction;

        if (isPushing)
        {
            Debug.DrawRay(transform.position, _direction, Color.cyan);
        }
        else
        {
            Debug.DrawRay(transform.position, _direction, Color.magenta);
        }
    }

    #region GENERAL METHODS
    public void SetGravityScale(float _scale)
    {
        rb.gravityScale = _scale;
    }

    //public float SetAccelerate(float _speed)
    //{
    //    float runAccelAmount = (50 * acceleration) / maxSpeed;
    //    float runDeccelAmount = (50 * decceleration) / maxSpeed;

    //    float acc = (Mathf.Abs(_speed) > 0.01f) ? runAccelAmount : runDeccelAmount;

    //    return acc;
    //}
    #endregion

    private void OnValidate()
    {

    }
}
