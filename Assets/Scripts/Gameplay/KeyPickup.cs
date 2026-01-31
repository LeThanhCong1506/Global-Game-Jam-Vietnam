// File: Scripts/Gameplay/KeyPickup.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Key pickup item. Only visible under Mask C.
    /// Player must collect to unlock exit door.
    /// </summary>
    public class KeyPickup : MonoBehaviour
    {
        public static KeyPickup Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isCollected = false;

        [Header("Visual")]
        [SerializeField] private GameObject keyVisual;

        // Event when key is collected
        public static event Action OnKeyCollected;

        public bool IsCollected => isCollected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Ensure MaskVisibleGroup is set up for Mask C only
            MaskVisibleGroup mvg = GetComponent<MaskVisibleGroup>();
            if (mvg == null)
            {
                Debug.LogWarning("[KeyPickup] No MaskVisibleGroup found. Key may be always visible.");
            }
        }

        private void OnEnable()
        {
            PlayerGridMover.OnPlayerEnteredTile += OnPlayerEnteredTile;
        }

        private void OnDisable()
        {
            PlayerGridMover.OnPlayerEnteredTile -= OnPlayerEnteredTile;
        }

        private void OnPlayerEnteredTile(TileData tile)
        {
            if (isCollected) return;

            // Check if player is on this key's tile
            if (tile.IsKeySpawn || IsPlayerOnKeyTile(tile))
            {
                CollectKey();
            }
        }

        private bool IsPlayerOnKeyTile(TileData tile)
        {
            // Check if key position matches tile position
            float distance = Vector3.Distance(
                new Vector3(tile.transform.position.x, 0, tile.transform.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)
            );
            return distance < 0.5f;
        }

        private void CollectKey()
        {
            if (isCollected) return;

            isCollected = true;

            // Hide key visual
            if (keyVisual != null)
            {
                keyVisual.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

            Debug.Log("[KeyPickup] Key collected!");

            // Notify listeners
            OnKeyCollected?.Invoke();

            // Play sound
            SimpleAudioManager.Instance?.PlayPickup();

            // UI notification
            UIHudController.Instance?.ShowToast("Key collected! Find the exit.", 2f);
            UIHudController.Instance?.UpdateKeyStatus(true);
        }

        /// <summary>
        /// Reset key state (for restart).
        /// </summary>
        public void ResetKey()
        {
            isCollected = false;

            if (keyVisual != null)
            {
                keyVisual.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
