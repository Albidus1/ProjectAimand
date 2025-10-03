using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MySaveLoadManagerMethodBinary : IMySaveLoadManagerMethod
{
    public void Save(object _saveObject, FileStream _saveFile)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(_saveFile, _saveObject);
        _saveFile.Close();
    }

    public object Load(System.Type _objectType, FileStream _saveFile)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        object loadObject = formatter.Deserialize(_saveFile);
        _saveFile.Close();

        return loadObject;
    }
}
