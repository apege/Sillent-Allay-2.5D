using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup blackOverlay;       // Image hitam full screen
    public CanvasGroup warningPanel;       // Panel WARNING
    public TextMeshProUGUI warningText;    // Text "⚠ WARNING"
    public TextMeshProUGUI subtitleText;   // Text "hahahaha nara..."
    public Image flashlightImage;          // Image putih buat flash effect

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public float warningHoldDuration = 2.5f;
    public float flashDuration = 0.1f;
    public int flashCount = 3;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // Pastiin semua tersembunyi dulu
        blackOverlay.alpha = 1f;
        warningPanel.alpha = 0f;
        subtitleText.alpha = 0f;
        flashlightImage.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(0.5f);

        // Fade in warning panel
        yield return StartCoroutine(FadeCanvasGroup(warningPanel, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(0.5f);

        // Flash effect (jumpscare)
        yield return StartCoroutine(DoFlash());

        yield return new WaitForSeconds(warningHoldDuration);

        // Fade out warning
        yield return StartCoroutine(FadeCanvasGroup(warningPanel, 1f, 0f, fadeDuration));

        yield return new WaitForSeconds(0.3f);

        // Munculin teks di background gelap
        yield return StartCoroutine(FadeText(subtitleText, 0f, 1f, 1f));

        yield return new WaitForSeconds(3f);

        // Fade out teks, fade out black overlay → masuk game
        yield return StartCoroutine(FadeText(subtitleText, 1f, 0f, 0.5f));
        yield return new WaitForSeconds(5f);


        // Load scene / enable player input dsb
        OnCutsceneEnd();
    }

    IEnumerator DoFlash()
    {
        for (int i = 0; i < flashCount; i++)
        {
            // Flash putih
            flashlightImage.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(flashDuration);
            flashlightImage.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(flashDuration * 2);
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            text.color = c;
            yield return null;
        }
        c.a = to;
        text.color = c;
    }

    void OnCutsceneEnd()
    {
        Debug.Log("Cutscene selesai, masuk game!");
        SceneManager.LoadSceneAsync("Sekolah_lt1"); // uncomment kalau mau load scene
    }
}