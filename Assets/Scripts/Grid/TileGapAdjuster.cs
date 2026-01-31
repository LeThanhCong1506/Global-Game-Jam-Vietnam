// File: Scripts/Grid/TileGapAdjuster.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Automatically adjusts tile positions at runtime to add gaps between adjacent tiles.
    /// Also expands click area for better mouse responsiveness.
    /// Attach to a manager object in the scene.
    /// </summary>
    public class TileGapAdjuster : MonoBehaviour
    {
        [Header("Gap Settings")]
        [Tooltip("Gap distance to add between tiles")]
        [SerializeField] private float gapDistance = 0.1f;

        [Header("Click Area Settings")]
        [Tooltip("How much larger the click area should be (1.0 = same size, 1.3 = 30% larger)")]
        [SerializeField] private float clickAreaMultiplier = 1.3f;
        
        [Tooltip("Height of the click collider")]
        [SerializeField] private float clickColliderHeight = 0.5f;

        [Header("Auto Run")]
        [SerializeField] private bool adjustPositionsOnStart = true;
        [SerializeField] private bool expandClickOnStart = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            if (adjustPositionsOnStart)
            {
                AdjustAllTilePositions();
            }

            if (expandClickOnStart)
            {
                ExpandAllTileClickAreas();
            }
        }

        [ContextMenu("Adjust All Tile Positions")]
        public void AdjustAllTilePositions()
        {
            // Find all TileData in the scene
            TileData[] allTiles = FindObjectsOfType<TileData>();

            if (allTiles.Length == 0)
            {
                Debug.LogWarning("[TileGapAdjuster] No tiles found in scene!");
                return;
            }

            foreach (TileData tile in allTiles)
            {
                AdjustTilePosition(tile);
            }

            if (showDebugLogs)
            {
                Debug.Log($"[TileGapAdjuster] Adjusted {allTiles.Length} tiles with gap: {gapDistance}");
            }
        }

        private void AdjustTilePosition(TileData tile)
        {
            Vector2Int gridCoord = tile.GridCoord;
            
            // Calculate new position with gap
            // Position = gridCoord * 1 (tile size) + gridCoord * gap
            float newX = gridCoord.x + (gridCoord.x * gapDistance);
            float newZ = gridCoord.y + (gridCoord.y * gapDistance);

            // Keep original Y position (height)
            float currentY = tile.transform.position.y;

            Vector3 newPosition = new Vector3(newX, currentY, newZ);
            tile.transform.position = newPosition;

            if (showDebugLogs)
            {
                Debug.Log($"[TileGapAdjuster] Tile ({gridCoord.x},{gridCoord.y}) moved to {newPosition}");
            }
        }

        [ContextMenu("Expand All Tile Click Areas")]
        public void ExpandAllTileClickAreas()
        {
            TileData[] allTiles = FindObjectsOfType<TileData>();

            if (allTiles.Length == 0)
            {
                Debug.LogWarning("[TileGapAdjuster] No tiles found in scene!");
                return;
            }

            int expandedCount = 0;
            foreach (TileData tile in allTiles)
            {
                ExpandTileClickArea(tile);
                expandedCount++;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[TileGapAdjuster] Expanded click area for {expandedCount} tiles (multiplier: {clickAreaMultiplier}x)");
            }
        }

        private void ExpandTileClickArea(TileData tile)
        {
            // Check if already has click expander child
            Transform existingExpander = tile.transform.Find("ClickExpander");
            if (existingExpander != null)
            {
                UpdateClickExpander(existingExpander.gameObject, tile);
                return;
            }

            // Create invisible child object with larger collider
            GameObject clickExpander = new GameObject("ClickExpander");
            clickExpander.transform.SetParent(tile.transform);
            clickExpander.transform.localPosition = Vector3.zero;
            clickExpander.transform.localRotation = Quaternion.identity;

            // Add box collider
            BoxCollider clickCollider = clickExpander.AddComponent<BoxCollider>();
            
            // Get tile's current scale
            Vector3 tileScale = tile.transform.localScale;
            
            // Calculate expanded size (accounting for parent scale)
            float expandedSizeX = (tileScale.x * clickAreaMultiplier) / tileScale.x;
            float expandedSizeZ = (tileScale.z * clickAreaMultiplier) / tileScale.z;
            
            clickCollider.size = new Vector3(expandedSizeX, clickColliderHeight / tileScale.y, expandedSizeZ);
            clickCollider.center = Vector3.zero;
            clickCollider.isTrigger = false;
        }

        private void UpdateClickExpander(GameObject expander, TileData tile)
        {
            BoxCollider clickCollider = expander.GetComponent<BoxCollider>();
            if (clickCollider == null)
            {
                clickCollider = expander.AddComponent<BoxCollider>();
            }

            Vector3 tileScale = tile.transform.localScale;
            float expandedSizeX = (tileScale.x * clickAreaMultiplier) / tileScale.x;
            float expandedSizeZ = (tileScale.z * clickAreaMultiplier) / tileScale.z;

            clickCollider.size = new Vector3(expandedSizeX, clickColliderHeight / tileScale.y, expandedSizeZ);
            clickCollider.center = Vector3.zero;
        }

        [ContextMenu("Reset Tile Positions (No Gap)")]
        public void ResetTilePositions()
        {
            TileData[] allTiles = FindObjectsOfType<TileData>();

            foreach (TileData tile in allTiles)
            {
                Vector2Int gridCoord = tile.GridCoord;
                float currentY = tile.transform.position.y;
                Vector3 newPosition = new Vector3(gridCoord.x, currentY, gridCoord.y);
                tile.transform.position = newPosition;
            }

            Debug.Log($"[TileGapAdjuster] Reset {allTiles.Length} tiles to original positions");
        }

        [ContextMenu("Remove All Click Expanders")]
        public void RemoveAllClickExpanders()
        {
            TileData[] allTiles = FindObjectsOfType<TileData>();

            foreach (TileData tile in allTiles)
            {
                Transform expander = tile.transform.Find("ClickExpander");
                if (expander != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(expander.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(expander.gameObject);
                    }
                }
            }

            Debug.Log("[TileGapAdjuster] Removed all click expanders");
        }
    }
}
