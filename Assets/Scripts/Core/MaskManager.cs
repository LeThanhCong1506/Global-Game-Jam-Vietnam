// File: Scripts/Core/MaskManager.cs
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Singleton manager that controls the current mask state.
    /// Broadcasts mask changes via event for all listeners.
    /// Supports both Legacy Input and New Input System.
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
#if ENABLE_INPUT_SYSTEM
            // New Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame)
            {
                SetMask(MaskType.Off);
            }
            else if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                SetMask(MaskType.MaskA);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                SetMask(MaskType.MaskB);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                SetMask(MaskType.MaskC);
            }
#else
            // Legacy Input
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
#endif
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
