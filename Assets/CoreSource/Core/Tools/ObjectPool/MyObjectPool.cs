using System.Collections.Generic;
using UnityEngine;

public class MyObjectPool : MonoBehaviour
{
    [MyReadOnly]
    public List<GameObject> pooledObjects;
}
