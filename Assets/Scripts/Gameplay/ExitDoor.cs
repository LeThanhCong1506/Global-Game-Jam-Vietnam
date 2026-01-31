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

        private void TryExit()
        {
            bool hasKey = KeyPickup.Instance != null && KeyPickup.Instance.IsCollected;

            if (!hasKey)
            {
                // Show locked message
                Debug.Log("[ExitDoor] Door is locked! Find the key.");
                UIHudController.Instance?.ShowToast("Door is locked! Find the key.", 2f);
                SimpleAudioManager.Instance?.PlayLocked();
                return;
            }

            // Check if this exit is the correct one for current count
            // Exit #1 must be in Level1, Exit #2 in Level2, etc.
            if (!IsCorrectLevelExit())
            {
                string expected = GetExpectedLevelName();
                Debug.Log($"[ExitDoor] Wrong exit! Expected exit in {expected}");
                UIHudController.Instance?.ShowToast($"Wrong exit! Find the exit in {expected}", 2f);
                SimpleAudioManager.Instance?.PlayInvalid();
                return;
            }

            // Increment exit touch counter
            exitTouchCount++;
            Debug.Log($"[ExitDoor] Exit touched! Count: {exitTouchCount}/{requiredExitCount}");

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
            bool hasKey = KeyPickup.Instance != null && KeyPickup.Instance.IsCollected;

            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(!hasKey);
            }
            if (unlockedIndicator != null)
            {
                unlockedIndicator.SetActive(hasKey);
            }
        }
    }
}

