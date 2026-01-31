// File: Scripts/Core/MaskManager.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Singleton manager that controls the current mask state.
    /// Broadcasts mask changes via event for all listeners.
    /// </summary>
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }

        // Event fired when mask changes - subscribe to react to mask switches
        public static event Action<MaskType> OnMaskChanged;

        [Header("Current State")]
        [SerializeField] private MaskType currentMask = MaskType.Off;

        public MaskType CurrentMask => currentMask;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Broadcast initial mask state so all listeners initialize correctly
            BroadcastMaskChange();
        }

        private void Update()
        {
            HandleMaskInput();
        }

        private void HandleMaskInput()
        {
            // Press 0-3 to switch masks
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                SetMask(MaskType.Off);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SetMask(MaskType.MaskA);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SetMask(MaskType.MaskB);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SetMask(MaskType.MaskC);
            }
        }

        /// <summary>
        /// Change the current mask and notify all listeners.
        /// </summary>
        public void SetMask(MaskType newMask)
        {
            if (currentMask == newMask) return;

            currentMask = newMask;
            BroadcastMaskChange();

            Debug.Log($"[MaskManager] Mask changed to: {currentMask}");
        }

        private void BroadcastMaskChange()
        {
            OnMaskChanged?.Invoke(currentMask);
        }

        /// <summary>
        /// Check if a specific mask is currently active.
        /// </summary>
        public bool IsMaskActive(MaskType mask)
        {
            return currentMask == mask;
        }
    }
}
