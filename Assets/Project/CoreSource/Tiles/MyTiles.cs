using UnityEngine;
using UnityEngine.Tilemaps;



[CreateAssetMenu(fileName = "TransparentTile", menuName = "MyTiles/TransparentTile")]
public class MyTiles : Tile
{
    [Range(0f, 1f)]
    public float transparency = 0;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.color = new Color(1f, 1f, 1f, transparency);
    }
}
