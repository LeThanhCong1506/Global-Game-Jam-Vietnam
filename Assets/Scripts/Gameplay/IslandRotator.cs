// File: Scripts/Gameplay/IslandRotator.cs
using System;
using System.Collections;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Island rotation mechanic (skeleton for future expansion).
    /// Click to rotate 90 degrees. Locks input during rotation.
    /// Does NOT rebuild grid in this base version - ready for future extension.
    /// </summary>
    public class IslandRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float rotationDuration = 0.5f;
        [SerializeField] private bool rotateClockwise = true;

        [Header("State")]
        [SerializeField] private bool isRotating = false;

        // Event fired when rotation completes
        public event Action OnIslandRotated;

        // Static event for any island rotation (useful for global listeners)
        public static event Action<IslandRotator> OnAnyIslandRotated;

        public bool IsRotating => isRotating;

        private void OnMouseDown()
        {
            // Check if we're the clicked object
            TryRotate();
        }

        /// <summary>
        /// Attempt to start rotation.
        /// </summary>
        public void TryRotate()
        {
            if (isRotating) return;

            // Check if player input is already locked
            if (PlayerGridMover.Instance != null && PlayerGridMover.Instance.IsInputLocked())
            {
                Debug.Log("[IslandRotator] Cannot rotate - input locked.");
                return;
            }

            StartCoroutine(RotateCoroutine());
        }

        private IEnumerator RotateCoroutine()
        {
            isRotating = true;

            // Lock player input during rotation
            PlayerGridMover.Instance?.SetInputLocked(true);

            Quaternion startRotation = transform.rotation;
            float targetYAngle = rotateClockwise ? 90f : -90f;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, targetYAngle, 0);

            float elapsed = 0f;

            // Tween rotation
            #if DOTWEEN
            // Optional DOTween implementation
            transform.DORotateQuaternion(endRotation, rotationDuration).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(rotationDuration);
            #else
            // Default coroutine tween
            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);

                // Smooth step for easing
                t = t * t * (3f - 2f * t);

                transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            // Snap to exact rotation
            transform.rotation = endRotation;
            #endif

            SimpleAudioManager.Instance?.PlayRotate();

            // Unlock player input
            PlayerGridMover.Instance?.SetInputLocked(false);

            isRotating = false;

            Debug.Log("[IslandRotator] Rotation complete.");

            // Fire events
            OnIslandRotated?.Invoke();
            OnAnyIslandRotated?.Invoke(this);

            // Future: Rebuild grid here when adding tile reconnection logic
            // GridManager.Instance?.RebuildGrid();
        }

        /// <summary>
        /// Force immediate rotation (no animation).
        /// </summary>
        public void RotateImmediate()
        {
            float angle = rotateClockwise ? 90f : -90f;
            transform.Rotate(0, angle, 0);

            OnIslandRotated?.Invoke();
            OnAnyIslandRotated?.Invoke(this);
        }
    }
}
