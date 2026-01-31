// File: Scripts/Grid/TileVisual.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Handles tile visual appearance based on current mask and tile type.
    /// Changes material/color when mask changes.
    /// </summary>
    [RequireComponent(typeof(TileData))]
    public class TileVisual : MonoBehaviour, IMaskListener
    {
        [Header("References")]
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] private TileData tileData;

        [Header("Default Materials/Colors")]
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Color defaultColor = Color.gray;

        [Header("Mask A - Walkable Highlight")]
        [SerializeField] private Color walkableHighlightColor = Color.green;

        [Header("Mask B - Trap Reveal")]
        [SerializeField] private Color trapRevealColor = Color.red;

        [Header("Mask C - Hidden Object Visibility")]
        [SerializeField] private Color hiddenTileColor = Color.cyan;

        [Header("Special Tile Colors")]
        [SerializeField] private Color exitColor = Color.yellow;
        [SerializeField] private Color startColor = Color.blue;

        private MaterialPropertyBlock propertyBlock;
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (tileRenderer == null)
            {
                tileRenderer = GetComponent<Renderer>();
            }
            if (tileData == null)
            {
                tileData = GetComponent<TileData>();
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            MaskManager.OnMaskChanged += OnMaskChanged;

            // Apply current state
            if (MaskManager.Instance != null)
            {
                OnMaskChanged(MaskManager.Instance.CurrentMask);
            }
        }

        private void OnDisable()
        {
            MaskManager.OnMaskChanged -= OnMaskChanged;
        }

        public void OnMaskChanged(MaskType newMask)
        {
            UpdateVisual(newMask);
        }

        private void UpdateVisual(MaskType mask)
        {
            Color targetColor = GetColorForMask(mask);
            ApplyColor(targetColor);
        }

        private Color GetColorForMask(MaskType mask)
        {
            // Priority: Start/Exit tiles always show their color
            if (tileData.IsStart)
            {
                return startColor;
            }
            if (tileData.IsExit)
            {
                return exitColor;
            }

            // Mask-specific coloring
            switch (mask)
            {
                case MaskType.MaskA:
                    // Highlight walkable tiles
                    if (tileData.IsWalkable)
                    {
                        return walkableHighlightColor;
                    }
                    break;

                case MaskType.MaskB:
                    // Reveal trap tiles
                    if (tileData.IsTrap)
                    {
                        return trapRevealColor;
                    }
                    break;

                case MaskType.MaskC:
                    // Show hidden tiles
                    if (tileData.IsHiddenTile)
                    {
                        return hiddenTileColor;
                    }
                    break;
            }

            return defaultColor;
        }

        private void ApplyColor(Color color)
        {
            if (tileRenderer == null) return;

            tileRenderer.GetPropertyBlock(propertyBlock);

            // Support both Standard and URP shaders
            propertyBlock.SetColor(ColorProperty, color);
            propertyBlock.SetColor(BaseColorProperty, color);

            tileRenderer.SetPropertyBlock(propertyBlock);
        }

        /// <summary>
        /// Force refresh the visual (call after tile properties change).
        /// </summary>
        public void RefreshVisual()
        {
            if (MaskManager.Instance != null)
            {
                UpdateVisual(MaskManager.Instance.CurrentMask);
            }
        }
    }
}
