using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI tmpText;
    private Coroutine typingCoroutine;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    // Fungsi ini yang akan dipanggil oleh sistem Cutscene Timeline
    public void TampilkanTeks(string naskahLengkap)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(KetikTeks(naskahLengkap));
    }

    private IEnumerator KetikTeks(string naskah)
    {
        tmpText.text = "";
        foreach (char huruf in naskah.ToCharArray())
        {
            tmpText.text += huruf;
            yield return new WaitForSeconds(0.05f); // Jeda kecepatan ketik per huruf
        }
    }
    
    public void KosongkanTeks()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (tmpText != null) tmpText.text = "";
    }
}
