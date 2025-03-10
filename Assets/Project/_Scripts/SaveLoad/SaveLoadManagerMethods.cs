using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;




public interface ISaveLoadManagerMethods
{
    string Save();
    void Load(string _json);
}

public abstract class SaveLoadManagerMethods : MonoBehaviour
{
    public virtual string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public abstract void Load(string _json);
}
