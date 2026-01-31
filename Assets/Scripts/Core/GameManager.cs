// File: Scripts/Core/GameManager.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Main game manager controlling game state.
    /// Handles Playing, Win, Lose states and restart.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Initial State")]
        [SerializeField] private GameState initialState = GameState.Playing;

        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.Playing;

        // Event fired when game state changes
        public static event Action<GameState> OnGameStateChanged;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetGameState(initialState);
        }

        /// <summary>
        /// Change the current game state.
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {currentState}");

            // Handle state-specific logic
            switch (currentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.Win:
                    Time.timeScale = 0f;
                    break;

                case GameState.Lose:
                    Time.timeScale = 0f;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
            }

            OnGameStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Restart the current level.
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Load next level (if exists).
        /// </summary>
        public void LoadNextLevel()
        {
            Time.timeScale = 1f;
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.Log("[GameManager] No more levels. Restarting current.");
                RestartLevel();
            }
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume the game.
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }
    }
}
