using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
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

        public System.Action OnTutorialCompleted;
        public System.Action<int> OnStepChanged;

        // ──────────────────────────────────────────────
        //  State Internal
        // ──────────────────────────────────────────────

        private int _currentStepIndex = 0;
        private bool _isTransitioning = false;
        private bool _tutorialActive = false;
        private Coroutine _timerCoroutine;

        // Key untuk menyimpan status tutorial
        private const string TUTORIAL_DONE_KEY = "TutorialCompleted";

        // ──────────────────────────────────────────────
        //  Unity Lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (steps == null || steps.Count == 0)
                InitDefaultSteps();
        }

        private void Start()
        {
            if (inputDetector != null)
                inputDetector.OnActionDetected += HandleActionDetected;

            if (tutorialUI != null)
            {
                tutorialUI.OnSkipRequested += SkipTutorial;
                tutorialUI.SetTotalSteps(steps.Count);
            }

            // Cek apakah tutorial sudah pernah selesai
            if (PlayerPrefs.GetInt(TUTORIAL_DONE_KEY, 0) == 1)
            {
                tutorialUI?.gameObject.SetActive(false);
                return;
            }

            if (startOnAwake)
                StartTutorial();
        }

        private void OnDestroy()
        {
            if (inputDetector != null)
                inputDetector.OnActionDetected -= HandleActionDetected;

            if (tutorialUI != null)
                tutorialUI.OnSkipRequested -= SkipTutorial;
        }

        // ──────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────

        public void StartTutorial()
        {
            if (_tutorialActive) return;

            _currentStepIndex = 0;
            _tutorialActive = true;

            if (tutorialUI != null)
                tutorialUI.SetSkipButtonVisible(showSkipButton);

            LoadCurrentStep();
        }

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

            yield return StartCoroutine(tutorialUI.ShowStep(step, _currentStepIndex));

            _isTransitioning = false;

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

            yield return StartCoroutine(tutorialUI.HideStep());

            _currentStepIndex++;
            _isTransitioning = false;

            LoadCurrentStep();
        }

        private IEnumerator FinishTutorial()
        {
            _tutorialActive = false;

            yield return StartCoroutine(tutorialUI.HideStep());

            tutorialUI.SetSkipButtonVisible(false);

            // Simpan bahwa tutorial sudah selesai
            PlayerPrefs.SetInt(TUTORIAL_DONE_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("[TutorialManager] Tutorial selesai.");
            OnTutorialCompleted?.Invoke();
        }

        // ──────────────────────────────────────────────
        //  Reset Tutorial (untuk testing)
        // ──────────────────────────────────────────────

        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TUTORIAL_DONE_KEY);
            Debug.Log("[TutorialManager] Tutorial direset!");
        }

        // ──────────────────────────────────────────────
        //  Default Steps
        // ──────────────────────────────────────────────

        private void InitDefaultSteps()
        {
            steps = new List<TutorialStep>
            {
                new TutorialStep
                {
                    stepTitle       = "Bergerak",
                    instructionText = "Tekan [A] untuk bergerak ke kiri\natau [D] untuk bergerak ke kanan.",
                    requiredAction  = TutorialActionType.MoveHorizontal,
                    isTimedStep     = false
                },
                new TutorialStep
                {
                    stepTitle       = "Lompat",
                    instructionText = "Tekan [SPACE] untuk melompat.",
                    requiredAction  = TutorialActionType.Jump,
                    isTimedStep     = false
                },
                new TutorialStep
                {
                    stepTitle       = "Berlari",
                    instructionText = "Tahan [SHIFT] sambil menekan [A] atau [D]\nuntuk berlari lebih cepat.",
                    requiredAction  = TutorialActionType.Sprint,
                    isTimedStep     = false
                },
                new TutorialStep
                {
                    stepTitle       = "Menu",
                    instructionText = "Tekan [ESC] untuk membuka menu.",
                    requiredAction  = TutorialActionType.OpenMenu,
                    isTimedStep     = false
                },
                new TutorialStep
                {
                    stepTitle         = "Antarmuka Permainan",
                    instructionText   = "Perhatikan layar:\n• Atas kiri  — Status Kesehatan (HP)\n• Atas kanan — Inventory\n• Bawah      — Quest aktif",
                    requiredAction    = TutorialActionType.UIIntroduction,
                    isTimedStep       = true,
                    timedStepDuration = 5f,
                    highlightTargetName = "Panel_Health",
                    highlightColor      = new Color(1f, 0.85f, 0f, 0.5f)
                }
            };
        }
    }
}