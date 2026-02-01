// File: Scripts/Gameplay/ExitDoor.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Exit door behavior.
    /// If key not collected: shows "Locked" message.
    /// If key collected: triggers level animation and win (only on final level).
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileData exitTileData;

        [Header("Level Settings")]
        [Tooltip("Name of the final level object that triggers Win panel (e.g., 'Level4')")]
        [SerializeField] private string finalLevelName = "Level4";
        
        [Tooltip("Reference to the level GameObject (for animation)")]
        [SerializeField] private GameObject levelObject;
        
        [Tooltip("Animator component to trigger when exit is touched")]
        [SerializeField] private Animator levelAnimator;
        
        [Tooltip("Animator bool parameter name to set true (e.g., 'lev2', 'lev3', 'lev4')")]
        [SerializeField] private string animatorTriggerParam = "";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject unlockedIndicator;

        // Event when player wins
        public static event Action OnPlayerExited;
        // Event when player completes a level (but not final)
        public static event Action OnLevelCompleted;

        private void Awake()
        {
            if (exitTileData == null)
            {
                exitTileData = GetComponent<TileData>();
            }
            
            // Try to get animator from level object if not assigned
            if (levelAnimator == null && levelObject != null)
            {
                levelAnimator = levelObject.GetComponent<Animator>();
            }
        }

        private void Start()
        {
            UpdateVisual();
        }

        private void OnEnable()
        {
            PlayerGridMover.OnPlayerEnteredTile += OnPlayerEnteredTile;
            KeyPickup.OnKeyCollected += OnKeyCollected;
        }

        private void OnDisable()
        {
            PlayerGridMover.OnPlayerEnteredTile -= OnPlayerEnteredTile;
            KeyPickup.OnKeyCollected -= OnKeyCollected;
        }

        private void OnKeyCollected()
        {
            UpdateVisual();
        }

        private void OnPlayerEnteredTile(TileData tile)
        {
            // IMPORTANT: Only trigger if player entered THIS specific exit tile
            // Not any exit tile in the scene
            if (exitTileData == null || tile != exitTileData) return;
            if (!tile.IsExit) return;

            TryExit();
        }

        // Static counter for exit touches (shared across all ExitDoor instances)
        private static int exitTouchCount = 0;
        
        [Header("Win Condition")]
        [Tooltip("Number of exits player must touch before showing Win panel")]
        [SerializeField] private int requiredExitCount = 4;

        /// <summary>
        /// Reset the exit touch counter (call at level start/restart)
        /// </summary>
        public static void ResetExitCount()
        {
            exitTouchCount = 0;
            Debug.Log("[ExitDoor] Exit touch count reset to 0");
        }

        /// <summary>
        /// Get the expected level name based on exit touch count.
        /// Exit #1 → Level1, Exit #2 → Level2, etc.
        /// </summary>
        private string GetExpectedLevelName()
        {
            return $"Level{exitTouchCount + 1}";
        }

        /// <summary>
        /// Check if this exit is a child of the expected level.
        /// </summary>
        private bool IsCorrectLevelExit()
        {
            string expectedLevel = GetExpectedLevelName();
            
            // Check if levelObject matches expected level
            if (levelObject != null && levelObject.name == expectedLevel)
            {
                return true;
            }
            
            // Check if this exit is a child of the expected level
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.name == expectedLevel)
                {
                    return true;
                }
                parent = parent.parent;
            }
            
            return false;
        }

        // Static counter for total keys collected (shared across all levels)
        private static int totalKeysCollected = 0;

        /// <summary>
        /// Reset the key counter (call at game start/restart)
        /// </summary>
        public static void ResetKeyCount()
        {
            totalKeysCollected = 0;
            Debug.Log("[ExitDoor] Key count reset to 0");
        }

        /// <summary>
        /// Increment the key count when player collects a key.
        /// Called from KeyPickup.
        /// </summary>
        public static void AddCollectedKey()
        {
            totalKeysCollected++;
            Debug.Log($"[ExitDoor] Key collected! Total: {totalKeysCollected}");
        }

        /// <summary>
        /// Get required keys for each exit.
        /// Exit 1: 0 keys (free pass)
        /// Exit 2: 0 keys (free pass)
        /// Exit 3: 1 key required
        /// Exit 4: 2 keys required
        /// </summary>
        private int GetRequiredKeysForExit(int exitNumber)
        {
            switch (exitNumber)
            {
                case 1: return 0;  // Lần 1: không cần key
                case 2: return 0;  // Lần 2: không cần key
                case 3: return 1;  // Lần 3: cần 1 key
                case 4: return 2;  // Lần 4: cần 2 key
                default: return 0;
            }
        }

        /// <summary>
        /// Check if player has enough keys for current exit.
        /// </summary>
        private bool HasEnoughKeys()
        {
            int exitNumber = exitTouchCount + 1;
            int requiredKeys = GetRequiredKeysForExit(exitNumber);
            
            Debug.Log($"[ExitDoor] Exit #{exitNumber}: Need {requiredKeys} keys, have {totalKeysCollected}");
            return totalKeysCollected >= requiredKeys;
        }

        /// <summary>
        /// Get the parent level object (Level1, Level2, etc.)
        /// </summary>
        private GameObject GetParentLevelObject()
        {
            if (levelObject != null) return levelObject;

            // Search parent hierarchy for LevelX
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.name.StartsWith("Level"))
                {
                    return parent.gameObject;
                }
                parent = parent.parent;
            }
            return null;
        }

        private void TryExit()
        {
            // Get current exit number (before incrementing)
            int exitNumber = exitTouchCount + 1;
            int requiredKeys = GetRequiredKeysForExit(exitNumber);

            Debug.Log($"[ExitDoor] Trying Exit #{exitNumber}: Need {requiredKeys} keys, have {totalKeysCollected}");

            // Check if enough keys collected
            if (totalKeysCollected < requiredKeys)
            {
                Debug.Log($"[ExitDoor] Exit #{exitNumber}: Not enough keys!");
                int keysNeeded = requiredKeys - totalKeysCollected;
                UIHudController.Instance?.ShowToast($"Cần thêm {keysNeeded} chìa khóa!", 2f);
                SimpleAudioManager.Instance?.PlayLocked();
                return;
            }

            // SUCCESS - Increment exit touch counter
            exitTouchCount++;
            Debug.Log($"[ExitDoor] Exit #{exitNumber} passed! Total: {exitTouchCount}/{requiredExitCount}");

            // Play level animation
            PlayLevelAnimation();

            // Play win sound on every exit touch
            SimpleAudioManager.Instance?.PlayWin();

            // Check if this is the final exit (4th touch)
            if (exitTouchCount >= requiredExitCount)
            {
                // 4th exit - trigger WIN!
                Debug.Log($"[ExitDoor] Player reached exit #{exitTouchCount} - WIN!");
                OnPlayerExited?.Invoke();
                GameManager.Instance?.SetGameState(GameState.Win);
            }
            else
            {
                // Not final exit - continue to next area
                Debug.Log($"[ExitDoor] Exit #{exitTouchCount} complete! Moving to next area...");
                OnLevelCompleted?.Invoke();
                UIHudController.Instance?.ShowToast($"Area {exitTouchCount} Complete!", 2f);
                
                // Grant double move ability (move 2 tiles once)
                PlayerGridMover.Instance?.GrantDoubleMove();
            }
        }

        /// <summary>
        /// Check if current exit is on the final level
        /// </summary>
        private bool IsFinalLevel()
        {
            // Check by level object name
            if (levelObject != null && levelObject.name == finalLevelName)
            {
                return true;
            }
            
            // Check by this object's parent name
            if (transform.parent != null && transform.parent.name == finalLevelName)
            {
                return true;
            }
            
            // Check by this object's name
            if (gameObject.name.Contains(finalLevelName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Trigger the level animation when exit is touched
        /// </summary>
        private void PlayLevelAnimation()
        {
            if (levelAnimator == null)
            {
                Debug.Log("[ExitDoor] No level animator assigned");
                return;
            }

            if (!string.IsNullOrEmpty(animatorTriggerParam))
            {
                levelAnimator.SetBool(animatorTriggerParam, true);
                Debug.Log($"[ExitDoor] Set animator param '{animatorTriggerParam}' = true");
            }
            else
            {
                Debug.LogWarning("[ExitDoor] No animator trigger parameter set!");
            }
        }

        private System.Collections.IEnumerator LoadNextLevelAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameManager.Instance?.LoadNextLevel();
        }

        private void UpdateVisual()
        {
            bool hasEnough = HasEnoughKeys();

            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(!hasEnough);
            }
            if (unlockedIndicator != null)
            {
                unlockedIndicator.SetActive(hasEnough);
            }
        }
    }
}

