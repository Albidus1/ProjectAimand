using System.Collections;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using File = System.IO.File;



[CreateAssetMenu(menuName = "플레이어 데이터")]
public class PlayerData : ScriptableObject
{
    [Header("중력")]
    [HideInInspector] public float gravityStrength;
    [HideInInspector] public float gravityScale;
    [Space(5)]

    public float fallGravityMult;
    public float maxFallSpeed;
    [Space(5)]

    public float fastFallGravityMult;
    public float maxFastFallSpeed;
    [Space(20)]

    [Header("이동")]
    public float runMaxSpeed;
    public float runAcceleration;
    [HideInInspector] public float runAccelAmount;
    public float runDecceleration;
    [HideInInspector] public float runDeccelAmount;
    [Space(5)]

    [Range(0f, 1f)] public float accelInAir;
    [Range(0f, 1f)] public float deccelInAir;
    [Space(5)]

    public bool doConserveMomentum = true;
    [Space(20)]

    [Header("점프")]
    public float jumpHeight;
    public float jumpTimeToApex;
    [HideInInspector] public float jumpForce;
    [Space(5)]

    public float jumpCutGravityMult;
    [Range(0f, 1.0f)] public float jumpHangGravityMult;
    public float jumpHangTimeThreshold;
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;
    [Space(20)]

    [Header("벽 점프")]
    public Vector2 wallJumpForce;
    [Space(5)]

    [Range(0f, 1f)] public float wallJumpRunLerp;
    [Range(0f, 1.5f)] public float wallJumpTime;
    public bool doTurnOnWallJump;
    [Space(20)]

    [Header("슬라이드")]
    public float slideSpeed;
    public float slideAccel;
    [Space(20)]

    [Header("벽 붙잡기")]
    public float grabStamina;
    public float climbUpSpeed;

    [Header("대쉬")]
    public bool doDoubleInput = true;
    public float doubleInputTime = 0.3f;
    [Space(5)]

    public int dashAmount;
    public float dashSpeed;
    public float dashSleepTime;
    [Space(5)]

    public float dashAttackTime;
    [Space(5)]

    public float dashEndTime;
    public Vector2 dashEndSpeed;
    [Range(0f, 1f)] public float dashEndRunLerp;
    [Space(5)]

    public float dashRefillTime;

    [Header("어시스트")]
    [Range(0.01f, 0.5f)] public float coyoteTime;
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;
    [Range(0.01f, 0.5f)] public float dashInputBufferTime;
    [Range(0.01f, 0.5f)] public float grabInputBufferTime;


    private void OnValidate() => CalculateDerivedFields();
    private void CalculateDerivedFields()
    {
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        gravityScale = gravityStrength / Physics2D.gravity.y;

        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
    }

    //[ContextMenu("저장")]
    public void OnSave()
    {
        SavePlayerData data = new SavePlayerData()
        {
            fallGravityMult = fallGravityMult,
            maxFallSpeed = maxFallSpeed,
            fastFallGravityMult = fastFallGravityMult,
            maxFastFallSpeed = maxFastFallSpeed,
            runMaxSpeed = runMaxSpeed,
            runAcceleration = runAcceleration,
            run_decceleration = runDecceleration,
            accelInAir = accelInAir,
            deccelInAir = deccelInAir,
            doConserveMomentum = doConserveMomentum,
            jumpHeight = jumpHeight,
            jumpTimeToApex = jumpTimeToApex,
            jumpCutGravityMult = jumpCutGravityMult,
            jumpHangGravityMult = jumpHangGravityMult,
            jumpHangTimeThreshold = jumpHangTimeThreshold,
            jumpHangAccelerationMult = jumpHangAccelerationMult,
            jumpHangMaxSpeedMult = jumpHangMaxSpeedMult,
            wallJumpForce = wallJumpForce,
            wallJumpRunLerp = wallJumpRunLerp,
            wallJumpTime = wallJumpTime,
            doTurnOnWallJump = doTurnOnWallJump,
            slideSpeed = slideSpeed,
            slideAccel = slideAccel,
            grabStamina = grabStamina,
            climbUpSpeed = climbUpSpeed,
            dashAmount = dashAmount,
            dashSpeed = dashSpeed,
            dashSleepTime = dashSleepTime,
            dashAttackTime = dashAttackTime,
            dashEndTime = dashEndTime,
            dashEndSpeed = dashEndSpeed,
            dashEndRunLerp = dashEndRunLerp,
            dashRefillTime = dashRefillTime,
            coyoteTime = coyoteTime,
            jumpInputBufferTime = jumpInputBufferTime,
            dashInputBufferTime = dashInputBufferTime,
            grabInputBufferTime = grabInputBufferTime
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.dataPath, "PlayerStateData.json");
        File.WriteAllText(path, json);
    }
    //[ContextMenu("로드")]
    public void OnLoad()
    {
        string path = Path.Combine(Application.persistentDataPath, "PlayerStateData.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SavePlayerData data = JsonUtility.FromJson<SavePlayerData>(json);

            fallGravityMult          = data.fallGravityMult;
            maxFallSpeed             = data.maxFallSpeed;
            fastFallGravityMult      = data.fastFallGravityMult;
            maxFastFallSpeed         = data.maxFastFallSpeed;
            runMaxSpeed              = data.runMaxSpeed;
            runAcceleration          = data.runAcceleration;
            runDecceleration        = data.run_decceleration;
            accelInAir               = data.accelInAir;
            deccelInAir              = data.deccelInAir;
            doConserveMomentum       = data.doConserveMomentum;
            jumpHeight               = data.jumpHeight;
            jumpTimeToApex           = data.jumpTimeToApex;
            jumpCutGravityMult       = data.jumpCutGravityMult;
            jumpHangGravityMult      = data.jumpHangGravityMult;
            jumpHangTimeThreshold    = data.jumpHangTimeThreshold;
            jumpHangAccelerationMult = data.jumpHangAccelerationMult;
            jumpHangMaxSpeedMult     = data.jumpHangMaxSpeedMult;
            wallJumpForce            = data.wallJumpForce;
            wallJumpRunLerp          = data.wallJumpRunLerp;
            wallJumpTime             = data.wallJumpTime;
            doTurnOnWallJump         = data.doTurnOnWallJump;
            slideSpeed               = data.slideSpeed;
            slideAccel               = data.slideAccel;
            grabStamina              = data.grabStamina;
            climbUpSpeed             = data.climbUpSpeed;
            dashAmount               = data.dashAmount;
            dashSpeed                = data.dashSpeed;
            dashSleepTime            = data.dashSleepTime;
            dashAttackTime           = data.dashAttackTime;
            dashEndTime              = data.dashEndTime;
            dashEndSpeed             = data.dashEndSpeed;
            dashEndRunLerp           = data.dashEndRunLerp;
            dashRefillTime           = data.dashRefillTime;
            coyoteTime               = data.coyoteTime;
            jumpInputBufferTime      = data.jumpInputBufferTime;
            dashInputBufferTime      = data.dashInputBufferTime;
            grabInputBufferTime      = data.grabInputBufferTime;

            CalculateDerivedFields();
        }
    }


    [System.Serializable]
    public class SavePlayerData
    {
        // 중력
        public float fallGravityMult;
        public float maxFallSpeed;
        public float fastFallGravityMult;
        public float maxFastFallSpeed;

        // 이동
        public float runMaxSpeed;
        public float runAcceleration;
        public float run_decceleration;
        public float accelInAir;
        public float deccelInAir;
        public bool doConserveMomentum;

        // 점프
        public float jumpHeight;
        public float jumpTimeToApex;
        public float jumpCutGravityMult;
        public float jumpHangGravityMult;
        public float jumpHangTimeThreshold;
        public float jumpHangAccelerationMult;
        public float jumpHangMaxSpeedMult;

        // 벽 점프
        public Vector2 wallJumpForce;
        public float wallJumpRunLerp;
        public float wallJumpTime;
        public bool doTurnOnWallJump;

        // 슬라이드
        public float slideSpeed;
        public float slideAccel;

        // 벽 붙잡기
        public float grabStamina;
        public float climbUpSpeed;

        // 대쉬
        public int dashAmount;
        public float dashSpeed;
        public float dashSleepTime;
        public float dashAttackTime;
        public float dashEndTime;
        public Vector2 dashEndSpeed;
        public float dashEndRunLerp;
        public float dashRefillTime;

        // 어시스트
        public float coyoteTime;
        public float jumpInputBufferTime;
        public float dashInputBufferTime;
        public float grabInputBufferTime;
    }
}

