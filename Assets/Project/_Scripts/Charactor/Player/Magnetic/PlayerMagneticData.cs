using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "플레이어 자력 데이터")]
public class PlayerMagneticData : ScriptableObject
{
    [Header("인력")]
    public float PullAngle = 45f;
    public float PullRange = 20f;
    public float PullPowerMax = 10f;
    public float PullPowerMin = 1f;
    public float PullPowerMaxRange = 4f;

    [Space(5)]
    [Header("척력")]
    public float PushRange = 10f;
    public float PushPower = 8f;

    public void OnSave()
    {
        SavePlayerMagneticData data = new SavePlayerMagneticData()
        {
            PullAngle = PullAngle,
            PullRange = PullRange,
            PullPowerMax = PullPowerMax,
            PullPowerMin = PullPowerMin,
            PullPowerMaxRange = PullPowerMaxRange,

            PushRange = PushRange,
            PushPower = PushPower
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.dataPath, "PlayerMagneticData.json");
        File.WriteAllText(path, json);
    }
    //[ContextMenu("로드")]
    public void OnLoad()
    {
        string path = Path.Combine(Application.persistentDataPath, "PlayerMagneticData.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SavePlayerMagneticData data = JsonUtility.FromJson<SavePlayerMagneticData>(json);

            PullAngle = data.PullAngle;
            PullRange = data.PullRange;
            PullPowerMax = data.PullPowerMax;
            PullPowerMin = data.PullPowerMin;
            PullPowerMaxRange = data.PullPowerMaxRange;

            PushRange = data.PushRange;
            PushPower = data.PushPower;
        }
    }

    [System.Serializable]
    public class SavePlayerMagneticData
    {
        // 인력
        public float PullAngle;
        public float PullRange;
        public float PullPowerMax;
        public float PullPowerMin;
        public float PullPowerMaxRange;

        // 척력
        public float PushRange;
        public float PushPower;
    }
}
