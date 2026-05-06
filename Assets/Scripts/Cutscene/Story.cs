using UnityEngine;
using TMPro;
using System.Collections;

public class Story : MonoBehaviour
{
    [Header("UI Nara (Subtitle Bawah)")]
    public GameObject canvasNara;
    public TextMeshProUGUI teksNara;

    [Header("UI NPC (Balon Teks)")]
    public GameObject dialogBoxNPC; 
    public TextMeshProUGUI teksNPC; 

    public float kecepatanKetik = 0.05f;

    void Start()
    {
        // Reset awal: semua teks kosong dan UI mati
        if(teksNara != null) teksNara.text = "";
        if(teksNPC != null) teksNPC.text = "";
        
        if(canvasNara != null) canvasNara.SetActive(false);
        if(dialogBoxNPC != null) dialogBoxNPC.SetActive(false);

        // Jalankan urutan cerita
        StartCoroutine(MulaiCerita());
    }

    IEnumerator MulaiCerita()
    {
        // 1. Nara Bicara (Bawah)
        if(canvasNara != null) canvasNara.SetActive(true);
        yield return StartCoroutine(EfekKetik(teksNara, "Naya! Bareng aku dong pulangnya."));
        yield return new WaitForSeconds(2.5f);
        if(canvasNara != null) canvasNara.SetActive(false);

// 2. NPC Bicara (Balon)
        if(dialogBoxNPC != null) dialogBoxNPC.SetActive(true);
        yield return StartCoroutine(EfekKetik(teksNPC, "Eh sorry Nara, aku harus eskul. Duluan ya!"));
        yield return new WaitForSeconds(2.5f);
        if(dialogBoxNPC != null) dialogBoxNPC.SetActive(false);

        // 3. Nara Bicara Lagi (Bawah)
        if(canvasNara != null) canvasNara.SetActive(true);
        yield return StartCoroutine(EfekKetik(teksNara, "Oh… oke deh."));
        yield return new WaitForSeconds(2f);
        if(canvasNara != null) canvasNara.SetActive(false);

        // --- JEDA (Saat ini NPC jalan pergi di Timeline) ---
        yield return new WaitForSeconds(3f); 

        // 4. Monolog Nara (Bawah)
        if(canvasNara != null) canvasNara.SetActive(true);
        yield return StartCoroutine(EfekKetik(teksNara, "Yahh. Sendirian lagi."));
        yield return new WaitForSeconds(3f);
        if(canvasNara != null) canvasNara.SetActive(false);
    }

    IEnumerator EfekKetik(TextMeshProUGUI targetTMP, string kalimat)
    {
        if (targetTMP == null) yield break;
        
        targetTMP.text = "";
        foreach (char huruf in kalimat.ToCharArray())
        {
            targetTMP.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
    }
}
