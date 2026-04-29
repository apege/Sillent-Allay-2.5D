using UnityEngine;
using TMPro;
using System.Collections;

// DIALOG WORLD SPACE: Muncul di atas kepala NPC kayak speech bubble
// Support dialog chaining: habis dialog ini, lanjut ke dialog berikutnya

public class WorldSpaceDialog : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 6)]
    public string[] dialogLines;
    public float typingSpeed = 0.05f;
    public float autoAdvanceDelay = 1.5f;  // Jeda antar baris
    public bool showOnlyOnce = false;

    [Header("Dialog Chain")]
    public WorldSpaceDialog nextDialog;          // Dialog yang main setelah ini selesai
    public float delayBeforeNextDialog = 0.5f;   // Jeda sebelum dialog berikutnya mulai

    [Header("UI References")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;
    public string npcName = "NPC";

    [Header("World Position Settings")]
    public Vector3 dialogOffset = new Vector3(0, 2.5f, 0);
    public bool followNPC = true;

    [Header("Trigger Settings")]
    public float triggerDistance = 5f;
    public bool debugMode = true;

    // State
    private bool isDialogActive = false;
    private bool hasShownDialog = false;
    private bool isChained = false;       // True = dialog ini dipanggil dari chain, bukan distance
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Transform player;
    private CanvasGroup dialogCanvasGroup;
    private RectTransform dialogBoxRect;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (dialogBox != null)
        {
            dialogCanvasGroup = dialogBox.GetComponent<CanvasGroup>();
            if (dialogCanvasGroup == null)
                dialogCanvasGroup = dialogBox.AddComponent<CanvasGroup>();

            dialogBoxRect = dialogBox.GetComponent<RectTransform>();

            dialogCanvasGroup.alpha = 0;
            dialogBox.SetActive(false);
        }

        // Pastikan dialog chain berikutnya tidak auto-trigger sendiri di awal
        if (nextDialog != null)
            nextDialog.SetChained(true);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            if (debugMode) Debug.Log("[WorldDialog] Player found: " + playerObj.name);
        }
        else
        {
            Debug.LogError("[WorldDialog] Player not found! Tag player GameObject dengan 'Player'!");
        }
    }

    // Dipanggil dari luar untuk menandai dialog ini bagian dari chain
    public void SetChained(bool chained)
    {
        isChained = chained;
    }

    void Update()
    {
        if (player == null) return;

        // Dialog yang di-chain tidak auto-trigger dari distance
        if (isChained)
        {
            if (isDialogActive && followNPC)
                UpdateDialogPosition();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance && !isDialogActive)
        {
            if (showOnlyOnce && hasShownDialog) return;
            ShowDialog();
        }
        else if (distance > triggerDistance && isDialogActive)
        {
            HideDialog();
        }

        if (isDialogActive && followNPC)
            UpdateDialogPosition();
    }

    void UpdateDialogPosition()
    {
        if (dialogBoxRect == null || mainCamera == null) return;

        Vector3 worldPosition = transform.position + dialogOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        dialogBoxRect.position = screenPosition;
    }

    public void ShowDialog()
    {
        // Kalau tidak ada UI references, skip aja (optional)
        if (dialogBox == null || dialogText == null)
        {
            if (debugMode) Debug.Log("[WorldDialog] No UI references, skipping: " + npcName);

            // Tetap jalankan chain ke dialog berikutnya kalau ada
            if (dialogLines.Length > 0)
                StartCoroutine(PlayAllLines());
            return;
        }

        if (debugMode) Debug.Log("[WorldDialog] Showing dialog: " + npcName);

        isDialogActive = true;
        hasShownDialog = true;
        currentLineIndex = 0;

        dialogBox.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npcName;

        UpdateDialogPosition();
        StartCoroutine(FadeDialog(true));

        if (dialogLines.Length > 0)
            StartCoroutine(PlayAllLines());
    }

    // Mainkan semua baris satu per satu, lalu chain ke dialog berikutnya
    IEnumerator PlayAllLines()
    {
        for (int i = 0; i < dialogLines.Length; i++)
        {
            currentLineIndex = i;
            yield return StartCoroutine(TypeText(dialogLines[i]));

            if (i < dialogLines.Length - 1)
                yield return new WaitForSeconds(autoAdvanceDelay);
        }

        if (nextDialog != null)
        {
            if (debugMode) Debug.Log("[WorldDialog] Chaining ke: " + nextDialog.npcName);
            yield return new WaitForSeconds(delayBeforeNextDialog);

            // Jangan pakai HideDialog() di sini — nanti bunuh coroutine ini sendiri
            // Langsung fade out manual, lalu lanjut
            isDialogActive = false;
            yield return StartCoroutine(FadeDialog(false));

            nextDialog.ShowDialog();
        }
        else
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            HideDialog(); // Ini aman karena tidak ada yang perlu dilanjut setelahnya
        }
    }

    public void HideDialog()
    {
        if (!isDialogActive) return;

        if (debugMode) Debug.Log("[WorldDialog] Hiding dialog: " + npcName);

        isDialogActive = false;
        StopAllCoroutines();
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
                dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (fadeIn && followNPC)
                UpdateDialogPosition();

            yield return null;
        }

        if (dialogCanvasGroup != null)
            dialogCanvasGroup.alpha = targetAlpha;

        if (!fadeIn && dialogBox != null)
            dialogBox.SetActive(false);
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (debugMode) Debug.Log("[WorldDialog] Selesai: " + line);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        Gizmos.color = Color.cyan;
        Vector3 dialogPos = transform.position + dialogOffset;
        Gizmos.DrawWireCube(dialogPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position, dialogPos);

        // Gambar garis ke nextDialog kalau ada
        if (nextDialog != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, nextDialog.transform.position);
            Gizmos.DrawWireSphere(nextDialog.transform.position, 0.4f);
        }
    }
}