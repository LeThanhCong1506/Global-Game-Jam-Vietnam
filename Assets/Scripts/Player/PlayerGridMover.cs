// File: Scripts/Player/PlayerGridMover.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Handles player movement on the tile grid.
    /// Click adjacent tile to move. Tile-step movement only.
    /// </summary>
    public class PlayerGridMover : MonoBehaviour
    {
        public static PlayerGridMover Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float moveHeight = 0.5f; // Slight hop during movement
        [SerializeField] private float playerHeight = 1f; // Y offset above tile

        [Header("Current Position")]
        [SerializeField] private Vector2Int currentGridCoord;

        [Header("Visual Feedback")]
        [SerializeField] private float invalidClickShakeDuration = 0.2f;
        [SerializeField] private float invalidClickShakeIntensity = 0.1f;

        // Events
        public static event Action<TileData> OnPlayerEnteredTile;
        public static event Action OnInvalidMove;

        // State
        public Vector2Int CurrentGridCoord => currentGridCoord;
        public bool IsMoving { get; private set; }
        private bool inputLocked = false;
        
        // Double move ability - granted after touching exit
        private bool canMove2Tiles = false;
        public bool CanMove2Tiles => canMove2Tiles;
        
        /// <summary>
        /// Grant one-time ability to move 2 tiles. Called after touching exit.
        /// </summary>
        public void GrantDoubleMove()
        {
            canMove2Tiles = true;
            Debug.Log("[PlayerGridMover] Double move granted!");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Initialize position to start tile if GridManager is ready
            if (GridManager.Instance != null && GridManager.Instance.StartTile != null)
            {
                TeleportToTile(GridManager.Instance.StartTile);
            }
        }

        private void Update()
        {
            if (inputLocked || IsMoving) return;

            HandleClickInput();
        }

        private void HandleClickInput()
        {
            // New Input System
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                TileData clickedTile = GridManager.Instance?.RaycastToTile(mousePos);

                if (clickedTile != null)
                {
                    Debug.Log($"[PlayerGridMover] Clicked tile: {clickedTile.GridCoord}, Walkable: {clickedTile.IsWalkable}");
                    TryMoveToTile(clickedTile);
                }
                else
                {
                    Debug.Log("[PlayerGridMover] Clicked but no tile found by raycast");
                }
            }
        }

        /// <summary>
        /// Calculate Manhattan distance between two grid coordinates.
        /// </summary>
        private int GetManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Attempt to move to clicked tile.
        /// Player can only move 1 tile at a time (adjacent tiles by position).
        /// </summary>
        public void TryMoveToTile(TileData targetTile)
        {
            if (IsMoving || inputLocked) return;

            // Get current tile by finding tile closest to player position
            TileData currentTile = GetCurrentTile();

            // Ignore click on same tile
            if (targetTile == currentTile)
            {
                Debug.Log("[PlayerGridMover] Clicked on current tile, ignoring");
                return;
            }

            // Check if adjacent by world position (1.5f tolerance for touching tiles)
            bool isAdjacent = GridManager.Instance.AreTilesAdjacentByPosition(currentTile, targetTile, 1.5f);
            
            if (!isAdjacent)
            {
                HandleInvalidMove("Move 1 tile at a time");
                return;
            }

            // Check if walkable
            if (!targetTile.IsWalkable)
            {
                HandleInvalidMove("Not walkable");
                return;
            }

            // Update current grid coord for tracking
            currentGridCoord = targetTile.GridCoord;

            // Start movement
            StartCoroutine(MoveToTileCoroutine(targetTile));
        }

        /// <summary>
        /// Get the tile the player is currently standing on based on position.
        /// </summary>
        private TileData GetCurrentTile()
        {
            TileData[] allTiles = FindObjectsOfType<TileData>();
            TileData closest = null;
            float closestDist = float.MaxValue;

            foreach (var tile in allTiles)
            {
                float dist = Vector3.Distance(transform.position, tile.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = tile;
                }
            }
            return closest;
        }

        private IEnumerator MoveToTileCoroutine(TileData targetTile)
        {
            IsMoving = true;

            Vector3 startPos = transform.position;
            Vector3 endPos = targetTile.transform.position;

            // Keep same Y height as player, or use tile height
            endPos.y = startPos.y;

            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Interpolate position with slight arc
                Vector3 pos = Vector3.Lerp(startPos, endPos, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * moveHeight;

                transform.position = pos;

                yield return null;
            }

            // Snap to final position
            transform.position = endPos;

            // Update grid coordinate
            currentGridCoord = targetTile.GridCoord;

            IsMoving = false;

            // Notify that player entered this tile
            targetTile.NotifyTileEntered();
            OnPlayerEnteredTile?.Invoke(targetTile);

            SimpleAudioManager.Instance?.PlayMove();
        }

        private void HandleInvalidMove(string reason)
        {
            Debug.Log($"[PlayerGridMover] Invalid move: {reason}");
            OnInvalidMove?.Invoke();

            // Optional: shake feedback
            StartCoroutine(ShakeCoroutine());

            SimpleAudioManager.Instance?.PlayInvalid();
        }

        private IEnumerator ShakeCoroutine()
        {
            Vector3 originalPos = transform.position;
            float elapsed = 0f;

            while (elapsed < invalidClickShakeDuration)
            {
                elapsed += Time.deltaTime;

                float offsetX = UnityEngine.Random.Range(-1f, 1f) * invalidClickShakeIntensity;
                float offsetZ = UnityEngine.Random.Range(-1f, 1f) * invalidClickShakeIntensity;

                transform.position = originalPos + new Vector3(offsetX, 0, offsetZ);

                yield return null;
            }

            transform.position = originalPos;
        }

        /// <summary>
        /// Instantly teleport player to a tile (used for respawn).
        /// </summary>
        public void TeleportToTile(TileData tile)
        {
            currentGridCoord = tile.GridCoord;
            transform.position = tile.transform.position + Vector3.up * playerHeight; // Offset above tile

            Debug.Log($"[PlayerGridMover] Teleported to {tile.GridCoord}");
        }

        /// <summary>
        /// Reset player to start tile.
        /// </summary>
        public void ResetToStart()
        {
            if (GridManager.Instance?.StartTile != null)
            {
                TeleportToTile(GridManager.Instance.StartTile);
            }
        }

        /// <summary>
        /// Make player fall down and then respawn at start position.
        /// Used when player triggers a trap.
        /// </summary>
        public void FallAndRespawn(float fallDistance = 10f, float fallSpeed = 8f)
        {
            // Use global start tile
            TileData respawnTile = GridManager.Instance?.StartTile;
            StartCoroutine(FallAndRespawnCoroutine(fallDistance, fallSpeed, respawnTile));
        }
        
        /// <summary>
        /// Make player fall down and then respawn at a specific tile.
        /// Used for level-specific trap respawn.
        /// </summary>
        public void FallAndRespawn(TileData respawnTile, float fallDistance = 10f, float fallSpeed = 8f)
        {
            StartCoroutine(FallAndRespawnCoroutine(fallDistance, fallSpeed, respawnTile));
        }

        private IEnumerator FallAndRespawnCoroutine(float fallDistance, float fallSpeed, TileData respawnTile)
        {
            // Lock input during fall
            inputLocked = true;
            IsMoving = true;

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos - Vector3.up * fallDistance;
            float duration = fallDistance / fallSpeed;
            float elapsed = 0f;

            // Fall down with acceleration (easing in)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Ease in - starts slow, accelerates (simulates gravity)
                float easedT = t * t;
                
                transform.position = Vector3.Lerp(startPos, endPos, easedT);
                yield return null;
            }

            transform.position = endPos;

            // Small delay at the bottom
            yield return new WaitForSeconds(0.2f);

            // Respawn at specified tile or fallback to global start
            TileData targetTile = respawnTile ?? GridManager.Instance?.StartTile;
            if (targetTile != null)
            {
                TeleportToTile(targetTile);
            }

            // Unlock input
            inputLocked = false;
            IsMoving = false;
        }

        /// <summary>
        /// Lock/unlock input (used during island rotation, cutscenes, etc).
        /// </summary>
        public void SetInputLocked(bool locked)
        {
            inputLocked = locked;
        }

        public bool IsInputLocked()
        {
            return inputLocked;
        }
    }
}

