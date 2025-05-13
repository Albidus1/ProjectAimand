using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;





public class LevelManager : MySingleton<LevelManager>
{
    public PlayerMovement playerPrefabs;

    [Header("체크 포인트")]
    [Tooltip("캐릭터를 강제로 이곳에서 스폰하고 싶을 때 사용할 체크포인트")]
    public CheckPoint debugSpawn;

    [Tooltip("현재 체크포인트")]
    [MyReadOnly]public CheckPoint currentCheckPoint;

    public virtual List<CheckPoint> checkPoints { get; private set; }



    public virtual void SetCurrentCheckPoint(CheckPoint _newCheckPoint)
    {
        if (currentCheckPoint == null)
        {
            currentCheckPoint = _newCheckPoint;
            return;
        }
    }
}
