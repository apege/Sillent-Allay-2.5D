using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NPCDialogSystem : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 6)]
    public string[] dialogLines; // Dialog yang mau ditampilin
    public float typingSpeed = 0.05f; // Kecepatan typewriter effect
    public bool showOnlyOnce = false; // Dialog cuma muncul sekali doang
    
    [Header("UI References")]
    public GameObject dialogBox; // Panel dialog
    public TextMeshProUGUI dialogText; // Text untuk dialog
    public TextMeshProUGUI npcNameText; // Text untuk nama NPC
    public string npcName = "NPC"; // Nama NPC
    
    [Header("Animation Settings")]
    public float fadeSpeed = 2f; // Kecepatan fade in/out
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curve untuk scale animation
    public float animationDuration = 0.3f; // Durasi animasi muncul
    
    [Header("Trigger Settings")]
    public float triggerDistance = 3f; // Jarak player buat trigger dialog
    public LayerMask playerLayer; // Layer untuk player
    
    private bool isDialogActive = false;
    private bool hasShownDialog = false;
    private int currentLineIndex = 0;
    private Transform player;
    private CanvasGroup dialogCanvasGroup;
    private Coroutine typingCoroutine;
    private RectTransform dialogBoxRect;
    
    void Start()
    {
        // Setup canvas group buat fade effect
        if (dialogBox != null)
        {
            dialogCanvasGroup = dialogBox.GetComponent<CanvasGroup>();
            if (dialogCanvasGroup == null)
            {
                dialogCanvasGroup = dialogBox.AddComponent<CanvasGroup>();
            }
            
            dialogBoxRect = dialogBox.GetComponent<RectTransform>();
            
            // Hide dialog di awal
            dialogCanvasGroup.alpha = 0;
            dialogBox.SetActive(false);
        }
        
        // Cari player (sesuaiin sama tag player lu)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Cek jarak player ke NPC
        float distance = Vector3.Distance(transform.position, player.position);
        
        // Kalo player deket dan dialog belum muncul
        if (distance <= triggerDistance && !isDialogActive)
        {
            // Kalo showOnlyOnce = true, cek udah pernah muncul belum
            if (showOnlyOnce && hasShownDialog)
                return;
                
            ShowDialog();
        }
        // Kalo player menjauh, hide dialog
        else if (distance > triggerDistance && isDialogActive)
        {
            HideDialog();
        }
        
        // Input buat next dialog (opsional, kalo mau manual advance)
        if (isDialogActive && Input.GetKeyDown(KeyCode.E))
        {
            NextDialogLine();
        }
    }
    
    void ShowDialog()
    {
        if (dialogBox == null) return;
        
        isDialogActive = true;
        hasShownDialog = true;
        currentLineIndex = 0;
        
        dialogBox.SetActive(true);
        
        // Set nama NPC
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        
        // Animate dialog box muncul
        StartCoroutine(AnimateDialogBox(true));
        
        // Tampilin dialog pertama
        if (dialogLines.Length > 0)
        {
            StartTyping(dialogLines[currentLineIndex]);
        }
    }
    
    void HideDialog()
    {
        if (!isDialogActive) return;
        
        isDialogActive = false;
        
        // Stop typing kalo masih jalan
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Animate dialog box hilang
        StartCoroutine(AnimateDialogBox(false));
    }
    
    IEnumerator AnimateDialogBox(bool show)
    {
        float elapsed = 0f;
        float startAlpha = show ? 0f : 1f;
        float targetAlpha = show ? 1f : 0f;
        Vector3 startScale = show ? Vector3.zero : Vector3.one;
        Vector3 targetScale = show ? Vector3.one : Vector3.zero;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Fade
            dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            // Scale dengan curve
            float curveValue = scaleCurve.Evaluate(t);
            if (dialogBoxRect != null)
            {
                dialogBoxRect.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            }
            
            yield return null;
        }
        
        // Pastiin nilai final
        dialogCanvasGroup.alpha = targetAlpha;
        if (dialogBoxRect != null)
        {
            dialogBoxRect.localScale = targetScale;
        }
        
        // Kalo hide, nonaktifin gameobject
        if (!show)
        {
            dialogBox.SetActive(false);
        }
    }
    
    void StartTyping(string line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(line));
    }
    
    IEnumerator TypeText(string line)
    {
        dialogText.text = "";
        
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
    }
    
    void NextDialogLine()
    {
        // Skip typing kalo lagi jalan
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = dialogLines[currentLineIndex];
            typingCoroutine = null;
            return;
        }
        
        // Next line
        currentLineIndex++;
        if (currentLineIndex < dialogLines.Length)
        {
            StartTyping(dialogLines[currentLineIndex]);
        }
        else
        {
            // Udah abis semua dialog
            currentLineIndex = 0; // Reset buat next trigger
        }
    }
    
    // Visual debug di Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
