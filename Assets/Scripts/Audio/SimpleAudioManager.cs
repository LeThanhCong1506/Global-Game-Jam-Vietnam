// File: Scripts/Audio/SimpleAudioManager.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Simple audio manager for playing sound effects.
    /// All clips are optional - works as stubs if no clips assigned.
    /// 
    /// Audio files to add:
    /// - move.wav - Di chuyển
    /// - mask_off.wav, mask_a.wav, mask_b.wav, mask_c.wav - Đổi mask
    /// - error.wav - Lỗi
    /// - trap.wav - Dính bẫy
    /// - key_collect.wav - Nhặt chìa khóa
    /// - win.wav - Chiến thắng
    /// - lose.wav - Thua cuộc
    /// </summary>
    public class SimpleAudioManager : MonoBehaviour
    {
        public static SimpleAudioManager Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Movement Sounds")]
        [Tooltip("move.wav - Phát khi player di chuyển")]
        [SerializeField] private AudioClip moveClip;

        [Header("Mask Sounds")]
        [Tooltip("mask_off.wav - Phát khi tắt mask (về Off)")]
        [SerializeField] private AudioClip maskOffClip;
        
        [Tooltip("mask_a.wav - Phát khi bật Mask A")]
        [SerializeField] private AudioClip maskAClip;
        
        [Tooltip("mask_b.wav - Phát khi bật Mask B")]
        [SerializeField] private AudioClip maskBClip;
        
        [Tooltip("mask_c.wav - Phát khi bật Mask C")]
        [SerializeField] private AudioClip maskCClip;

        [Header("Gameplay Sounds")]
        [Tooltip("error.wav - Phát khi thao tác lỗi (invalid move, locked door)")]
        [SerializeField] private AudioClip errorClip;
        
        [Tooltip("trap.wav - Phát khi dính bẫy")]
        [SerializeField] private AudioClip trapClip;
        
        [Tooltip("key_collect.wav - Phát khi nhặt chìa khóa")]
        [SerializeField] private AudioClip keyCollectClip;

        [Header("Game State Sounds")]
        [Tooltip("win.wav - Phát khi chiến thắng")]
        [SerializeField] private AudioClip winClip;
        
        [Tooltip("lose.wav - Phát khi thua cuộc")]
        [SerializeField] private AudioClip loseClip;

        [Header("Other Sounds (Optional)")]
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

        /// <summary>
        /// Play appropriate mask sound based on which mask was activated
        /// </summary>
        private void OnMaskChanged(MaskType newMask)
        {
            switch (newMask)
            {
                case MaskType.Off:
                    PlayClip(maskOffClip);
                    break;
                case MaskType.MaskA:
                    PlayClip(maskAClip);
                    break;
                case MaskType.MaskB:
                    PlayClip(maskBClip);
                    break;
                case MaskType.MaskC:
                    PlayClip(maskCClip);
                    break;
            }
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        // === PUBLIC METHODS ===

        /// <summary>
        /// Play move sound (move.wav)
        /// </summary>
        public void PlayMove() => PlayClip(moveClip);

        /// <summary>
        /// Play trap sound (trap.wav)
        /// </summary>
        public void PlayTrap() => PlayClip(trapClip);

        /// <summary>
        /// Play key collect sound (key_collect.wav)
        /// </summary>
        public void PlayPickup() => PlayClip(keyCollectClip);
        public void PlayKeyCollect() => PlayClip(keyCollectClip);

        /// <summary>
        /// Play win sound (win.wav)
        /// </summary>
        public void PlayWin() => PlayClip(winClip);

        /// <summary>
        /// Play lose sound (lose.wav)
        /// </summary>
        public void PlayLose() => PlayClip(loseClip);

        /// <summary>
        /// Play error sound (error.wav) - for invalid moves, locked doors, etc.
        /// </summary>
        public void PlayInvalid() => PlayClip(errorClip);
        public void PlayLocked() => PlayClip(errorClip);
        public void PlayError() => PlayClip(errorClip);

        /// <summary>
        /// Play rotate sound (optional)
        /// </summary>
        public void PlayRotate() => PlayClip(rotateClip);

        // === MASK SOUNDS (can be called manually if needed) ===

        public void PlayMaskOff() => PlayClip(maskOffClip);
        public void PlayMaskA() => PlayClip(maskAClip);
        public void PlayMaskB() => PlayClip(maskBClip);
        public void PlayMaskC() => PlayClip(maskCClip);
        
        // Legacy method for compatibility
        public void PlayMaskSwap() => PlayClip(maskOffClip);

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

