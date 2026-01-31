// File: Scripts/Core/MaskVisibleGroup.cs
using System.Collections.Generic;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Attach to any GameObject to control visibility based on active mask.
    /// Designer sets which masks can see this object.
    /// When not visible, renderers and colliders are disabled.
    /// </summary>
    public class MaskVisibleGroup : MonoBehaviour, IMaskListener
    {
        [Header("Visibility Settings")]
        [Tooltip("List of masks under which this object is visible")]
        [SerializeField] private List<MaskType> visibleUnderMasks = new List<MaskType>();

        [Tooltip("If true, object is visible when mask is OFF (default view)")]
        [SerializeField] private bool visibleWhenMaskOff = true;

        [Header("Components to Toggle")]
        [Tooltip("If empty, will auto-find renderers in children")]
        [SerializeField] private Renderer[] renderers;

        [Tooltip("If empty, will auto-find colliders in children")]
        [SerializeField] private Collider[] colliders;

        private bool isVisible = true;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>();
            }
        }

        private void OnEnable()
        {
            MaskManager.OnMaskChanged += OnMaskChanged;

            // Apply current mask state if MaskManager exists
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
            bool shouldBeVisible = ShouldBeVisible(newMask);

            if (isVisible != shouldBeVisible)
            {
                isVisible = shouldBeVisible;
                SetVisibility(isVisible);
            }
        }

        private bool ShouldBeVisible(MaskType mask)
        {
            // Check if visible when mask is off
            if (mask == MaskType.Off)
            {
                return visibleWhenMaskOff;
            }

            // Check if current mask is in the visible list
            return visibleUnderMasks.Contains(mask);
        }

        private void SetVisibility(bool visible)
        {
            // Toggle renderers
            foreach (var rend in renderers)
            {
                if (rend != null)
                {
                    rend.enabled = visible;
                }
            }

            // Toggle colliders
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = visible;
                }
            }
        }

        /// <summary>
        /// Check if this object is currently visible.
        /// </summary>
        public bool IsCurrentlyVisible()
        {
            return isVisible;
        }
    }
}
