using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MoveElement : ICloneable
{
    public Vector2 movePosition;
    public Ease moveEase;
    public float moveSpeed;
    public float rotate;
    public float waitTime;

    public object Clone()
    {
        return this.MemberwiseClone() as MoveElement;
    }
}

[CreateAssetMenu(menuName = "보스 이동 패턴")]
public class MovePattern : ScriptableObject
{
    public List<MoveElement> moveElements = new List<MoveElement>();
}
