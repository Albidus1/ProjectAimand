using System.IO;
using UnityEngine;



public static class MySaveLoadManager
{
    public static IMySaveLoadManagerMethod SaveLoadMethod = new MySaveLoadManagerMethodBinary();
    private const string m_baseForderName = "/MyData/";
    private const string m_defaultForderName = "SaveLoadManager";


    private static string DetermineSavePath(string folderName = m_defaultForderName)
    {
        string savePath = Application.persistentDataPath + m_baseForderName;

#if UNITY_EDITOR
        savePath = Application.dataPath + m_baseForderName;
#endif

        savePath = savePath + folderName + "/";
        return savePath;
    }

    public static void Save(object _saveObject, string _fileName, string _folderName = m_defaultForderName)
    {
        string svaePath = DetermineSavePath(_folderName);
        string saveFileName = _fileName;

        if (!Directory.Exists(svaePath))
        {
            Directory.CreateDirectory(svaePath);
        }

        FileStream saveFile = File.Create(svaePath + saveFileName);

        SaveLoadMethod.Save(_saveObject, saveFile);
        saveFile.Close();
    }

    public static object Load(System.Type _objectType, string _fileName, string _folderName = m_defaultForderName)
    {
        string svaePath = DetermineSavePath(_folderName);
        string saveFileName = svaePath + _fileName;

        if (!Directory.Exists(svaePath) || !File.Exists(saveFileName))
        {
            return null;
        }

        FileStream saveFile = File.Open(saveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        object loadObject = SaveLoadMethod.Load(_objectType, saveFile);
        saveFile.Close();

        return loadObject;
    }
}
