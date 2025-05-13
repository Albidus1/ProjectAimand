using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;





public class LevelManager : MySingleton<LevelManager>
{
    public PlayerMovement playerPrefabs;

    public CheckPoint debugSpawn;
    public CheckPoint currentCheckPoint;
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
