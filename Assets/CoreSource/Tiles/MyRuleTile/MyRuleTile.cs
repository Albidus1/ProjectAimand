using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;



[CreateAssetMenu(menuName = "Tiles/MyRuleTile")]
public class MyRuleTile : RuleTile<MyRuleTile.Neighbor>
{
    public bool alwaysConnect;
    public TileBase[] tilesToConnect;
    public bool checkSelf;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Any = 3;
        public const int Specified = 4;
        public const int Empty = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        //var tilemaplist = GameObject.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        //tilemaplist[0].GetTile()

        
        switch (neighbor)
        {
            case Neighbor.This:
                return CheckThis(other);
            case Neighbor.NotThis:
                return CheckNotThis(other);
            case Neighbor.Any:
                return CheckAny(other);
            case Neighbor.Specified:
                return CheckSpecified(other);
            case Neighbor.Empty:
                return CheckEmpty(other);
        }

        return base.RuleMatch(neighbor, other);
    }

    private bool CheckThis(TileBase _tile)
    {
        if (false == alwaysConnect)
        {
            return _tile == this;
        }
        else
        {
            return tilesToConnect.Contains(this) || _tile == this;
        }
    }

    private bool CheckNotThis(TileBase _tile)
    {
        return _tile != this;
    }

    private bool CheckAny(TileBase _tile)
    {
        if (true == checkSelf)
        {
            return _tile != null;
        }
        else
        {
            return _tile != null && _tile != this;
        }
    }

    private bool CheckSpecified(TileBase _tile)
    {
        return tilesToConnect.Contains(_tile);
    }

    private bool CheckEmpty(TileBase _tile)
    {
        return _tile == null;
    }
}
