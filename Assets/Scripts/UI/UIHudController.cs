// File: Scripts/UI/UIHudController.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// HUD controller showing mask state, timer, key status, and toast messages.
    /// Supports both Unity UI Text and TextMeshPro via wrapper approach.
    /// </summary>
    public class UIHudController : MonoBehaviour
    {
        public static UIHudController Instance { get; private set; }

        [Header("Timer Display")]
        [SerializeField] private GameObject timer;
        private TextMeshPro timerText;
        [SerializeField] private Image timerFillBar;

        [Header("Mask Display")]
        [SerializeField] private GameObject mask;
        private TextMeshPro maskText;
        [SerializeField] private Image[] maskIndicators; // 4 indicators for Off, A, B, C

        [Header("Key Status")]
        [SerializeField] private GameObject keyStatus;
        private TextMeshPro keyStatusText;
        [SerializeField] private GameObject keyIcon;
        [SerializeField] private Color keyNotCollectedColor = Color.gray;
        [SerializeField] private Color keyCollectedColor = Color.yellow;

        [Header("Toast Messages")]
        [SerializeField] private GameObject toast;
        private TextMeshPro toastText;
        [SerializeField] private CanvasGroup toastGroup;
        [SerializeField] private float toastFadeSpeed = 2f;

        [Header("Game Over Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Colors")]
        [SerializeField] private Color maskActiveColor = Color.green;
        [SerializeField] private Color maskInactiveColor = Color.gray;

        private Coroutine toastCoroutine;

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
            // Cache text components
            timerText = timer.GetComponent<TextMeshPro>();
            maskText = mask.GetComponent<TextMeshPro>();
            keyStatusText = keyStatus.GetComponent<TextMeshPro>();
            toastText = toast.GetComponent<TextMeshPro>();

            // Hide game over panels
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);

            // Hide toast initially
            if (toastGroup != null) toastGroup.alpha = 0;

            // Initialize displays
            UpdateKeyStatus(false);
        }

        private void OnEnable()
        {
            MaskManager.OnMaskChanged += OnMaskChanged;
            LevelTimer.OnTimeUpdated += OnTimeUpdated;
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            MaskManager.OnMaskChanged -= OnMaskChanged;
            LevelTimer.OnTimeUpdated -= OnTimeUpdated;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnMaskChanged(MaskType newMask)
        {
            // Update mask text
            SetText(maskText, $"Mask: {GetMaskDisplayName(newMask)}");

            // Update mask indicators
            if (maskIndicators != null)
            {
                for (int i = 0; i < maskIndicators.Length; i++)
                {
                    if (maskIndicators[i] != null)
                    {
                        maskIndicators[i].color = (i == (int)newMask) ? maskActiveColor : maskInactiveColor;
                    }
                }
            }
        }

        private string GetMaskDisplayName(MaskType mask)
        {
            switch (mask)
            {
                case MaskType.Off: return "OFF (0)";
                case MaskType.MaskA: return "A - Paths (1)";
                case MaskType.MaskB: return "B - Traps (2)";
                case MaskType.MaskC: return "C - Hidden (3)";
                default: return mask.ToString();
            }
        }

        private void OnTimeUpdated(float time)
        {
            // Update timer text
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            SetText(timerText, $"{minutes:00}:{seconds:00}");

            // Update fill bar
            if (timerFillBar != null && LevelTimer.Instance != null)
            {
                timerFillBar.fillAmount = time / LevelTimer.Instance.StartTime;

                // Change color when low
                timerFillBar.color = time < 10f ? Color.red : Color.white;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            if (winPanel != null) winPanel.SetActive(newState == GameState.Win);
            if (losePanel != null) losePanel.SetActive(newState == GameState.Lose);
        }

        /// <summary>
        /// Update key collected status display.
        /// </summary>
        public void UpdateKeyStatus(bool collected)
        {
            SetText(keyStatusText, collected ? "Key: COLLECTED" : "Key: Not Found");

            if (keyIcon != null)
            {
                Image iconImage = keyIcon.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.color = collected ? keyCollectedColor : keyNotCollectedColor;
                }
            }
        }

        /// <summary>
        /// Show a toast message for specified duration.
        /// </summary>
        public void ShowToast(string message, float duration = 2f)
        {
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(ToastCoroutine(message, duration));
        }

        private IEnumerator ToastCoroutine(string message, float duration)
        {
            SetText(toastText, message);

            // Fade in
            if (toastGroup != null)
            {
                while (toastGroup.alpha < 1)
                {
                    toastGroup.alpha += Time.deltaTime * toastFadeSpeed;
                    yield return null;
                }
                toastGroup.alpha = 1;
            }

            yield return new WaitForSeconds(duration);

            // Fade out
            if (toastGroup != null)
            {
                while (toastGroup.alpha > 0)
                {
                    toastGroup.alpha -= Time.deltaTime * toastFadeSpeed;
                    yield return null;
                }
                toastGroup.alpha = 0;
            }
        }

        // Replace the SetText helper to support both TextMeshPro and UnityEngine.UI.Text
        private void SetText(Object textComponent, string value)
        {
            if (textComponent is TMPro.TextMeshPro tmp)
            {
                tmp.text = value;
            }
            else if (textComponent is UnityEngine.UI.Text uiText)
            {
                uiText.text = value;
            }
        }

        /// <summary>
        /// Called by Restart button.
        /// </summary>
        public void OnRestartButtonClicked()
        {
            GameManager.Instance?.RestartLevel();
        }
    }
}
