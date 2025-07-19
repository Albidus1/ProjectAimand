using UnityEngine;
using UnityEngine.Events;

public abstract class Product : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent onDisable;

    protected void OnDisable()
    {
        onDisable.Invoke();
    }

    public void DestroyProduct()
    {
        this.gameObject.SetActive(false);
    }
}
