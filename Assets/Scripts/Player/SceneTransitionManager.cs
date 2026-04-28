using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private Image transitionOverlay;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transitionSound;

    private bool isTransitioning = false;
    private PlayerController playerController;

    private void Awake()
    {
        // Singleton pattern - hanya ada 1 instance di seluruh game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Canvas tidak dihancurkan saat ganti scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pastikan overlay transparan di awal
        if (transitionOverlay != null)
        {
            Color color = transitionOverlay.color;
            color.a = 0f;
            transitionOverlay.color = color;
        }

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Fade in saat game pertama kali dimulai
        FindPlayerInScene();
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Memulai transisi ke scene baru berdasarkan nama
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneName));
        }
    }

    /// <summary>
    /// Memulai transisi ke scene baru berdasarkan index
    /// </summary>
    public void TransitionToScene(int sceneIndex)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine(sceneIndex));
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        // 1. Lock player movement dan stop velocity
        LockPlayerMovement(true);

        // 2. Play transition sound
        if (transitionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // 3. Fade to black (bayangan menutup)
        yield return StartCoroutine(FadeOut());

        // 4. Load scene baru
        SceneManager.LoadScene(sceneName);

        // 5. Tunggu satu frame untuk scene loading selesai
        yield return null;

        // 6. Cari player baru di scene
        FindPlayerInScene();

        // 7. Fade in (bayangan terbuka)
        yield return StartCoroutine(FadeIn());

        // 8. Unlock player movement
        LockPlayerMovement(false);

        isTransitioning = false;
    }

    private IEnumerator TransitionCoroutine(int sceneIndex)
    {
        isTransitioning = true;

        LockPlayerMovement(true);

        if (transitionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        yield return StartCoroutine(FadeOut());

        SceneManager.LoadScene(sceneIndex);
        yield return null;

        FindPlayerInScene();
        yield return StartCoroutine(FadeIn());

        LockPlayerMovement(false);
        isTransitioning = false;
    }

    /// <summary>
    /// Fade to black (menutup layar dengan bayangan hitam)
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float curveValue = fadeCurve.Evaluate(t);

            Color color = transitionOverlay.color;
            color.a = curveValue;
            transitionOverlay.color = color;

            yield return null;
        }

        // Pastikan alpha = 1 (hitam penuh)
        Color finalColor = transitionOverlay.color;
        finalColor.a = 1f;
        transitionOverlay.color = finalColor;
    }

    /// <summary>
    /// Fade from black (membuka layar, bayangan menghilang)
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float curveValue = fadeCurve.Evaluate(t);

            Color color = transitionOverlay.color;
            color.a = 1f - curveValue; // Kebalikan dari fade out
            transitionOverlay.color = color;

            yield return null;
        }

        // Pastikan alpha = 0 (transparan penuh)
        Color finalColor = transitionOverlay.color;
        finalColor.a = 0f;
        transitionOverlay.color = finalColor;
    }

    /// <summary>
    /// Lock/unlock player movement saat transisi
    /// </summary>
    private void LockPlayerMovement(bool shouldLock)
    {
        if (playerController != null)
        {
            // Disable input
            if (shouldLock)
            {
                playerController.enabled = false;

                // Stop velocity
                Rigidbody rb = playerController.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                playerController.enabled = true;
            }
        }
    }

    /// <summary>
    /// Cari player di scene (dipanggil setelah load scene baru)
    /// </summary>
    private void FindPlayerInScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();

            if (playerController == null)
            {
                Debug.LogWarning("Player ditemukan tapi tidak memiliki PlayerController component!");
            }
        }
        else
        {
            Debug.LogWarning("Player tidak ditemukan di scene! Pastikan Player memiliki tag 'Player'.");
        }
    }

    /// <summary>
    /// Cek apakah sedang dalam proses transisi
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}