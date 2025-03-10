using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Tilemaps;
using File = System.IO.File;



public class SaveAndLoadHandler : MonoBehaviour
{
    [SerializeField] private Tilemap[] tilemaps;

    [SerializeField] private Tilemap propsTilemap;
    [SerializeField] private List<GameObject> prefabsList = new List<GameObject>();

    [SerializeField] private string saveFileName = "save.json";
    protected string saveFilePath => Application.dataPath;



    #region SAVE
    [ContextMenu("저장")]
    public void OnSave()
    {
        SaveData saveData = new SaveData();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != null)
            {
                saveData.tilemapDataList.Add(SaveTilemap(tilemap));
            }
        }

        foreach (Transform child in propsTilemap.transform)
        {
            if (child != null)
            {
                saveData.gameObjects.Add(SaveGameObject(child));
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetSavePath(), json);
    }

    private TilemapData SaveTilemap(Tilemap _tilemap)
    {
        TilemapData data = new TilemapData();

        BoundsInt bounds = _tilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = _tilemap.GetTile(pos);

                if (pos != null)
                {
#if UNITY_EDITOR
                    data.tiles.Add(new TileInformation
                    {
                        position = pos,
                        tileGuid = UnityEditor.AssetDatabase.AssetPathToGUID(
                            UnityEditor.AssetDatabase.GetAssetPath(tile)),
                        tileResourcePath = ""
                    });
#else
                    data.tiles.Add(new TileInformation
                    {
                        position = pos,
                        tileGuid = tile.name
                    });
#endif
                }
            }
        }

        return data;
    }

    private GameObjectData SaveGameObject(Transform _child)
    {
        string prefabsName = prefabsList.Find(p => p.name == _child.name)?.name ?? "Unknown";

        GameObjectData data = new GameObjectData
        {
            prefabName = prefabsName,
            position = _child.position,
            rotation = _child.rotation,
            componentData = new List<KeyValue>()
        };

        bool hasComponentData = false;

        foreach (var component in _child.GetComponents<MonoBehaviour>())
        {
            if (component is ISaveLoadManagerMethods serializable)
            {
                string savedData = serializable.Save();
                //Debug.Log($"[Save] {component.GetType().Name} -> {savedData}");
                data.componentData.Add(new KeyValue(component.GetType().Name, savedData));

                hasComponentData = true;
            }
        }

        if (false == hasComponentData)
        {
            data.componentData.Add(new KeyValue("NoData", ""));
        }

        return data;
    }
#endregion

    #region LOAD
    [ContextMenu("로드")]
    public void OnLoad()
    {
        string path = Path.Combine(saveFilePath, saveFileName);
        Debug.Log("Load Path: " + path);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            ClearScene();

            for (int i = 0; i < saveData.tilemapDataList.Count; i++)
            {
                if (i < tilemaps.Length && tilemaps[i] != null)
                {
                    LoadTilemap(tilemaps[i], saveData.tilemapDataList[i]);
                }
            }

            LoadGameObjects(saveData);
        }
    }

    private void ClearScene()
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
            }
        }

        foreach (Transform child in propsTilemap.transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                //Undo.DestroyObjectImmediate(child.gameObject);
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void LoadTilemap(Tilemap _tilemap, TilemapData _data)
    {
        foreach (var tileInfo in _data.tiles)
        {
#if UNITY_EDITOR
            TileBase tile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>(
            UnityEditor.AssetDatabase.GUIDToAssetPath(tileInfo.tileGuid));

            _tilemap.SetTile(tileInfo.position, tile);

            //TileBase tiles = Resources.Load<TileBase>()
#else
            TileBase tile = U
#endif
        }
    }

    private void LoadGameObjects(SaveData _saveData)
    {
        Dictionary<string, GameObject> prefabDirctionary = new Dictionary<string, GameObject>();

        foreach (var prefab in prefabsList)
        {
            prefabDirctionary[prefab.name] = prefab;
        }

        foreach (var data in _saveData.gameObjects)
        {
            GameObject prefab;

            if (prefabDirctionary.TryGetValue(data.prefabName, out prefab))
            {
                GameObject obj = Instantiate(prefab, data.position, data.rotation, propsTilemap.transform);
                obj.name = prefab.name;


                Dictionary<string, string> componentDictiionary = new Dictionary<string, string>();

                foreach (var item in data.componentData)
                {
                    componentDictiionary[item.key] = item.value;
                }

                foreach (var component in obj.GetComponents<MonoBehaviour>())
                {
                    if (component is ISaveLoadManagerMethods serializable &&
                        componentDictiionary.TryGetValue(component.GetType().Name, out string componentJson))
                    {
                        serializable.Load(componentJson);
                    }
                }
            }
        }
    }
    #endregion

    #region FILE
    private string GetSavePath(string _path = "")
    {
        if (_path == "")
        {
            _path = saveFilePath;
        }

        Debug.Log("Save Path: " + _path);

        return Path.Combine(_path, saveFileName);
    }

#if UNITY_EDITOR
    //private string GetResourcesPath(string _path)
    //{
    //    string resourcesPath;

    //    return resourcesPath;
    //}
#endif
#endregion
}