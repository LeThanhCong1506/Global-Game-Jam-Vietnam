// File: Scripts/Gameplay/TrapTile.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Behavior for trap tiles.
    /// On enter: resets player to start + optional time penalty.
    /// Trap is only visible under Mask B.
    /// Player dies after hitting traps more than maxTrapHits times.
    /// </summary>
    [RequireComponent(typeof(TileData))]
    public class TrapTile : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float timePenalty = 5f;
        
        [Header("Death Settings")]
        [Tooltip("Maximum number of trap hits before player dies")]
        [SerializeField] private static int maxTrapHits = 3;

        [Header("References")]
        [SerializeField] private TileData tileData;

        // Static counter shared across all trap tiles
        private static int currentTrapHits = 0;

        // Event fired when trap hit count changes
        public static event Action<int, int> OnTrapHitsChanged; // current hits, max hits

        public static int CurrentTrapHits => currentTrapHits;
        public static int MaxTrapHits => maxTrapHits;

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

        /// <summary>
        /// Reset the trap hit counter. Called at level start or restart.
        /// </summary>
        public static void ResetTrapHits()
        {
            currentTrapHits = 0;
            OnTrapHitsChanged?.Invoke(currentTrapHits, maxTrapHits);
            Debug.Log("[TrapTile] Trap hits reset to 0");
        }

        private void OnTileEntered(TileData tile)
        {
            if (!tile.IsTrap) return;

            // Increment trap hit counter
            currentTrapHits++;
            Debug.Log($"[TrapTile] Player stepped on trap! Hits: {currentTrapHits}/{maxTrapHits}");
            
            // Notify listeners about trap hit change
            OnTrapHitsChanged?.Invoke(currentTrapHits, maxTrapHits);

            // Check if player should die
            if (currentTrapHits >= maxTrapHits)
            {
                Debug.Log("[TrapTile] Maximum trap hits reached! Player dies!");
                
                // Play trap sound
                SimpleAudioManager.Instance?.PlayTrap();
                
                // Show death message
                UIHudController.Instance?.ShowToast($"Too many traps! ({currentTrapHits}/{maxTrapHits}) Game Over!", 2f);
                
                // Trigger game over
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetGameState(GameState.Lose);
                }
                return;
            }

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

            // Show UI message with remaining lives
            int remainingLives = maxTrapHits - currentTrapHits;
            UIHudController.Instance?.ShowToast($"Trap triggered! Lives remaining: {remainingLives}", 2f);
        }
    }
}
