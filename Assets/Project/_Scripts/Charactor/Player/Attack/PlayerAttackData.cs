using System.IO;
using UnityEngine;
using File = System.IO.File;



[CreateAssetMenu(menuName = "플레이어 공격 데이터")]
public class PlayerAttackData : ScriptableObject
{
    [Header("근거리 공격")]
    public float meleeAttackDamage;
    public float meleeAttackDelay;

    [Space(5)]
    [Header("원거리 공격")]
    public float rangeAttackMinDamage;
    public float rangeAttackMaxDamage;
    public float rangeAttackHoldMinTime;
    public float rangeAttackHoldMaxTime;
    public float maxFlyTime;
    public float bulletSpeed;

    //[ContextMenu("저장")]
    public void OnSave()
    {
        SavePlayerAttackData data = new SavePlayerAttackData()
        {
            meleeAttackDamage               = meleeAttackDamage,
            meleeAttackDelay                = meleeAttackDelay,

            rangeAttackMinDamage            = rangeAttackMinDamage,
            rangeAttackMaxDamage            = rangeAttackMaxDamage,
            rangeAttackHoldMinTime          = rangeAttackHoldMinTime,
            rangeAttackHoldMaxTime          = rangeAttackHoldMaxTime,
            maxFlyTime                      = maxFlyTime,
            bulletSpeed                     = bulletSpeed
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.dataPath, "PlayerAttackData.json");
        File.WriteAllText(path, json);
    }
    //[ContextMenu("로드")]
    public void OnLoad()
    {
        string path = Path.Combine(Application.persistentDataPath, "PlayerStateData.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SavePlayerAttackData data = JsonUtility.FromJson<SavePlayerAttackData>(json);

            meleeAttackDamage               = data.meleeAttackDamage;
            meleeAttackDelay                = data.meleeAttackDelay;

            rangeAttackMinDamage            = data.rangeAttackMinDamage;
            rangeAttackMaxDamage            = data.rangeAttackMaxDamage;
            rangeAttackHoldMinTime          = data.rangeAttackHoldMinTime;
            rangeAttackHoldMaxTime          = data.rangeAttackHoldMaxTime;
            maxFlyTime                      = data.maxFlyTime;
            bulletSpeed                     = data.bulletSpeed;
        }
    }


    [System.Serializable]
    public class SavePlayerAttackData
    {
        // 근접 공격
        public float meleeAttackDamage;
        public float meleeAttackDelay;

        // 원거리 공격
        public float rangeAttackMinDamage;
        public float rangeAttackMaxDamage;
        public float rangeAttackHoldMinTime;
        public float rangeAttackHoldMaxTime;
        public float maxFlyTime;
        public float bulletSpeed;
    }
}

