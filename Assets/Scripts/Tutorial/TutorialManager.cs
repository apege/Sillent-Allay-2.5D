using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// TutorialManager — Controller utama sistem tutorial interaktif.
    ///
    /// Cara kerja:
    ///   1. Saat game dimulai, manager memuat daftar TutorialStep.
    ///   2. Setiap step ditampilkan via TutorialUI (fade in teks instruksi).
    ///   3. TutorialInputDetector menunggu aksi player yang sesuai.
    ///   4. Jika aksi terdeteksi, lanjut ke step berikutnya (fade out → fade in).
    ///   5. Setelah semua step selesai, event OnTutorialCompleted dipanggil.
    ///
    /// Setup di Unity:
    ///   - Buat GameObject kosong bernama "TutorialManager".
    ///   - Pasang komponen ini, TutorialUI, dan TutorialInputDetector.
    ///   - Isi daftar steps di Inspector, atau biarkan default (lihat InitDefaultSteps).
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        //  Serialized Fields (Inspector)
        // ──────────────────────────────────────────────

        [Header("Referensi Komponen")]
        [SerializeField] private TutorialUI tutorialUI;
        [SerializeField] private TutorialInputDetector inputDetector;

        [Header("Daftar Step Tutorial")]
        [Tooltip("Isi secara manual, atau biarkan kosong untuk menggunakan default bawaan.")]
        [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

        [Header("Pengaturan")]
        [SerializeField] private bool showSkipButton = true;
        [SerializeField] private bool startOnAwake = true;

        // ──────────────────────────────────────────────
        //  Events
        // ──────────────────────────────────────────────

        /// <summary>Dipanggil saat tutorial selesai (semua step atau di-skip).</summary>
        public System.Action OnTutorialCompleted;

        /// <summary>Dipanggil setiap kali step berubah. Parameter: nomor step (0-based).</summary>
        public System.Action<int> OnStepChanged;

        // ──────────────────────────────────────────────
        //  State Internal
        // ──────────────────────────────────────────────

        private int _currentStepIndex = 0;
        private bool _isTransitioning = false;
        private bool _tutorialActive = false;
        private Coroutine _timerCoroutine;

        // ──────────────────────────────────────────────
        //  Unity Lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            // Jika daftar steps kosong, gunakan preset default
            if (steps == null || steps.Count == 0)
                InitDefaultSteps();
        }

        private void Start()
        {
            // Setup event dari input detector dan tombol skip
            if (inputDetector != null)
                inputDetector.OnActionDetected += HandleActionDetected;

            if (tutorialUI != null)
            {
                tutorialUI.OnSkipRequested += SkipTutorial;
                tutorialUI.SetTotalSteps(steps.Count);
            }

            if (startOnAwake)
                StartTutorial();
        }

        private void OnDestroy()
        {
            // Bersihkan event listener agar tidak terjadi memory leak
            if (inputDetector != null)
                inputDetector.OnActionDetected -= HandleActionDetected;

            if (tutorialUI != null)
                tutorialUI.OnSkipRequested -= SkipTutorial;
        }

        // ──────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Mulai tutorial dari step pertama.
        /// </summary>
        public void StartTutorial()
        {
            if (_tutorialActive) return;

            _currentStepIndex = 0;
            _tutorialActive = true;

            if (tutorialUI != null)
                tutorialUI.SetSkipButtonVisible(showSkipButton);

            LoadCurrentStep();
        }

        /// <summary>
        /// Lewati semua sisa tutorial sekarang.
        /// </summary>
        public void SkipTutorial()
        {
            if (!_tutorialActive) return;

            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            inputDetector?.StopListening();
            StartCoroutine(FinishTutorial());
        }

        // ──────────────────────────────────────────────
        //  Step Logic (Private)
        // ──────────────────────────────────────────────

        private void LoadCurrentStep()
        {
            if (_currentStepIndex >= steps.Count)
            {
                StartCoroutine(FinishTutorial());
                return;
            }

            TutorialStep step = steps[_currentStepIndex];
            OnStepChanged?.Invoke(_currentStepIndex);

            StartCoroutine(ShowStepRoutine(step));
        }

        private IEnumerator ShowStepRoutine(TutorialStep step)
        {
            _isTransitioning = true;

            // Tampilkan UI step (fade in)
            yield return StartCoroutine(tutorialUI.ShowStep(step, _currentStepIndex));

            _isTransitioning = false;

            // Mulai mendeteksi input (kecuali UIIntroduction, gunakan timer)
            if (step.isTimedStep || step.requiredAction == TutorialActionType.UIIntroduction)
            {
                _timerCoroutine = StartCoroutine(TimedStepRoutine(step.timedStepDuration));
            }
            else
            {
                inputDetector?.StartListening(step.requiredAction);
            }
        }

        private IEnumerator TimedStepRoutine(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            AdvanceToNextStep();
        }

        private void HandleActionDetected()
        {
            if (_isTransitioning) return;
            AdvanceToNextStep();
        }

        private void AdvanceToNextStep()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            inputDetector?.StopListening();
            StartCoroutine(TransitionToNextStep());
        }

        private IEnumerator TransitionToNextStep()
        {
            _isTransitioning = true;

            // Fade out UI step saat ini
            yield return StartCoroutine(tutorialUI.HideStep());

            _currentStepIndex++;
            _isTransitioning = false;

            // Muat step berikutnya
            LoadCurrentStep();
        }

        private IEnumerator FinishTutorial()
        {
            _tutorialActive = false;

            yield return StartCoroutine(tutorialUI.HideStep());

            tutorialUI.SetSkipButtonVisible(false);
            Debug.Log("[TutorialManager] Tutorial selesai.");
            OnTutorialCompleted?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  Default Steps (Fallback / Contoh)
        // ──────────────────────────────────────────────

        /// <summary>
        /// Inisialisasi 5 step tutorial default jika Inspector tidak diisi.
        /// Gunakan ini sebagai referensi untuk mengisi langsung dari Inspector.
        /// </summary>
        private void InitDefaultSteps()
        {
            steps = new List<TutorialStep>
            {
                // ─── Step 1: Gerak kanan/kiri ───────────────────
                new TutorialStep
                {
                    stepTitle       = "Bergerak",
                    instructionText = "Tekan [A] untuk bergerak ke kiri\natau [D] untuk bergerak ke kanan.",
                    requiredAction  = TutorialActionType.MoveHorizontal,
                    isTimedStep     = false
                },

                // ─── Step 2: Lompat ─────────────────────────────
                new TutorialStep
                {
                    stepTitle       = "Lompat",
                    instructionText = "Tekan [SPACE] untuk melompat.",
                    requiredAction  = TutorialActionType.Jump,
                    isTimedStep     = false
                },

                // ─── Step 3: Lari ───────────────────────────────
                new TutorialStep
                {
                    stepTitle       = "Berlari",
                    instructionText = "Tahan [SHIFT] sambil menekan [A] atau [D]\nuntuk berlari lebih cepat.",
                    requiredAction  = TutorialActionType.Sprint,
                    isTimedStep     = false
                },

                // ─── Step 4: Buka menu ──────────────────────────
                new TutorialStep
                {
                    stepTitle       = "Menu",
                    instructionText = "Tekan [ESC] untuk membuka menu.",
                    requiredAction  = TutorialActionType.OpenMenu,
                    isTimedStep     = false
                },

                // ─── Step 5: Pengenalan UI ──────────────────────
                new TutorialStep
                {
                    stepTitle         = "Antarmuka Permainan",
                    instructionText   = "Perhatikan layar:\n• Atas kiri  — Status Kesehatan (HP)\n• Atas kanan — Inventory\n• Bawah      — Quest aktif",
                    requiredAction    = TutorialActionType.UIIntroduction,
                    isTimedStep       = true,
                    timedStepDuration = 5f,
                    // Opsional: highlight panel Health
                    highlightTargetName = "Panel_Health",
                    highlightColor      = new Color(1f, 0.85f, 0f, 0.5f)
                }
            };
        }
    }
}