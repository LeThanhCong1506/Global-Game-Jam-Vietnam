// File: Scripts/Grid/TileData.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Data component holding tile properties.
    /// Attach to each tile GameObject in the grid.
    /// </summary>
    public class TileData : MonoBehaviour
    {
        [Header("Grid Position")]
        [SerializeField] private Vector2Int gridCoord;

        [Header("Tile Properties")]
        [SerializeField] private bool isWalkable = true;
        [SerializeField] private bool isTrap = false;
        [SerializeField] private bool isExit = false;
        [SerializeField] private bool isStart = false;
        [SerializeField] private bool isKeySpawn = false;

        [Header("Hidden Tile (only visible under certain masks)")]
        [Tooltip("If true, this tile is hidden by default (e.g., hidden bridge)")]
        [SerializeField] private bool isHiddenTile = false;

        // Events for tile interactions
        public event Action<TileData> OnTileEntered;

        // Properties for external access
        public Vector2Int GridCoord => gridCoord;
        public bool IsWalkable => isWalkable;
        public bool IsTrap => isTrap;
        public bool IsExit => isExit;
        public bool IsStart => isStart;
        public bool IsKeySpawn => isKeySpawn;
        public bool IsHiddenTile => isHiddenTile;

        /// <summary>
        /// Set grid coordinate (used by GridManager during setup).
        /// </summary>
        public void SetGridCoord(Vector2Int coord)
        {
            gridCoord = coord;
        }

        /// <summary>
        /// Set walkable status at runtime.
        /// </summary>
        public void SetWalkable(bool walkable)
        {
            isWalkable = walkable;
        }

        /// <summary>
        /// Called when player enters this tile.
        /// </summary>
        public void NotifyTileEntered()
        {
            OnTileEntered?.Invoke(this);
        }

        /// <summary>
        /// Check if this tile is adjacent to another tile (Manhattan distance = 1).
        /// </summary>
        public bool IsAdjacentTo(Vector2Int otherCoord)
        {
            int dx = Mathf.Abs(gridCoord.x - otherCoord.x);
            int dy = Mathf.Abs(gridCoord.y - otherCoord.y);
            return (dx + dy) == 1;
        }
    }
}
