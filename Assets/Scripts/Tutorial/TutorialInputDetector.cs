using System;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Mendeteksi input player sesuai jenis aksi yang dibutuhkan tutorial.
    /// Pisahkan dari TutorialManager agar bisa diuji atau diganti sendiri.
    /// </summary>
    public class TutorialInputDetector : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        //  Event yang dipanggil ketika aksi berhasil terdeteksi
        // ──────────────────────────────────────────────
        public event Action OnActionDetected;

        // ──────────────────────────────────────────────
        //  State internal
        // ──────────────────────────────────────────────
        private TutorialActionType _currentActionType;
        private bool _isListening = false;

        // Threshold minimum gerakan horizontal agar dianggap "gerak"
        private const float MOVE_THRESHOLD = 0.1f;

        // ──────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Mulai mendengarkan aksi tertentu. Panggil ini setiap kali step baru dimulai.
        /// </summary>
        public void StartListening(TutorialActionType actionType)
        {
            _currentActionType = actionType;
            _isListening = true;
        }

        /// <summary>
        /// Hentikan deteksi input (dipanggil saat step selesai atau tutorial di-skip).
        /// </summary>
        public void StopListening()
        {
            _isListening = false;
        }

        // ──────────────────────────────────────────────
        //  Unity Update — Polling input tiap frame
        // ──────────────────────────────────────────────

        private void Update()
        {
            if (!_isListening) return;

            bool detected = _currentActionType switch
            {
                TutorialActionType.MoveHorizontal => DetectHorizontalMove(),
                TutorialActionType.Jump => DetectJump(),
                TutorialActionType.Sprint => DetectSprint(),
                TutorialActionType.OpenMenu => DetectOpenMenu(),
                TutorialActionType.UIIntroduction => false, // Ditangani oleh timer di TutorialManager
                _ => false
            };

            if (detected)
            {
                _isListening = false;           // Cegah trigger berulang
                OnActionDetected?.Invoke();     // Beritahu TutorialManager
            }
        }

        // ──────────────────────────────────────────────
        //  Metode Deteksi Per Aksi
        // ──────────────────────────────────────────────

        /// <summary>
        /// Deteksi tombol A atau D (gerakan horizontal).
        /// Kompatibel dengan Input System lama (legacy) maupun direct key check.
        /// </summary>
        private bool DetectHorizontalMove()
        {
            float h = Input.GetAxisRaw("Horizontal");
            return Mathf.Abs(h) > MOVE_THRESHOLD;
        }

        /// <summary>
        /// Deteksi tombol Space untuk lompat.
        /// </summary>
        private bool DetectJump()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        /// <summary>
        /// Deteksi kombinasi Shift + gerakan horizontal (A atau D).
        /// </summary>
        private bool DetectSprint()
        {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float h = Input.GetAxisRaw("Horizontal");
            return shiftHeld && Mathf.Abs(h) > MOVE_THRESHOLD;
        }

        /// <summary>
        /// Deteksi tombol Escape untuk membuka menu.
        /// </summary>
        private bool DetectOpenMenu()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }
    }
}