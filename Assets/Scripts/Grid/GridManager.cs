// File: Scripts/Grid/GridManager.cs
using System.Collections.Generic;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Manages the tile grid - stores tiles, provides queries.
    /// Singleton for easy access from other systems.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private Transform tilesParent;
        [SerializeField] private LayerMask tileLayerMask = -1;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Dictionary storing all tiles by their grid coordinate
        private Dictionary<Vector2Int, TileData> tilesByCoord = new Dictionary<Vector2Int, TileData>();

        // Quick access to special tiles
        public TileData StartTile { get; private set; }
        public TileData ExitTile { get; private set; }
        public TileData KeySpawnTile { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeGrid();
        }

        private void InitializeGrid()
        {
            tilesByCoord.Clear();

            // Find all tiles under parent (or in scene if no parent specified)
            TileData[] allTiles;
            if (tilesParent != null)
            {
                allTiles = tilesParent.GetComponentsInChildren<TileData>();
            }
            else
            {
                allTiles = FindObjectsOfType<TileData>();
            }

            foreach (var tile in allTiles)
            {
                Vector2Int coord = tile.GridCoord;

                if (tilesByCoord.ContainsKey(coord))
                {
                    Debug.LogWarning($"[GridManager] Duplicate tile at {coord}. Skipping.");
                    continue;
                }

                tilesByCoord[coord] = tile;

                // Track special tiles
                if (tile.IsStart)
                {
                    StartTile = tile;
                }
                if (tile.IsExit)
                {
                    ExitTile = tile;
                }
                if (tile.IsKeySpawn)
                {
                    KeySpawnTile = tile;
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"[GridManager] Initialized with {tilesByCoord.Count} tiles.");
            }
        }

        /// <summary>
        /// Get tile at specific grid coordinate.
        /// </summary>
        public TileData GetTileAt(Vector2Int coord)
        {
            tilesByCoord.TryGetValue(coord, out TileData tile);
            return tile;
        }

        /// <summary>
        /// Get all adjacent tiles (up, down, left, right).
        /// </summary>
        public List<TileData> GetAdjacentTiles(Vector2Int coord)
        {
            List<TileData> adjacent = new List<TileData>();

            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var dir in directions)
            {
                TileData tile = GetTileAt(coord + dir);
                if (tile != null)
                {
                    adjacent.Add(tile);
                }
            }

            return adjacent;
        }

        /// <summary>
        /// Check if two coordinates are adjacent (Manhattan distance = 1).
        /// </summary>
        public bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) == 1;
        }

        /// <summary>
        /// Raycast from screen position to find tile.
        /// </summary>
        public TileData RaycastToTile(Vector3 screenPosition, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            Ray ray = camera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, tileLayerMask))
            {
                TileData tile = hit.collider.GetComponent<TileData>();
                if (tile == null)
                {
                    tile = hit.collider.GetComponentInParent<TileData>();
                }
                return tile;
            }

            return null;
        }

        /// <summary>
        /// Get world position for a grid coordinate (center of tile).
        /// </summary>
        public Vector3 GetWorldPosition(Vector2Int coord)
        {
            TileData tile = GetTileAt(coord);
            if (tile != null)
            {
                return tile.transform.position;
            }

            // Estimate position if tile doesn't exist (1 unit spacing assumed)
            return new Vector3(coord.x, 0, coord.y);
        }

        /// <summary>
        /// Rebuild grid (call after adding/removing tiles at runtime).
        /// </summary>
        public void RebuildGrid()
        {
            InitializeGrid();
        }
    }
}
