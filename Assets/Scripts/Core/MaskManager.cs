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
    /// 
    /// Mask Rules:
    /// - Mask A: Can be used 2 times, each time allows 10 steps, max 2 seconds per use
    /// - Mask B: Can be used 1 time only, max 2 seconds per use
    /// - Mask C: Drains timer faster while active (handled in LevelTimer), max 2 seconds per use
    /// - All masks have a maximum duration of 2 seconds per activation
    /// </summary>
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }

        // Event fired when mask changes - subscribe to react to mask switches
        public static event Action<MaskType> OnMaskChanged;
        // Event fired when mask usage is updated
        public static event Action<MaskType, int, int> OnMaskUsageUpdated; // mask, remaining uses, remaining steps
        // Event fired when mask time is updated
        public static event Action<float, float> OnMaskTimeUpdated; // remaining time, max time

        [Header("Current State")]
        [SerializeField] private MaskType currentMask = MaskType.Off;

        [Header("Time Limit Settings")]
        [Tooltip("Maximum time (in seconds) a mask can be active per use")]
        [SerializeField] private float maxMaskDuration = 2f;

        [Header("Mask A Settings")]
        [Tooltip("How many times Mask A can be activated")]
        [SerializeField] private int maskAMaxUses = 2;
        [Tooltip("How many steps allowed per Mask A activation")]
        [SerializeField] private int maskAStepsPerUse = 10;
        
        [Header("Mask B Settings")]
        [Tooltip("How many times Mask B can be activated")]
        [SerializeField] private int maskBMaxUses = 1;

        // Runtime state for Mask A
        private int maskAUsesRemaining;
        private int maskAStepsRemaining;
        
        // Runtime state for Mask B
        private int maskBUsesRemaining;

        // Runtime state for mask time limit
        private float maskTimeRemaining;
        private bool isMaskTimerActive;

        public MaskType CurrentMask => currentMask;
        public int MaskAUsesRemaining => maskAUsesRemaining;
        public int MaskAStepsRemaining => maskAStepsRemaining;
        public int MaskBUsesRemaining => maskBUsesRemaining;
        public float MaskTimeRemaining => maskTimeRemaining;
        public float MaxMaskDuration => maxMaskDuration;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize mask usage counts
            ResetMaskUsage();
        }

        private void Start()
        {
            // Broadcast initial mask state so all listeners initialize correctly
            BroadcastMaskChange();
        }

        private void OnEnable()
        {
            // Subscribe to player movement to track steps
            PlayerGridMover.OnPlayerEnteredTile += OnPlayerStep;
        }

        private void OnDisable()
        {
            PlayerGridMover.OnPlayerEnteredTile -= OnPlayerStep;
        }

        /// <summary>
        /// Called when player takes a step (enters a tile)
        /// </summary>
        private void OnPlayerStep(TileData tile)
        {
            // Only track steps for Mask A
            if (currentMask == MaskType.MaskA && maskAStepsRemaining > 0)
            {
                maskAStepsRemaining--;
                Debug.Log($"[MaskManager] Mask A step used. Remaining: {maskAStepsRemaining}");
                
                OnMaskUsageUpdated?.Invoke(MaskType.MaskA, maskAUsesRemaining, maskAStepsRemaining);

                // If no steps remaining, turn off mask
                if (maskAStepsRemaining <= 0)
                {
                    Debug.Log("[MaskManager] Mask A steps exhausted, turning off.");
                    SetMask(MaskType.Off);
                }
            }
        }

        private void Update()
        {
            HandleMaskInput();
            UpdateMaskTimer();
        }

        /// <summary>
        /// Update the mask duration timer
        /// </summary>
        private void UpdateMaskTimer()
        {
            if (!isMaskTimerActive || currentMask == MaskType.Off)
                return;

            maskTimeRemaining -= Time.deltaTime;
            OnMaskTimeUpdated?.Invoke(maskTimeRemaining, maxMaskDuration);

            if (maskTimeRemaining <= 0)
            {
                Debug.Log($"[MaskManager] Mask {currentMask} time expired (2 seconds limit reached), turning off.");
                SetMask(MaskType.Off);
            }
        }

        /// <summary>
        /// Start the mask timer when a mask is activated
        /// </summary>
        private void StartMaskTimer()
        {
            maskTimeRemaining = maxMaskDuration;
            isMaskTimerActive = true;
            OnMaskTimeUpdated?.Invoke(maskTimeRemaining, maxMaskDuration);
            Debug.Log($"[MaskManager] Mask timer started: {maxMaskDuration} seconds");
        }

        /// <summary>
        /// Stop the mask timer
        /// </summary>
        private void StopMaskTimer()
        {
            isMaskTimerActive = false;
            maskTimeRemaining = 0;
            OnMaskTimeUpdated?.Invoke(0, maxMaskDuration);
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
                TryActivateMaskA();
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                TryActivateMaskB();
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                TryActivateMaskC();
            }
#else
            // Legacy Input
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                SetMask(MaskType.Off);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                TryActivateMaskA();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                TryActivateMaskB();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                TryActivateMaskC();
            }
#endif
        }

        /// <summary>
        /// Try to activate Mask A. Only succeeds if uses remain.
        /// </summary>
        private void TryActivateMaskA()
        {
            // If already using Mask A, allow toggling off
            if (currentMask == MaskType.MaskA)
            {
                SetMask(MaskType.Off);
                return;
            }

            // Check if uses remain
            if (maskAUsesRemaining <= 0)
            {
                Debug.Log("[MaskManager] Mask A has no uses remaining!");
                SimpleAudioManager.Instance?.PlayInvalid();
                return;
            }

            // Consume one use and reset steps
            maskAUsesRemaining--;
            maskAStepsRemaining = maskAStepsPerUse;
            
            Debug.Log($"[MaskManager] Mask A activated. Uses remaining: {maskAUsesRemaining}, Steps: {maskAStepsRemaining}");
            OnMaskUsageUpdated?.Invoke(MaskType.MaskA, maskAUsesRemaining, maskAStepsRemaining);
            
            SetMask(MaskType.MaskA);
        }

        /// <summary>
        /// Try to activate Mask B. Only succeeds if uses remain.
        /// </summary>
        private void TryActivateMaskB()
        {
            // If already using Mask B, allow toggling off
            if (currentMask == MaskType.MaskB)
            {
                SetMask(MaskType.Off);
                return;
            }

            // Check if uses remain
            if (maskBUsesRemaining <= 0)
            {
                Debug.Log("[MaskManager] Mask B has no uses remaining!");
                SimpleAudioManager.Instance?.PlayInvalid();
                return;
            }

            // Consume the one use
            maskBUsesRemaining--;
            
            Debug.Log($"[MaskManager] Mask B activated. Uses remaining: {maskBUsesRemaining}");
            OnMaskUsageUpdated?.Invoke(MaskType.MaskB, maskBUsesRemaining, 0);
            
            SetMask(MaskType.MaskB);
        }

        /// <summary>
        /// Try to activate Mask C.
        /// </summary>
        private void TryActivateMaskC()
        {
            // If already using Mask C, allow toggling off
            if (currentMask == MaskType.MaskC)
            {
                SetMask(MaskType.Off);
                return;
            }

            SetMask(MaskType.MaskC);
        }

        /// <summary>
        /// Change the current mask and notify all listeners.
        /// </summary>
        public void SetMask(MaskType newMask)
        {
            if (currentMask == newMask) return;

            // Stop timer when turning off mask
            if (newMask == MaskType.Off)
            {
                StopMaskTimer();
            }
            else
            {
                // Start timer when activating any mask
                StartMaskTimer();
            }

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

        /// <summary>
        /// Reset all mask usage counts to initial values.
        /// Called at level start or reset.
        /// </summary>
        public void ResetMaskUsage()
        {
            maskAUsesRemaining = maskAMaxUses;
            maskAStepsRemaining = 0;
            maskBUsesRemaining = maskBMaxUses;
            maskTimeRemaining = 0;
            isMaskTimerActive = false;
            
            Debug.Log($"[MaskManager] Mask usage reset. A: {maskAUsesRemaining} uses, B: {maskBUsesRemaining} uses");
        }

        /// <summary>
        /// Check if a mask can be activated.
        /// </summary>
        public bool CanActivateMask(MaskType mask)
        {
            switch (mask)
            {
                case MaskType.MaskA:
                    return maskAUsesRemaining > 0;
                case MaskType.MaskB:
                    return maskBUsesRemaining > 0;
                case MaskType.MaskC:
                case MaskType.Off:
                    return true;
                default:
                    return false;
            }
        }
    }
}
