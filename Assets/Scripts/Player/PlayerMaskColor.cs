// File: Scripts/Player/PlayerMaskColor.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Changes player mesh color based on active mask.
    /// Mask A = Green, Mask B = Red, Mask C = Yellow.
    /// </summary>
    public class PlayerMaskColor : MonoBehaviour
    {
        [Header("Player Reference")]
        [Tooltip("Reference to the player GameObject (will get MeshRenderer from it)")]
        [SerializeField] private GameObject playerObject;
        
        [Header("Colors")]
        [SerializeField] private Color maskAColor = Color.green;   // Mask A = Green
        [SerializeField] private Color maskBColor = Color.red;     // Mask B = Red
        [SerializeField] private Color maskCColor = Color.yellow;  // Mask C = Yellow
        [SerializeField] private Color noMaskColor = Color.white;  // No mask = White
        
        private MeshRenderer playerMeshRenderer;
        private Material playerMaterial;

        private void Start()
        {
            // Get MeshRenderer from player
            if (playerObject != null)
            {
                playerMeshRenderer = playerObject.GetComponentInChildren<MeshRenderer>();
                if (playerMeshRenderer != null)
                {
                    // Create material instance to avoid modifying shared material
                    playerMaterial = playerMeshRenderer.material;
                    Debug.Log("[PlayerMaskColor] Found player MeshRenderer");
                }
                else
                {
                    Debug.LogWarning("[PlayerMaskColor] No MeshRenderer found on player!");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerMaskColor] Player object not assigned!");
            }
        }

        private void OnEnable()
        {
            MaskManager.OnMaskChanged += OnMaskChanged;
        }

        private void OnDisable()
        {
            MaskManager.OnMaskChanged -= OnMaskChanged;
        }

        private void OnMaskChanged(MaskType newMask)
        {
            if (playerMaterial == null) return;

            Color targetColor = GetColorForMask(newMask);
            playerMaterial.color = targetColor;
            
            Debug.Log($"[PlayerMaskColor] Changed to {newMask} -> {targetColor}");
        }

        private Color GetColorForMask(MaskType mask)
        {
            switch (mask)
            {
                case MaskType.MaskA: return maskAColor;  // Green
                case MaskType.MaskB: return maskBColor;  // Red
                case MaskType.MaskC: return maskCColor;  // Yellow
                case MaskType.Off:
                default: return noMaskColor;             // White
            }
        }

        /// <summary>
        /// Manually set player reference at runtime.
        /// </summary>
        public void SetPlayer(GameObject player)
        {
            playerObject = player;
            if (playerObject != null)
            {
                playerMeshRenderer = playerObject.GetComponentInChildren<MeshRenderer>();
                if (playerMeshRenderer != null)
                {
                    playerMaterial = playerMeshRenderer.material;
                }
            }
        }
    }
}
