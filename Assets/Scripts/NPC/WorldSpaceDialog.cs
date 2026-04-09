using UnityEngine;
using TMPro;
using System.Collections;

// DIALOG WORLD SPACE: Muncul di atas kepala NPC kayak speech bubble
// Ngikutin posisi NPC di world space

public class WorldSpaceDialog : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 6)]
    public string[] dialogLines;
    public float typingSpeed = 0.05f;
    public bool showOnlyOnce = false;
    
    [Header("UI References")]
    public GameObject dialogBox; // DialogBox di Canvas
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;
    public string npcName = "NPC";
    
    [Header("World Position Settings")]
    public Vector3 dialogOffset = new Vector3(0, 2.5f, 0); // Offset dari NPC (naik 2.5 unit)
    public bool followNPC = true; // Dialog ikutin NPC atau fixed position
    
    [Header("Trigger Settings")]
    public float triggerDistance = 5f;
    public bool debugMode = true;
    
    private bool isDialogActive = false;
    private bool hasShownDialog = false;
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
            {
                dialogCanvasGroup = dialogBox.AddComponent<CanvasGroup>();
            }
            
            dialogBoxRect = dialogBox.GetComponent<RectTransform>();
            
            dialogCanvasGroup.alpha = 0;
            dialogBox.SetActive(false);
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            if (debugMode) Debug.Log("[WorldDialog] Player found: " + playerObj.name);
        }
        else
        {
            Debug.LogError("[WorldDialog] Player not found! Set tag 'Player' on player GameObject!");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= triggerDistance && !isDialogActive)
        {
            if (showOnlyOnce && hasShownDialog)
                return;
                
            ShowDialog();
        }
        else if (distance > triggerDistance && isDialogActive)
        {
            HideDialog();
        }
        
        // Update posisi dialog box biar ngikutin NPC
        if (isDialogActive && followNPC)
        {
            UpdateDialogPosition();
        }
    }
    
    void UpdateDialogPosition()
    {
        if (dialogBoxRect == null || mainCamera == null) return;
        
        // Posisi NPC di world + offset (di atas kepala)
        Vector3 worldPosition = transform.position + dialogOffset;
        
        // Convert world position ke screen position
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Set posisi dialog box
        dialogBoxRect.position = screenPosition;
    }
    
    void ShowDialog()
    {
        if (dialogBox == null || dialogText == null)
        {
            Debug.LogError("[WorldDialog] UI references NULL!");
            return;
        }
        
        if (debugMode) Debug.Log("[WorldDialog] Showing dialog");
        
        isDialogActive = true;
        hasShownDialog = true;
        currentLineIndex = 0;
        
        dialogBox.SetActive(true);
        
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        
        // Set posisi awal
        UpdateDialogPosition();
        
        // Fade in
        StartCoroutine(FadeDialog(true));
        
        // Show first line
        if (dialogLines.Length > 0)
        {
            StartCoroutine(TypeText(dialogLines[currentLineIndex]));
        }
    }
    
    void HideDialog()
    {
        if (!isDialogActive) return;
        
        if (debugMode) Debug.Log("[WorldDialog] Hiding dialog");
        
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
            {
                dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            }
            
            // Keep updating position during fade in
            if (fadeIn && followNPC)
            {
                UpdateDialogPosition();
            }
            
            yield return null;
        }
        
        if (dialogCanvasGroup != null)
        {
            dialogCanvasGroup.alpha = targetAlpha;
        }
        
        if (!fadeIn && dialogBox != null)
        {
            dialogBox.SetActive(false);
        }
    }
    
    IEnumerator TypeText(string line)
    {
        dialogText.text = "";
        
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        if (debugMode) Debug.Log("[WorldDialog] Typing complete");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw trigger radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        
        // Draw dialog position indicator
        Gizmos.color = Color.cyan;
        Vector3 dialogPos = transform.position + dialogOffset;
        Gizmos.DrawWireCube(dialogPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position, dialogPos);
    }
}
