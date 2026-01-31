// File: Scripts/Player/PlayerGridMover.cs
using System;
using System.Collections;
using UnityEngine;

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
            if (Input.GetMouseButtonDown(0))
            {
                TileData clickedTile = GridManager.Instance?.RaycastToTile(Input.mousePosition);

                if (clickedTile != null)
                {
                    TryMoveToTile(clickedTile);
                }
            }
        }

        /// <summary>
        /// Attempt to move to clicked tile.
        /// </summary>
        public void TryMoveToTile(TileData targetTile)
        {
            if (IsMoving || inputLocked) return;

            // Check if adjacent
            if (!GridManager.Instance.AreAdjacent(currentGridCoord, targetTile.GridCoord))
            {
                HandleInvalidMove("Not adjacent");
                return;
            }

            // Check if walkable
            if (!targetTile.IsWalkable)
            {
                HandleInvalidMove("Not walkable");
                return;
            }

            // Start movement
            StartCoroutine(MoveToTileCoroutine(targetTile));
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
            transform.position = tile.transform.position + Vector3.up * 0.5f; // Slight offset above tile

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
