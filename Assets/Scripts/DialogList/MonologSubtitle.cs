using UnityEngine;
using TMPro;
using System.Collections;
using System;

// MONOLOG SUBTITLE
// Nampilin monolog Nara kayak teks subtitle di tengah bawah layar
// Auto-setup UI via code, gak perlu bikin Canvas manual
//
// Cara pakai:
//   1. Attach script ini ke GameObject kosong di scene
//   2. Panggil MonologSubtitle.Instance.Show("teks") dari mana aja
//   3. Atau ShowLines(string[]) buat beberapa baris berurutan

public class MonologSubtitle : MonoBehaviour
{
    public static MonologSubtitle Instance;

    [Header("Text Settings")]
    public float typingSpeed = 0.04f;
    public float displayDuration = 2.5f;    // Lama nongol setelah selesai ngetik
    public float fadeDuration = 0.4f;
    public int fontSize = 28;
    public Color textColor = new Color(1f, 1f, 1f, 1f);

    // Internal UI (auto-generated waktu game jalan)
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI tmp;
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        SetupUI();
    }

    // ============================================================
    // AUTO SETUP UI — bikin Canvas + TMP lewat code
    // ============================================================
    void SetupUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("MonologCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;

        UnityEngine.UI.CanvasScaler scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // TMP Text object
        GameObject textGO = new GameObject("MonologText");
        textGO.transform.SetParent(canvasGO.transform, false);

        tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.fontStyle = FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;

        // Drop shadow biar keliatan di background apapun
        UnityEngine.UI.Shadow shadow = textGO.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        // Posisi: tengah bawah layar
        RectTransform rect = tmp.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0f);
        rect.anchorMax = new Vector2(0.85f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 60f); // 60px dari bawah
        rect.sizeDelta = new Vector2(0f, 100f);
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    // Tampilkan satu baris monolog, callback dipanggil setelah selesai
    public void Show(string line, Action onComplete = null)
    {
        if (isPlaying) StopAllCoroutines();
        StartCoroutine(PlayLine(line, onComplete));
    }

    // Tampilkan beberapa baris berurutan, callback setelah SEMUA selesai
    public void ShowLines(string[] lines, Action onComplete = null)
    {
        if (isPlaying) StopAllCoroutines();
        StartCoroutine(PlayAllLines(lines, onComplete));
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    public bool IsPlaying() => isPlaying;

    // ============================================================
    // COROUTINES
    // ============================================================

    IEnumerator PlayLine(string line, Action onComplete)
    {
        isPlaying = true;
        tmp.text = "";

        yield return StartCoroutine(FadeIn());

        // Typing effect
        foreach (char c in line)
        {
            tmp.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Tunggu biar kebaca
        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeOut());

        isPlaying = false;
        onComplete?.Invoke();
    }

    IEnumerator PlayAllLines(string[] lines, Action onComplete)
    {
        foreach (string line in lines)
        {
            bool done = false;
            yield return StartCoroutine(PlayLine(line, () => done = true));
            yield return new WaitUntil(() => done);
        }
        onComplete?.Invoke();
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        tmp.text = "";
    }
}