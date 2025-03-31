using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Tilemaps;



[System.Serializable]
public class SaveMapData
{
    public List<GameObjectData> gameObjects = new List<GameObjectData>();
    public List<TilemapData> tilemapDataList = new List<TilemapData>();
}

[System.Serializable]
public class GameObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public List<KeyValue> componentData = new List<KeyValue>();
}

[System.Serializable]
public class KeyValue
{
    public string key;
    public string value;

    public KeyValue(string _key, string _value)
    {
        key = _key;
        value = _value;
    }
}


[System.Serializable]
public class TilemapData
{
    public List<TileInformation> tiles = new List<TileInformation>();
}

[System.Serializable]
public class TileInformation
{
    //public TileBase tileBase;
    public Vector3Int position;
    public string tileGuid;
    public string tileResourcePath;
}


//public class TestCls
//{
//    public int a;
//    public float b;

//    public string toJsonStr()
//    {
//        string str = "{a:10}";

//        var typedata = this.GetType();
//        var fieldlist = typedata.GetFields();

//        ////foreach (var item in fieldlist)
//        ////{
//        ////    item.Name;
//        ////    item.GetValue(this).ToString();

//        ////}


//        return str;
//    }
//}
