// File: Scripts/Grid/TileClickExpander.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Expands the clickable area of tiles by adding a larger invisible collider.
    /// Attach to manager object - runs automatically on Start.
    /// </summary>
    public class TileClickExpander : MonoBehaviour
    {
        [Header("Click Area Settings")]
        [Tooltip("How much larger the click area should be (1.0 = same size, 1.2 = 20% larger)")]
        [SerializeField] private float clickAreaMultiplier = 1.3f;
        
        [Tooltip("Height of the click collider")]
        [SerializeField] private float clickColliderHeight = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool expandOnStart = true;
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            if (expandOnStart)
            {
                ExpandAllTileClickAreas();
            }
        }

        [ContextMenu("Expand All Tile Click Areas")]
        public void ExpandAllTileClickAreas()
        {
            TileData[] allTiles = FindObjectsOfType<TileData>();

            if (allTiles.Length == 0)
            {
                Debug.LogWarning("[TileClickExpander] No tiles found in scene!");
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
                Debug.Log($"[TileClickExpander] Expanded click area for {expandedCount} tiles (multiplier: {clickAreaMultiplier}x)");
            }
        }

        private void ExpandTileClickArea(TileData tile)
        {
            // Check if already has click expander child
            Transform existingExpander = tile.transform.Find("ClickExpander");
            if (existingExpander != null)
            {
                // Update existing expander
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
            clickCollider.isTrigger = false; // Keep as solid for raycast

            // Copy TileData reference for raycast detection
            // The raycast will find this collider and GetComponentInParent will find TileData

            if (showDebugLogs)
            {
                Debug.Log($"[TileClickExpander] Expanded tile {tile.GridCoord} click area to {clickAreaMultiplier}x");
            }
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

            Debug.Log("[TileClickExpander] Removed all click expanders");
        }
    }
}
