// File: Scripts/Gameplay/TrapTile.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Behavior for trap tiles.
    /// On enter: resets player to start + optional time penalty.
    /// Trap is only visible under Mask B.
    /// Player dies after hitting traps more than maxTrapHits times (from level 2 onwards).
    /// </summary>
    [RequireComponent(typeof(TileData))]
    public class TrapTile : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float timePenalty = 5f;
        
        [Header("Death Settings")]
        [Tooltip("Maximum number of trap hits before player dies")]
        [SerializeField] private static int maxTrapHits = 3;
        
        [Header("Level Settings")]
        [Tooltip("If true, lives system will be disabled on the first level (build index 0)")]
        private static bool disableOnFirstLevel = true;

        [Header("Visual Settings")]
        [SerializeField] private Color trapTriggeredColor = Color.red;
        [SerializeField] private float flashDuration = 1.5f;

        [Header("References")]
        [SerializeField] private TileData tileData;
        [SerializeField] private TileVisual tileVisual;

        // Static counter shared across all trap tiles
        private static int currentTrapHits = 0;
        
        // Whether lives system is disabled for current level
        private static bool isLivesDisabled = false;

        // Event fired when trap hit count changes
        public static event Action<int, int> OnTrapHitsChanged; // current hits, max hits

        public static int CurrentTrapHits => currentTrapHits;
        public static int MaxTrapHits => maxTrapHits;
        public static bool IsLivesDisabled => isLivesDisabled;

        private void Awake()
        {
            if (tileData == null)
            {
                tileData = GetComponent<TileData>();
            }
            
            // Get TileVisual if not assigned
            if (tileVisual == null)
            {
                tileVisual = GetComponent<TileVisual>();
            }
        }

        private void Start()
        {
            // Check if this is the first level
            int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            isLivesDisabled = disableOnFirstLevel && currentLevelIndex == 0;

            if (isLivesDisabled)
            {
                Debug.Log("[TrapTile] Lives system disabled for first level");
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

        /// <summary>
        /// Flash the trap tile red using TileVisual.
        /// </summary>
        private void FlashRed()
        {
            if (tileVisual != null)
            {
                tileVisual.FlashColor(trapTriggeredColor, flashDuration);
            }
        }

        private void OnTileEntered(TileData tile)
        {
            if (!tile.IsTrap) return;

            Debug.Log($"[TrapTile] Player stepped on trap!");

            // Flash the trap tile red
            FlashRed();

            // Check if this is level 1 or 2 - lives system disabled until level 3
            int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            bool livesDisabledForThisLevel = currentLevelIndex < 2; // Level 3 = build index 2

            // If lives system is disabled (level 1-2), just reset player without counting
            if (livesDisabledForThisLevel)
            {
                Debug.Log("[TrapTile] Lives system disabled for levels 1-2 - no death penalty");
                
                // Play trap sound
                SimpleAudioManager.Instance?.PlayTrap();

                // Show UI message (no lives info on first level)
                UIHudController.Instance?.ShowToast("Trap triggered! Returned to start.", 2f);

                // Player falls and respawns at start
                if (PlayerGridMover.Instance != null)
                {
                    PlayerGridMover.Instance.FallAndRespawn(10f, 8f);
                }
                return;
            }

            // === LIVES SYSTEM ACTIVE (Level 2+) ===

            // Increment trap hit counter
            currentTrapHits++;
            Debug.Log($"[TrapTile] Trap hit! Lives used: {currentTrapHits}/{maxTrapHits}");
            
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

            // Play trap sound
            SimpleAudioManager.Instance?.PlayTrap();

            // Show UI message with remaining lives
            int remainingLives = maxTrapHits - currentTrapHits;
            UIHudController.Instance?.ShowToast($"Trap triggered! Lives remaining: {remainingLives}", 2f);

            // Player falls and respawns at start
            if (PlayerGridMover.Instance != null)
            {
                PlayerGridMover.Instance.FallAndRespawn(10f, 8f);
            }
        }
    }
}

