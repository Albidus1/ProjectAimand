using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBulletFactory : PrefabFactory<PlayerBullet>
{
    private static PlayerBulletFactory instance;
    public static PlayerBulletFactory Instance
    {
        get
        {
            if (instance == null)
                throw new Exception("Instance가 먼저 Awake에서 할당된 후에 호출하십시오.");

            return instance;
        }
    }

    private void Awake()
    {
        // Instance 초기화 및 중복시 제거
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("PlayerBulletFactory가 중복되어 제거합니다.");
            DestroyImmediate(this);
        }
    }

    [Obsolete("GetProduct()를 직접 호출하지 마십시오.", true)]
    public new Product GetProduct()
    {
        return base.GetProduct();
    }

    public Product GetProduct(float damage, float flyTime, float speed)
    {
        Product result = base.GetProduct();
        ((PlayerBullet)result).Initialize(damage, flyTime, speed);

        return result;
    }
}
