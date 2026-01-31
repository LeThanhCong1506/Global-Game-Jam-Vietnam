// File: Scripts/Grid/TileFactory.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Helper to create tiles at runtime using primitives.
    /// Use this for quick prototyping without imported assets.
    /// </summary>
    public static class TileFactory
    {
        /// <summary>
        /// Create a single tile at world position.
        /// </summary>
        public static GameObject CreateTile(Vector2Int gridCoord, Vector3 worldPos, Transform parent = null)
        {
            // Create cube primitive
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = $"Tile_{gridCoord.x}_{gridCoord.y}";
            tile.transform.position = worldPos;
            tile.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);

            if (parent != null)
            {
                tile.transform.SetParent(parent);
            }

            // Add components
            TileData tileData = tile.AddComponent<TileData>();
            tileData.SetGridCoord(gridCoord);

            tile.AddComponent<TileVisual>();

            return tile;
        }

        /// <summary>
        /// Create a grid of tiles.
        /// </summary>
        public static void CreateGrid(int width, int height, float tileSpacing, Transform parent = null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Vector3 worldPos = new Vector3(x * tileSpacing, 0, y * tileSpacing);
                    CreateTile(coord, worldPos, parent);
                }
            }
        }
    }
}
