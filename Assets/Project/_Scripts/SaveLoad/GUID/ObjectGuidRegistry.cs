using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectGuidRegistry : MySingleton<ObjectGuidRegistry>
{
    [System.Serializable]
    public class GuidMapping
    {
        public Object objectToSave;
        public string guid;
    }

    [SerializeField] private List<GuidMapping> objectMapping = new List<GuidMapping>();
    private Dictionary<string, Object> guidToObejct = new Dictionary<string, Object>();

    protected override void Awake()
    {
        instance = this;

        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        foreach (var mapping in objectMapping)
        {
            guidToObejct[mapping.guid] = mapping.objectToSave;
        }
    }

    public T GetObject<T>(string _guid) where T : Object
    {
        if (guidToObejct.TryGetValue(_guid, out Object obj))
        {
            return obj as T;
        }

        return null;
    }

#if UNITY_EDITOR
    public void RefreshMappings()
    {
        objectMapping.Clear();

        string[] prefabsGuids = UnityEditor.AssetDatabase.FindAssets("t:Prefabs");

        foreach (string guid in prefabsGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            objectMapping.Add(new GuidMapping { objectToSave = prefab, guid = guid, });
        }


        string[] tileGuids = UnityEditor.AssetDatabase.FindAssets("t:Tile");

        foreach (string guid in tileGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            TileBase tile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>(path);
            objectMapping.Add(new GuidMapping{ objectToSave = tile, guid = guid, });
        }
    }
#endif
}
