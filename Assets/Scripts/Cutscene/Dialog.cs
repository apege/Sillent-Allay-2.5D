using System.Collections;
using UnityEngine;
using TMPro;

public class Dialog : MonoBehaviour
{
    public TextMeshProUGUI textDisplay; 
    public string[] sentences;
    public float typingSpeed = 0.01f;

    [Header("Character Animators")]
    public Animator polisiAnimator;
    public Animator naraAnimator;
    public Animator buDeaAnimator;
    public Animator ibuNaraAnimator;

    private int index = 0; 
    private Coroutine typingCoroutine;
    private Animator currentTalkingAnimator;
    private bool isTyping = false; // Menandai apakah teks sedang mengetik

    void Start()
    {
        if (sentences.Length > 0)
        {
            textDisplay.text = "";
            typingCoroutine = StartCoroutine(Type());
        }
    }

    // MEMBUAT DETEKSI KLIK OTOMATIS LEWAT KEYBOARD / MOUSE
    void Update()
    {
        // Jika mouse kiri diklik ATAU tombol Spasi ditekan
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // Jika teks sudah selesai mengetik, lanjut ke kalimat berikutnya
            if (!isTyping)
            {
                NextSentence();
            }
            else 
            {
                // (Opsional) Jika diklik saat sedang mengetik, langsung munculkan semua teks
                StopCoroutine(typingCoroutine);
                textDisplay.text = sentences[index];
                isTyping = false;
                if (currentTalkingAnimator != null) currentTalkingAnimator.SetBool("isTalking", false);
            }
        }
    }

    public void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            textDisplay.text = "";
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            ResetAllTalkingAnimations();

            typingCoroutine = StartCoroutine(Type());
        }
        else
        {
            textDisplay.text = "";
            ResetAllTalkingAnimations();
        }
    }

    IEnumerator Type()
    {
        isTyping = true;
        string currentSentence = sentences[index];

        if (currentSentence.StartsWith("Polisi:")) currentTalkingAnimator = polisiAnimator;
        else if (currentSentence.StartsWith("Nara:")) currentTalkingAnimator = naraAnimator;
        else if (currentSentence.StartsWith("Bu Dea :") || currentSentence.StartsWith("Bu Dea:")) currentTalkingAnimator = buDeaAnimator;
        else if (currentSentence.StartsWith("Ibu Nara:")) currentTalkingAnimator = ibuNaraAnimator;
        else currentTalkingAnimator = null;

        if (currentTalkingAnimator != null)
        {
            currentTalkingAnimator.SetBool("isTalking", true);
        }

        foreach (char letter in currentSentence.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (currentTalkingAnimator != null)
        {
            currentTalkingAnimator.SetBool("isTalking", false);
        }
        
        isTyping = false;
    }

    void ResetAllTalkingAnimations()
    {
        if (polisiAnimator != null) polisiAnimator.SetBool("isTalking", false);
        if (naraAnimator != null) naraAnimator.SetBool("isTalking", false);
        if (buDeaAnimator != null) buDeaAnimator.SetBool("isTalking", false);
        if (ibuNaraAnimator != null) ibuNaraAnimator.SetBool("isTalking", false);
    }
}
