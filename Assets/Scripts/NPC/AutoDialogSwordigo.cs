using UnityEngine;
using TMPro;
using System.Collections;

// VERSI SUPER SIMPLE: Dialog otomatis muncul & ilang
// KAYAK SWORDIGO - Tanpa perlu pencet tombol!

public class AutoDialogSwordigo : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 6)]
    public string[] dialogLines; // Semua dialog
    public float typingSpeed = 0.05f; // Kecepatan ketik
    public bool showOnlyOnce = false; // Cuma muncul sekali?
    public float autoNextDelay = 2f; // Otomatis next dialog setelah X detik (0 = manual)

    [Header("UI References")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;
    public string npcName = "NPC";

    [Header("Auto-Resize Settings")]
    public bool autoResizeBox = true; // Otomatis resize box sesuai text
    public float padding = 40f; // Padding kiri-kanan
    public float minWidth = 200f; // Lebar minimum
    public float maxWidth = 800f; // Lebar maksimum
    public float verticalPadding = 60f; // Padding atas-bawah

    [Header("Trigger Settings")]
    public float triggerDistance = 5f; // Jarak trigger
    public bool debugMode = true; // Aktifkan buat liat log

    // Private variables
    private RectTransform dialogBoxRect;
    private bool isDialogActive = false;
    private bool hasShownDialog = false;
    private int currentLineIndex = 0;
    private Transform player;
    private CanvasGroup dialogCanvasGroup;

    void Start()
    {
        if (debugMode) Debug.Log($"[AutoDialog] {gameObject.name} initialized");

        // Setup DialogBox
        if (dialogBox != null)
        {
            // Tambah CanvasGroup kalo belum ada
            dialogCanvasGroup = dialogBox.GetComponent<CanvasGroup>();
            if (dialogCanvasGroup == null)
            {
                dialogCanvasGroup = dialogBox.AddComponent<CanvasGroup>();
            }

            // Get RectTransform buat resize
            dialogBoxRect = dialogBox.GetComponent<RectTransform>();

            // Hide di awal
            dialogCanvasGroup.alpha = 0;
            dialogBox.SetActive(false);

            if (debugMode) Debug.Log("[AutoDialog] DialogBox setup complete");
        }
        else
        {
            Debug.LogError($"[AutoDialog] DialogBox NULL di {gameObject.name}! Assign di Inspector!");
        }

        // Cari Player
        FindPlayer();

        // Validasi
        if (dialogText == null)
        {
            Debug.LogError($"[AutoDialog] DialogText NULL di {gameObject.name}!");
        }
        if (dialogLines.Length == 0)
        {
            Debug.LogWarning($"[AutoDialog] Ga ada dialog lines di {gameObject.name}!");
        }
    }

    void FindPlayer()
    {
        // Coba cari pake tag Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            if (debugMode) Debug.Log($"[AutoDialog] Player found: {playerObj.name}");
        }
        else
        {
            // Coba cari manual
            playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.LogWarning("[AutoDialog] Player found by name (not tag). Harusnya pake tag 'Player'!");
            }
            else
            {
                Debug.LogError("[AutoDialog] PLAYER GA KETEMU! Set tag Player ke player GameObject!");
            }
        }
    }

    void Update()
    {
        // Kalo player ga ada, coba cari lagi
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Hitung jarak
        float distance = Vector3.Distance(transform.position, player.position);

        // Player masuk radius & dialog belum aktif
        if (distance <= triggerDistance && !isDialogActive)
        {
            // Cek show only once
            if (showOnlyOnce && hasShownDialog)
            {
                return;
            }

            if (debugMode) Debug.Log($"[AutoDialog] Player masuk radius! Distance: {distance:F2}");
            ShowDialog();
        }
        // Player keluar radius & dialog aktif
        else if (distance > triggerDistance && isDialogActive)
        {
            if (debugMode) Debug.Log($"[AutoDialog] Player keluar radius! Distance: {distance:F2}");
            HideDialog();
        }
    }

    void ShowDialog()
    {
        if (dialogBox == null || dialogText == null)
        {
            Debug.LogError("[AutoDialog] UI references NULL! Ga bisa show dialog!");
            return;
        }

        if (debugMode) Debug.Log("[AutoDialog] ===== SHOWING DIALOG =====");

        isDialogActive = true;
        hasShownDialog = true;
        currentLineIndex = 0;

        // Aktifkan DialogBox
        dialogBox.SetActive(true);

        // Set nama NPC
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }

        // Fade in
        StartCoroutine(FadeDialog(true));

        // Tampilkan dialog pertama
        if (dialogLines.Length > 0)
        {
            StartCoroutine(ShowDialogLine(currentLineIndex));
        }
    }

    void HideDialog()
    {
        if (!isDialogActive) return;

        if (debugMode) Debug.Log("[AutoDialog] ===== HIDING DIALOG =====");

        isDialogActive = false;

        // Stop typing
        StopAllCoroutines();

        // Fade out
        StartCoroutine(FadeDialog(false));
    }

    IEnumerator FadeDialog(bool fadeIn)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (dialogCanvasGroup != null)
            {
                dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            }

            yield return null;
        }

        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.alpha = targetAlpha;
        }

        // Kalo fade out, matiin GameObject
        if (!fadeIn && dialogBox != null)
        {
            dialogBox.SetActive(false);
        }

        if (debugMode) Debug.Log($"[AutoDialog] Fade complete. Alpha: {targetAlpha}");
    }

    void ResizeDialogBox(string text)
    {
        if (!autoResizeBox || dialogBoxRect == null || dialogText == null) return;

        // Set text sementara buat calculate size
        string originalText = dialogText.text;
        dialogText.text = text;

        // Force update buat calculate preferred size
        Canvas.ForceUpdateCanvases();
        dialogText.ForceMeshUpdate();

        // Get preferred size dari text
        Vector2 textSize = dialogText.GetPreferredValues(text);

        // Calculate box size dengan padding
        float width = Mathf.Clamp(textSize.x + padding * 2, minWidth, maxWidth);
        float height = Mathf.Clamp(textSize.y + verticalPadding, 80f, 500f);

        // Set size
        dialogBoxRect.sizeDelta = new Vector2(width, height);

        // Restore original text
        dialogText.text = originalText;

        if (debugMode)
        {
            Debug.Log($"[AutoDialog] Box resized to: {width:F0}x{height:F0} (text size: {textSize.x:F0}x{textSize.y:F0})");
        }
    }

    IEnumerator ShowDialogLine(int index)
    {
        if (index >= dialogLines.Length) yield break;

        string line = dialogLines[index];

        if (debugMode) Debug.Log($"[AutoDialog] Showing line {index}: {line}");

        // Resize box sesuai panjang text
        ResizeDialogBox(line);

        // Typewriter effect
        dialogText.text = "";

        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (debugMode) Debug.Log("[AutoDialog] Typing complete");

        // Auto next kalo ada setting
        if (autoNextDelay > 0 && currentLineIndex < dialogLines.Length - 1)
        {
            yield return new WaitForSeconds(autoNextDelay);

            // Cek masih aktif ga
            if (isDialogActive)
            {
                currentLineIndex++;
                StartCoroutine(ShowDialogLine(currentLineIndex));
            }
        }
    }

    // Visualisasi trigger radius di Scene view
    void OnDrawGizmosSelected()
    {
        // Circle trigger radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        // Fill
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, triggerDistance);

        // Line ke player pas play mode
        if (Application.isPlaying && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            Gizmos.color = dist <= triggerDistance ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            // Label distance
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                (transform.position + player.position) / 2,
                $"Distance: {dist:F2}m"
            );
#endif
        }
    }
}