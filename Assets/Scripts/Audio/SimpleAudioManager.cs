// File: Scripts/Audio/SimpleAudioManager.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Simple audio manager for playing sound effects.
    /// All clips are optional - works as stubs if no clips assigned.
    /// </summary>
    public class SimpleAudioManager : MonoBehaviour
    {
        public static SimpleAudioManager Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Sound Effects (Optional)")]
        [SerializeField] private AudioClip maskSwapClip;
        [SerializeField] private AudioClip moveClip;
        [SerializeField] private AudioClip trapClip;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] private AudioClip lockedClip;
        [SerializeField] private AudioClip invalidClip;
        [SerializeField] private AudioClip rotateClip;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create audio source if not assigned
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            MaskManager.OnMaskChanged += OnMaskChanged;
        }

        private void OnDisable()
        {
            MaskManager.OnMaskChanged -= OnMaskChanged;
        }

        private void OnMaskChanged(MaskType newMask)
        {
            PlayMaskSwap();
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void PlayMaskSwap() => PlayClip(maskSwapClip);
        public void PlayMove() => PlayClip(moveClip);
        public void PlayTrap() => PlayClip(trapClip);
        public void PlayPickup() => PlayClip(pickupClip);
        public void PlayWin() => PlayClip(winClip);
        public void PlayLose() => PlayClip(loseClip);
        public void PlayLocked() => PlayClip(lockedClip);
        public void PlayInvalid() => PlayClip(invalidClip);
        public void PlayRotate() => PlayClip(rotateClip);

        /// <summary>
        /// Play a custom clip.
        /// </summary>
        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume * sfxVolume);
            }
        }

        /// <summary>
        /// Set master SFX volume.
        /// </summary>
        public void SetVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
    }
}
