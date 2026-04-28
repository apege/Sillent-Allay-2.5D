using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;

// SCRIPT ADVANCED: Dialog dengan pilihan jawaban + Sound FX
// Pake ini kalo mau fitur lebih kompleks

[System.Serializable]
public class DialogLine
{
    [TextArea(2, 4)]
    public string text;
    public AudioClip voiceClip; // Opsional: voice acting
    public Sprite characterPortrait; // Opsional: gambar portrait
}

[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public UnityEvent onChoiceSelected; // Event kalo pilihan dipilih
    public int nextDialogIndex = -1; // Index dialog selanjutnya, -1 = end
}

public class AdvancedNPCDialog : MonoBehaviour
{
    [Header("Dialog Content")]
    public DialogLine[] dialogLines;
    public bool hasChoices = false;
    public DialogChoice[] choices; // Pilihan jawaban
    
    [Header("UI References")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;
    public Image portraitImage;
    public GameObject choiceButtonPrefab; // Prefab untuk button pilihan
    public Transform choiceContainer; // Parent untuk spawn buttons
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dialogOpenSound;
    public AudioClip dialogCloseSound;
    public AudioClip typingSound; // Sound tiap huruf
    public AudioClip choiceSelectSound;
    [Range(0f, 1f)]
    public float typingSoundVolume = 0.3f;
    
    [Header("Settings")]
    public string npcName = "NPC";
    public float typingSpeed = 0.05f;
    public float triggerDistance = 3f;
    public bool showOnlyOnce = false;
    
    private bool isDialogActive = false;
    private bool hasShownDialog = false;
    private int currentLineIndex = 0;
    private Transform player;
    private CanvasGroup dialogCanvasGroup;
    private Coroutine typingCoroutine;
    private bool isWaitingForChoice = false;
    
    void Start()
    {
        if (dialogBox != null)
        {
            dialogCanvasGroup = dialogBox.GetComponent<CanvasGroup>();
            if (dialogCanvasGroup == null)
            {
                dialogCanvasGroup = dialogBox.AddComponent<CanvasGroup>();
            }
            dialogCanvasGroup.alpha = 0;
            dialogBox.SetActive(false);
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
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
        
        // Input untuk advance dialog
        if (isDialogActive && Input.GetKeyDown(KeyCode.E) && !isWaitingForChoice)
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
        isWaitingForChoice = false;
        
        dialogBox.SetActive(true);
        
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        
        // Play sound
        if (dialogOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogOpenSound);
        }
        
        StartCoroutine(AnimateDialogBox(true));
        
        if (dialogLines.Length > 0)
        {
            DisplayDialogLine(currentLineIndex);
        }
    }
    
    void HideDialog()
    {
        if (!isDialogActive) return;
        
        isDialogActive = false;
        isWaitingForChoice = false;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Play sound
        if (dialogCloseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogCloseSound);
        }
        
        ClearChoices();
        StartCoroutine(AnimateDialogBox(false));
    }
    
    IEnumerator AnimateDialogBox(bool show)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = show ? 0f : 1f;
        float targetAlpha = show ? 1f : 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        
        dialogCanvasGroup.alpha = targetAlpha;
        
        if (!show)
        {
            dialogBox.SetActive(false);
        }
    }
    
    void DisplayDialogLine(int index)
    {
        if (index >= dialogLines.Length) return;
        
        DialogLine line = dialogLines[index];
        
        // Update portrait kalo ada
        if (portraitImage != null && line.characterPortrait != null)
        {
            portraitImage.sprite = line.characterPortrait;
            portraitImage.enabled = true;
        }
        else if (portraitImage != null)
        {
            portraitImage.enabled = false;
        }
        
        // Play voice clip kalo ada
        if (line.voiceClip != null && audioSource != null)
        {
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }
        
        StartTyping(line.text);
    }
    
    void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        
        foreach (char c in text)
        {
            dialogText.text += c;
            
            // Play typing sound
            if (typingSound != null && audioSource != null && c != ' ')
            {
                audioSource.PlayOneShot(typingSound, typingSoundVolume);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
        
        // Cek apakah ini dialog terakhir dan ada choices
        if (currentLineIndex == dialogLines.Length - 1 && hasChoices)
        {
            ShowChoices();
        }
    }
    
    void NextDialogLine()
    {
        // Skip typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogText.text = dialogLines[currentLineIndex].text;
            typingCoroutine = null;
            
            // Cek choices
            if (currentLineIndex == dialogLines.Length - 1 && hasChoices)
            {
                ShowChoices();
            }
            return;
        }
        
        // Next line
        currentLineIndex++;
        if (currentLineIndex < dialogLines.Length)
        {
            DisplayDialogLine(currentLineIndex);
        }
        else if (hasChoices)
        {
            ShowChoices();
        }
        else
        {
            currentLineIndex = 0;
        }
    }
    
    void ShowChoices()
    {
        if (choiceContainer == null || choiceButtonPrefab == null) return;
        
        isWaitingForChoice = true;
        ClearChoices();
        
        foreach (DialogChoice choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }
            
            // Setup button click
            DialogChoice capturedChoice = choice; // Capture untuk closure
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }
    
    void OnChoiceSelected(DialogChoice choice)
    {
        // Play sound
        if (choiceSelectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(choiceSelectSound);
        }
        
        // Invoke event
        choice.onChoiceSelected?.Invoke();
        
        // Hide dialog atau lanjut ke dialog lain
        if (choice.nextDialogIndex >= 0 && choice.nextDialogIndex < dialogLines.Length)
        {
            ClearChoices();
            isWaitingForChoice = false;
            currentLineIndex = choice.nextDialogIndex;
            DisplayDialogLine(currentLineIndex);
        }
        else
        {
            HideDialog();
        }
    }
    
    void ClearChoices()
    {
        if (choiceContainer == null) return;
        
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
