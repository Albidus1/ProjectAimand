using UnityEngine;

public class PlatformMagnetic : MonoBehaviour
{
    public enum PoleType { NPole, SPole }
    [Header("극성")]
    public PoleType Pole;

    [Header("중력")]
    public float gravityScale = 1f;

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

    public void Gravity()
    {
        // 여기에 중력 관련 작업하기
        // 자력블럭의 중력 기준은 플레이어로
        // 1. 자력블럭 gravityScale이 1배율일 경우 => 플레이어의 기본 중력 값
        // 2. 낙하시 가속 넣기

        if (rb.linearVelocity.y < 0)
        {
            SetGravityScale(playerData.data.gravityScale * playerData.data.fallGravityMult);

            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -playerData.data.maxFallSpeed));
        }
        else
        {
            SetGravityScale(playerData.data.gravityScale);
        }
    }

    public void Move()
    {
        // MagneticAbility의 PullMagnet에서 이동 처리를 지우고 여기로 옮기기
        // 미끄러지는 현상 해결하기
    }

    #region GENERAL METHODS
    public void SetGravityScale(float _scale)
    {
        // PlayerMovement 참고하셔도 됩니다
        rb.gravityScale = _scale;
    }
    #endregion

    private void OnValidate()
    {
        //gravityScale = 1;
    }
}
