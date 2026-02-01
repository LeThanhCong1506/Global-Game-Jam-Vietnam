// File: Scripts/Gameplay/KeyPickup.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Key pickup item. Only visible under Mask C.
    /// Player must collect to unlock exit door.
    /// Supports multiple keys in the scene.
    /// </summary>
    public class KeyPickup : MonoBehaviour
    {
        // Removed Singleton - multiple keys can exist in scene
        // Use FindObjectsOfType<KeyPickup>() to find all keys

        [Header("State")]
        [SerializeField] private bool isCollected = false;

        [Header("Visual")]
        [SerializeField] private GameObject keyVisual;

        // Event when any key is collected
        public static event Action OnKeyCollected;

        public bool IsCollected => isCollected;

        private void Awake()
        {
            // Ensure MaskVisibleGroup is set up for Mask C only
            MaskVisibleGroup mvg = GetComponent<MaskVisibleGroup>();
            if (mvg == null)
            {
                Debug.LogWarning($"[KeyPickup] {gameObject.name}: No MaskVisibleGroup found. Key may be always visible.");
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

            // Only collect THIS key when player is at THIS key's exact position
            // Each key checks independently if player is on its tile
            if (IsPlayerOnThisKeyTile(tile))
            {
                CollectKey();
            }
        }

        /// <summary>
        /// Check if the player's current tile matches THIS key's position.
        /// Each key only checks its own position.
        /// </summary>
        private bool IsPlayerOnThisKeyTile(TileData tile)
        {
            // Compare player's tile position with this key's position
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

            // Use MaskVisibleGroup to permanently hide this key
            // This prevents the key from reappearing when mask changes
            MaskVisibleGroup mvg = GetComponent<MaskVisibleGroup>();
            if (mvg != null)
            {
                mvg.SetPermanentlyHidden();
            }
            else
            {
                // Fallback: Hide key visual - only disable renderers, NOT the GameObject
                if (keyVisual != null)
                {
                    keyVisual.SetActive(false);
                }
                else
                {
                    // Disable all renderers on this object
                    Renderer[] rends = GetComponentsInChildren<Renderer>();
                    foreach (var rend in rends)
                    {
                        rend.enabled = false;
                    }
                    
                    // Also disable collider so player can't interact again
                    Collider[] cols = GetComponentsInChildren<Collider>();
                    foreach (var col in cols)
                    {
                        col.enabled = false;
                    }
                }
            }

            Debug.Log("[KeyPickup] Key collected!");

            // Notify ExitDoor about collected key
            ExitDoor.AddCollectedKey();

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

            // Use MaskVisibleGroup to reset visibility
            MaskVisibleGroup mvg = GetComponent<MaskVisibleGroup>();
            if (mvg != null)
            {
                mvg.ResetPermanentlyHidden();
            }
            else
            {
                // Fallback: Re-enable manually
                if (keyVisual != null)
                {
                    keyVisual.SetActive(true);
                }
                else
                {
                    // Re-enable all renderers
                    Renderer[] rends = GetComponentsInChildren<Renderer>(true);
                    foreach (var rend in rends)
                    {
                        rend.enabled = true;
                    }
                    
                    // Re-enable all colliders
                    Collider[] cols = GetComponentsInChildren<Collider>(true);
                    foreach (var col in cols)
                    {
                        col.enabled = true;
                    }
                }
            }
        }
    }
}
