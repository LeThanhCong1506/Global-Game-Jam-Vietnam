// File: Scripts/Camera/IsometricCameraSetup.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Sets up a 2.5D isometric camera.
    /// Attach to camera and call SetupCamera() or let it auto-setup on Start.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class IsometricCameraSetup : MonoBehaviour
    {
        [Header("Isometric Settings")]
        [SerializeField] private float cameraHeight = 10f;
        [SerializeField] private float cameraDistance = 10f;
        [SerializeField] private float isometricAngle = 30f; // Typical iso angle

        [Header("Target")]
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        [Header("Options")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField] private float orthographicSize = 5f;
        [SerializeField] private bool autoSetupOnStart = true;

        private Camera cam;

        private void Start()
        {
            cam = GetComponent<Camera>();

            if (autoSetupOnStart)
            {
                SetupCamera();
            }
        }

        /// <summary>
        /// Configure camera for isometric view.
        /// </summary>
        public void SetupCamera()
        {
            if (cam == null) cam = GetComponent<Camera>();

            // Calculate position
            Vector3 targetPos = lookTarget != null ? lookTarget.position + targetOffset : targetOffset;

            // Position camera at angle
            float radAngle = isometricAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
            offset = Quaternion.Euler(0, 45, 0) * offset; // 45 degree rotation for iso look

            transform.position = targetPos + offset;
            transform.LookAt(targetPos);

            // Set projection
            cam.orthographic = useOrthographic;
            if (useOrthographic)
            {
                cam.orthographicSize = orthographicSize;
            }
        }

        /// <summary>
        /// Follow target (call in LateUpdate if dynamic following needed).
        /// </summary>
        public void FollowTarget()
        {
            if (lookTarget == null) return;

            Vector3 targetPos = lookTarget.position + targetOffset;
            float radAngle = isometricAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
            offset = Quaternion.Euler(0, 45, 0) * offset;

            transform.position = targetPos + offset;
            transform.LookAt(targetPos);
        }
    }
}
