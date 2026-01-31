// File: Scripts/Gameplay/TrapTile.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Behavior for trap tiles.
    /// On enter: resets player to start + optional time penalty.
    /// Trap is only visible under Mask B.
    /// </summary>
    [RequireComponent(typeof(TileData))]
    public class TrapTile : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float timePenalty = 5f;

        [Header("References")]
        [SerializeField] private TileData tileData;

        private void Awake()
        {
            if (tileData == null)
            {
                tileData = GetComponent<TileData>();
            }
        }

        private void OnEnable()
        {
            if (tileData != null)
            {
                tileData.OnTileEntered += OnTileEntered;
            }
        }

        private void OnDisable()
        {
            if (tileData != null)
            {
                tileData.OnTileEntered -= OnTileEntered;
            }
        }

        private void OnTileEntered(TileData tile)
        {
            if (!tile.IsTrap) return;

            Debug.Log("[TrapTile] Player stepped on trap!");

            // Apply time penalty
            if (LevelTimer.Instance != null && timePenalty > 0)
            {
                LevelTimer.Instance.ApplyPenalty(timePenalty);
            }

            // Reset player to start
            if (PlayerGridMover.Instance != null)
            {
                PlayerGridMover.Instance.ResetToStart();
            }

            // Play trap sound
            SimpleAudioManager.Instance?.PlayTrap();

            // Show UI message
            UIHudController.Instance?.ShowToast("Trap triggered! Returned to start.", 2f);
        }
    }
}
