using UnityEngine;
using UnityEngine.Events;

public class MyPoolableObject : MonoBehaviour
{
    [Header("이벤트")]
    public UnityEvent ExecuteOnEnable;
    public UnityEvent ExecuteOnDisable;

    public delegate void Events();
    public event Events OnSpawnComplete;

    [Header("풀링된 오브젝트")]
    /// 0이면 무한지속
    public float lifeTime = 0f;


    public void Destroy()
    {
        gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        if (lifeTime > 0f)
        {
            Invoke(nameof(Destroy), lifeTime);
        }

        ExecuteOnEnable?.Invoke();
    }

    protected virtual void OnDisable()
    {
        ExecuteOnDisable?.Invoke();
        CancelInvoke();
    }

    public void TriggerOnSpawnComplete()
    {
        OnSpawnComplete?.Invoke();
    }
}
