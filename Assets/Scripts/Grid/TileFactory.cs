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
        // Default gap between tiles
        public static float DefaultTileGap = 0.1f;

        /// <summary>
        /// Create a single tile at world position.
        /// </summary>
        public static GameObject CreateTile(Vector2Int gridCoord, Vector3 worldPos, Transform parent = null, float tileGap = 0.1f)
        {
            // Create cube primitive
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = $"Tile_{gridCoord.x}_{gridCoord.y}";
            tile.transform.position = worldPos;
            
            // Scale tile to be smaller to show gap (1 - gap = visible tile size)
            float tileScale = 1f - tileGap;
            tile.transform.localScale = new Vector3(tileScale, 0.2f, tileScale);

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
        /// Create a grid of tiles with gap between them.
        /// </summary>
        public static void CreateGrid(int width, int height, float tileSize = 1f, float tileGap = 0.1f, Transform parent = null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    // Position = coord * tileSize + coord * gap
                    float posX = x * tileSize + x * tileGap;
                    float posZ = y * tileSize + y * tileGap;
                    Vector3 worldPos = new Vector3(posX, 0, posZ);
                    CreateTile(coord, worldPos, parent, tileGap);
                }
            }
        }
    }
}
