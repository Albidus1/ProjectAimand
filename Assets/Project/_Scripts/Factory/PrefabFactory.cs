using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrefabFactory<T> : MonoBehaviour where T : Product
{
    [SerializeField]
    protected T prefab;
    [SerializeField]
    protected uint maxInstance;
    protected List<T> activeProducts = new List<T>();
    protected List<T> disableProducts = new List<T>();

    public Product GetProduct()
    {
        T result;
        // 비활성화 된 product가 있으면 가장 오래된 product부터 반환
        if (disableProducts.Count > 0)
        {
            result = disableProducts[0];
            disableProducts.RemoveAt(0);
            activeProducts.Add(result);
            result.gameObject.SetActive(true);

            return result;
        }
        // 팩토리 최대 개수에 도달하지 않았으면 새롭게 생성
        else if (activeProducts.Count < maxInstance)
        {
            result = CreateProduct();
            activeProducts.Add(result);

            return result;
        }
        // 팩토리 최대 개수에 도달할때 요청하면 가장 오래전에 만들어진 product 반환
        else
        {
            // 혹시나 activeProducts의 크기가 0일 경우를 위한 try catch문
            try
            {
                if (activeProducts.Count == 0) throw new ArgumentOutOfRangeException("Product의 개수에 오류가 있습니다.");
                result = activeProducts[0];
                result.gameObject.SetActive(false);
                result.gameObject.SetActive(true);
                activeProducts.RemoveAt(0);
                activeProducts.Add(result);

                return result;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e.Message);
            }
        }

        throw new Exception("Product 반환에 오류가 발생했습니다.");
    }

    private T CreateProduct()
    {
        T result = GameObject.Instantiate(prefab);
        result.onDisable.AddListener(() => { OnProductDisable(result); });
        return result;
    }


    private void OnProductDisable(T product)
    {
        activeProducts.Remove(product);
        disableProducts.Add(product);
    }
}
