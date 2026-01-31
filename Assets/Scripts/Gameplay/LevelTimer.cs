// File: Scripts/Gameplay/LevelTimer.cs
using System;
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Level timer that counts down.
    /// Mask C active = extra drain per second.
    /// Time up = lose.
    /// </summary>
    public class LevelTimer : MonoBehaviour
    {
        public static LevelTimer Instance { get; private set; }

        [Header("Timer Settings")]
        [SerializeField] private float startTime = 360f; // 6 minutes
        [SerializeField] private float maskCExtraDrainPerSecond = 1f;
        
        [Header("Enable/Disable")]
        [Tooltip("Completely disable the countdown timer")]
        [SerializeField] private bool timerDisabled = true; // OFF by default
        
        [Header("Level Settings")]
        [Tooltip("If true, timer will be disabled on the first level (build index 0)")]
        [SerializeField] private bool disableOnFirstLevel = true;

        [Header("State")]
        [SerializeField] private float currentTime;
        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool isDisabledForLevel = false;

        // Events
        public static event Action<float> OnTimeUpdated; // Current time
        public static event Action OnTimeUp;

        public float CurrentTime => currentTime;
        public float StartTime => startTime;
        public bool IsRunning => isRunning;
        public bool IsDisabledForLevel => isDisabledForLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentTime = startTime;
        }

        private void Start()
        {
            // Check if timer is completely disabled
            if (timerDisabled)
            {
                isDisabledForLevel = true;
                Debug.Log("[LevelTimer] Timer is disabled");
                return;
            }

            // Check if this is the first level
            int currentLevelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            isDisabledForLevel = disableOnFirstLevel && currentLevelIndex == 0;

            if (isDisabledForLevel)
            {
                Debug.Log("[LevelTimer] Timer disabled for first level");
                return;
            }

            // Start timer when game starts
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
            {
                StartTimer();
            }
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.Playing)
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }
        }

        private void Update()
        {
            if (!isRunning || isDisabledForLevel) return;

            // Base drain
            float drain = Time.deltaTime;

            // Extra drain if Mask C is active
            if (MaskManager.Instance != null && MaskManager.Instance.CurrentMask == MaskType.MaskC)
            {
                drain += maskCExtraDrainPerSecond * Time.deltaTime;
            }

            currentTime -= drain;
            currentTime = Mathf.Max(0, currentTime);

            OnTimeUpdated?.Invoke(currentTime);

            // Check for time up
            if (currentTime <= 0)
            {
                TimeUp();
            }
        }

        private void TimeUp()
        {
            isRunning = false;
            Debug.Log("[LevelTimer] Time's up!");

            OnTimeUp?.Invoke();
            SimpleAudioManager.Instance?.PlayLose();

            // Notify GameManager
            GameManager.Instance?.SetGameState(GameState.Lose);
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        public void StartTimer()
        {
            isRunning = true;
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void StopTimer()
        {
            isRunning = false;
        }

        /// <summary>
        /// Reset timer to start value.
        /// </summary>
        public void ResetTimer()
        {
            currentTime = startTime;
            OnTimeUpdated?.Invoke(currentTime);
        }

        /// <summary>
        /// Apply time penalty (e.g., from trap).
        /// </summary>
        public void ApplyPenalty(float seconds)
        {
            currentTime -= seconds;
            currentTime = Mathf.Max(0, currentTime);

            Debug.Log($"[LevelTimer] Penalty applied: -{seconds}s. Time left: {currentTime:F1}s");

            OnTimeUpdated?.Invoke(currentTime);
        }

        /// <summary>
        /// Add bonus time.
        /// </summary>
        public void AddTime(float seconds)
        {
            currentTime += seconds;
            OnTimeUpdated?.Invoke(currentTime);
        }
    }
}
