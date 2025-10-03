using System.IO;
using UnityEngine;



public interface IMySaveLoadManagerMethod
{
    void Save(object _saveObject, FileStream _saveFile);
    object Load(System.Type _objectType, FileStream _saveFile);
}

public enum MySaveLoadManagerMethods
{
    Json,
    Binary
}
