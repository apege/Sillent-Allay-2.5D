using UnityEngine;
using TMPro;
using System.Collections;
using System;

// SUBTITLE MANAGER
// Nampilin monolog / narasi kayak subtitle di bawah layar
// Attach ke GameObject kosong di scene, bisa dipanggil dari mana aja via SubtitleManager.Instance

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [Header("UI References")]
    public GameObject subtitleBox;           // Panel/background subtitle
    public TextMeshProUGUI subtitleText;     // Teks subtitle-nya
    public TextMeshProUGUI speakerLabel;     // Nama speaker (optional, bisa dikosongkan)

    [Header("Settings")]
    public float typingSpeed = 0.04f;        // Kecepatan ketik per karakter
    public float displayDuration = 2.5f;     // Berapa lama subtitle nongol setelah selesai ngetik
    public float fadeDuration = 0.3f;        // Durasi fade in/out

    [Header("Monolog Style")]
    public Color monologColor = new Color(0.9f, 0.9f, 0.9f, 1f);   // Warna teks monolog (putih soft)
    public FontStyles monologFontStyle = FontStyles.Italic;          // Italic buat monolog

    private CanvasGroup canvasGroup;
    private bool isPlaying = false;

    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Setup canvas group buat fade
        if (subtitleBox != null)
        {
            canvasGroup = subtitleBox.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = subtitleBox.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            subtitleBox.SetActive(false);
        }
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    // Tampilkan satu baris subtitle, lalu panggil callback kalau udah selesai
    public void ShowLine(string line, string speaker = "", Action onComplete = null)
    {
        if (isPlaying) StopAllCoroutines();
        StartCoroutine(PlayLine(line, speaker, onComplete));
    }

    // Tampilkan beberapa baris berurutan, callback dipanggil setelah SEMUA selesai
    public void ShowLines(string[] lines, string[] speakers = null, Action onComplete = null)
    {
        if (isPlaying) StopAllCoroutines();
        StartCoroutine(PlayLines(lines, speakers, onComplete));
    }

    // Sembunyikan subtitle paksa
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    public bool IsPlaying() => isPlaying;

    // ============================================================
    // COROUTINES
    // ============================================================

    IEnumerator PlayLine(string line, string speaker, Action onComplete)
    {
        isPlaying = true;

        // Setup teks
        if (speakerLabel != null)
        {
            speakerLabel.text = speaker;
            speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        }

        // Style monolog (italic, warna beda)
        if (subtitleText != null)
        {
            subtitleText.color = monologColor;
            subtitleText.fontStyle = monologFontStyle;
            subtitleText.text = "";
        }

        // Fade in
        subtitleBox.SetActive(true);
        yield return StartCoroutine(FadeIn());

        // Typing effect
        foreach (char c in line)
        {
            if (subtitleText != null)
                subtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Tunggu sebentar biar kebaca
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeOut());

        isPlaying = false;
        onComplete?.Invoke();
    }

    IEnumerator PlayLines(string[] lines, string[] speakers, Action onComplete)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            string speaker = (speakers != null && i < speakers.Length) ? speakers[i] : "";
            bool isDone = false;

            yield return StartCoroutine(PlayLine(lines[i], speaker, () => isDone = true));

            // Tunggu sampe bener-bener selesai
            yield return new WaitUntil(() => isDone);
        }

        onComplete?.Invoke();
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (subtitleBox != null) subtitleBox.SetActive(false);
    }
}