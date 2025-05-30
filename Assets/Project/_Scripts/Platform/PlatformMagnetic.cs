using DG.Tweening;
using UnityEditor.Rendering;
using UnityEngine;

public class PlatformMagnetic : MonoBehaviour
{
    public enum PoleType { NPole, SPole }
    [Header("극성")]
    public PoleType Pole;
    public bool isActive { get; set; }

    //[Header("중력")]
    private float gravityScale = 1;
    private float gravityStrength = 0;

    [Header("힘")]
    public float minKillSpeed = 5f;
    [Range(0f, 2f)]
    public float forceLimit = 1f;
    //public float acceleration = 1.5f;
    //public float decceleration = 3f;


    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private Vector2 m_magnetForce;
    private float m_currentSpeed;
    private float m_activateTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Pole == PoleType.NPole ? Color.red : Color.blue; // 🔴 N극 = 빨강, 🔵 S극 = 파랑
    }

    private void Update()
    {
        m_activateTime -= Time.deltaTime;

        if (m_activateTime < 0 && isActive)
        {
            isActive = false;
            m_magnetForce = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        Gravity();
    }

    private void FixedUpdate()
    {
        m_currentSpeed = Mathf.Abs(rb.linearVelocityX);

        if (isActive)
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
                health.currentHP -= 10;
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

        if (rb.linearVelocity.y < 0)
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
        rb.AddForce(_force, ForceMode2D.Force);

        //float targetSpeed = Mathf.Lerp(rb.linearVelocity.x, _force.magnitude, 1);
        //float movement = targetSpeed - rb.linearVelocity.x;

        //rb.AddForce(movement * _force.normalized);

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
        ////transform.Translate(new Vector2((rb.linearVelocity.x + movement) * Time.deltaTime, 0));
    }

    public void MagneticActivate(bool _isActive, Vector3 _direction, float _pullForce, bool _isSamePole)
    {
        isActive = _isActive;

        if (false == _isActive)
        {
            m_magnetForce = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        m_activateTime = 0.1f;

        float distance = _direction.magnitude;

        m_magnetForce = _direction * _pullForce * forceLimit;

        if (_isSamePole)
        {
            Debug.DrawRay(transform.position, _pullForce * Vector2.up, Color.cyan);
        }
        else
        {
            Debug.DrawRay(transform.position, _pullForce * Vector2.down, Color.magenta);
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
        //gravityScale = 1;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Pole == PoleType.NPole ? Color.red : Color.blue; // 🔴 N극 = 빨강, 🔵 S극 = 파랑
    }
}
