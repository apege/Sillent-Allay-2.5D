using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("Animasi")]
        [SerializeField] private float fadeInDuration = 0.35f;
        [SerializeField] private float fadeOutDuration = 0.25f;
        public System.Action OnSkipRequested;
        private TutorialCanvasSetup _setup;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _raycaster;
        private int _totalSteps;

        private void Awake()
        {
            _setup = GetComponent<TutorialCanvasSetup>();
            if (_setup == null)
            {
                Debug.LogError("[TutorialUI] TutorialCanvasSetup tidak ditemukan!");
                return;
            }
            _canvasGroup = _setup.panelCanvasGroup;
            _raycaster = GetComponent<GraphicRaycaster>();

            // Matiin raycaster di awal
            if (_raycaster != null) _raycaster.enabled = false;

            if (_setup.skipButton != null)
                _setup.skipButton.onClick.AddListener(() => OnSkipRequested?.Invoke());
        }

        public void SetTotalSteps(int total) { _totalSteps = total; }

        public IEnumerator ShowStep(TutorialStep step, int stepIndex)
        {
            if (_setup == null) yield break;

            // Nyalain raycaster hanya waktu tutorial aktif
            if (_raycaster != null) _raycaster.enabled = true;

            if (_setup.instructionText != null)
                _setup.instructionText.text = step.instructionText;
            if (_setup.stepLabelText != null)
                _setup.stepLabelText.text = $"Langkah {stepIndex + 1} dari {_totalSteps}";

            _setup.ShowKeysForAction(step.requiredAction);
            _setup.UpdateStepDots(stepIndex, _totalSteps);

            yield return StartCoroutine(FadePanel(0f, 1f, fadeInDuration));

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public IEnumerator HideStep()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            yield return StartCoroutine(FadePanel(1f, 0f, fadeOutDuration));

            // Matiin raycaster setelah hidden
            if (_raycaster != null) _raycaster.enabled = false;
        }

        public void SetSkipButtonVisible(bool visible)
        {
            if (_setup?.skipButton != null)
                _setup.skipButton.gameObject.SetActive(visible);
        }

        private IEnumerator FadePanel(float from, float to, float duration)
        {
            if (_canvasGroup == null) yield break;
            float elapsed = 0f;
            _canvasGroup.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
        }
    }
}