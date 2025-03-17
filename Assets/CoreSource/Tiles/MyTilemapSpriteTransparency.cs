using UnityEngine;
using UnityEngine.Tilemaps;



public class MyTilemapSpriteTransparency : MonoBehaviour
{
    private Tilemap tilemap;
    public MyTiles transparentTile;



    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        AdjustTransparency();
    }

    public void AdjustTransparency()
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(position);

                if (tile == null)
                    continue;

                if (tile == transparentTile)
                {
                    tilemap.SetColor(position, new Color(1f, 1f, 1f, 0));
                }
            }
        }
    }
}
