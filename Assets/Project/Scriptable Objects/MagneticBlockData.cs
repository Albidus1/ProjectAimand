using System.Collections;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using File = System.IO.File;



[CreateAssetMenu(menuName = "자력블록 데이터")]
public class MagneticBlockData : ScriptableObject
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
    public float run_decceleration;
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
    [Space(5)]

    public float jumpCutGravityMult;
    [Range(0f, 1.0f)] public float jumpHangGravityMult;
    public float jumpHangTimeThreshold;
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;




    private void OnValidate() => CalculateDerivedFields();
    private void CalculateDerivedFields()
    {
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        gravityScale = gravityStrength / Physics2D.gravity.y;

        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * run_decceleration) / runMaxSpeed;

        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        run_decceleration = Mathf.Clamp(run_decceleration, 0.01f, runMaxSpeed);
    }

    ////[ContextMenu("저장")]
    //public void OnSave()
    //{
    //    SavePlayerData data = new SavePlayerData()
    //    {
    //        fallGravityMult = fallGravityMult,
    //        maxFallSpeed = maxFallSpeed,
    //        fastFallGravityMult = fastFallGravityMult,
    //        maxFastFallSpeed = maxFastFallSpeed,
    //        runMaxSpeed = runMaxSpeed,
    //        runAcceleration = runAcceleration,
    //        run_decceleration = run_decceleration,
    //        accelInAir = accelInAir,
    //        deccelInAir = deccelInAir,
    //        doConserveMomentum = doConserveMomentum,
    //        jumpHeight = jumpHeight,
    //        jumpTimeToApex = jumpTimeToApex,
    //        jumpCutGravityMult = jumpCutGravityMult,
    //        jumpHangGravityMult = jumpHangGravityMult,
    //        jumpHangTimeThreshold = jumpHangTimeThreshold,
    //        jumpHangAccelerationMult = jumpHangAccelerationMult,
    //        jumpHangMaxSpeedMult = jumpHangMaxSpeedMult,          
    //    };

    //    string json = JsonUtility.ToJson(data, true);
    //    string path = Path.Combine(Application.dataPath, "MagneticBlockData.json");
    //    File.WriteAllText(path, json);
    //}
    ////[ContextMenu("로드")]
    //public void OnLoad()
    //{
    //    string path = Path.Combine(Application.persistentDataPath, "MagneticBlockData.json");

    //    if (File.Exists(path))
    //    {
    //        string json = File.ReadAllText(path);
    //        SavePlayerData data = JsonUtility.FromJson<SavePlayerData>(json);

    //        fallGravityMult = data.fallGravityMult;
    //        maxFallSpeed = data.maxFallSpeed;
    //        fastFallGravityMult = data.fastFallGravityMult;
    //        maxFastFallSpeed = data.maxFastFallSpeed;
    //        runMaxSpeed = data.runMaxSpeed;
    //        runAcceleration = data.runAcceleration;
    //        run_decceleration = data.run_decceleration;
    //        accelInAir = data.accelInAir;
    //        deccelInAir = data.deccelInAir;
    //        doConserveMomentum = data.doConserveMomentum;
    //        jumpHeight = data.jumpHeight;
    //        jumpTimeToApex = data.jumpTimeToApex;
    //        jumpCutGravityMult = data.jumpCutGravityMult;
    //        jumpHangGravityMult = data.jumpHangGravityMult;
    //        jumpHangTimeThreshold = data.jumpHangTimeThreshold;
    //        jumpHangAccelerationMult = data.jumpHangAccelerationMult;
    //        jumpHangMaxSpeedMult = data.jumpHangMaxSpeedMult;
            
    //        CalculateDerivedFields();
    //    }
    //}

    //[System.Serializable]
    //public class SavePlayerData
    //{
    //    // 중력
    //    public float fallGravityMult;
    //    public float maxFallSpeed;
    //    public float fastFallGravityMult;
    //    public float maxFastFallSpeed;

    //    // 이동
    //    public float runMaxSpeed;
    //    public float runAcceleration;
    //    public float run_decceleration;
    //    public float accelInAir;
    //    public float deccelInAir;
    //    public bool doConserveMomentum;

    //    // 점프
    //    public float jumpHeight;
    //    public float jumpTimeToApex;
    //    public float jumpCutGravityMult;
    //    public float jumpHangGravityMult;
    //    public float jumpHangTimeThreshold;
    //    public float jumpHangAccelerationMult;
    //    public float jumpHangMaxSpeedMult;       
    //}
}
