// File: Scripts/Gameplay/ExitDoor.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Exit door behavior.
    /// If key not collected: shows "Locked" message.
    /// If key collected: triggers win.
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileData exitTileData;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject lockedIndicator;
        [SerializeField] private GameObject unlockedIndicator;

        // Event when player wins
        public static event Action OnPlayerExited;

        private void Awake()
        {
            if (exitTileData == null)
            {
                exitTileData = GetComponent<TileData>();
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
            // Check if player entered any exit tile
            if (!tile.IsExit) return;

            TryExit();
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

            // Player has key - trigger win!
            Debug.Log("[ExitDoor] Player exited with key - WIN!");
            OnPlayerExited?.Invoke();

            SimpleAudioManager.Instance?.PlayWin();

            // Notify GameManager
            GameManager.Instance?.SetGameState(GameState.Win);
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
