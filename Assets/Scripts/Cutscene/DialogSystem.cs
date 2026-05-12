using UnityEngine;
using TMPro; // Penting untuk TextMeshPro
using System.Collections;

public class DialogSystem : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public string[] sentences;
    private int index;
    public float typingSpeed = 0.05f;

    void Start()
    {
        // Mulai dialog pertama
        StartCoroutine(Type());
    }

    void Update()
    {
        // Klik kiri mouse atau tekan Space untuk lanjut
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (textDisplay.text == sentences[index])
            {
                NextSentence();
            }
            else
            {
                // Skip animasi ngetik (langsung muncul semua teks)
                StopAllCoroutines();
                textDisplay.text = sentences[index];
            }
        }
    }

    IEnumerator Type()
    {
        textDisplay.text = "";
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;
            StartCoroutine(Type());
        }
        else
        {
            textDisplay.text = ""; // Dialog habis
            Debug.Log("Dialog Selesai");
        }
    }
}
