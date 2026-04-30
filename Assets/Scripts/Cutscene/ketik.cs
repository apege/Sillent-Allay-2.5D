using System.Collections;
using UnityEngine;
using TMPro;

public class ketik : MonoBehaviour
{
    public TextMeshProUGUI teksTMP;
    public AudioSource sfxNangis; 
    public float kecepatanKetik = 0.05f;
    
    [Header("Pengaturan Transisi")]
    public GameObject kotakDialogUI; // Seret objek 'Image' (kotak hitam) ke sini
    public GameObject kameraCutscene; // Seret kamera yang buat nangis ke sini
    public GameObject playerUtama;    // Seret objek 'Player' kamu ke sini
    
    private string pesanPenuh;

    void Start()
    {
        pesanPenuh = teksTMP.text;
        teksTMP.text = "";
        
        if (sfxNangis != null) {
            sfxNangis.Play();
        }
        
        StartCoroutine(MulaiNgetik());
    }

    IEnumerator MulaiNgetik()
    {
        // 1. Ngetik kalimat pertama
        foreach (char huruf in pesanPenuh.ToCharArray())
        {
            teksTMP.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        // 2. JEDA DIAM SEBENTAR
        yield return new WaitForSeconds(2.0f);

        // 3. HAPUS SEMUA TEKS LAMA
        teksTMP.text = ""; 

        // 4. JEDA LAGI BENTAR
        yield return new WaitForSeconds(0.5f);

        // 5. NGETIK KALIMAT TERAKHIR
        string kalimatTerakhir = "Aku capek gini terus..."; 
        foreach (char huruf in kalimatTerakhir.ToCharArray())
        {
            teksTMP.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        // 6. SELESAI & BALIK KE PLAYER
        yield return new WaitForSeconds(2.0f); // Kasih waktu baca kalimat terakhir

        if (kotakDialogUI != null) kotakDialogUI.SetActive(false);
        if (kameraCutscene != null) kameraCutscene.SetActive(false);
        
        // Menyalakan skrip jalan player (Ganti 'PlayerMovement' sesuai nama skrip kamu)
        if (playerUtama != null) {
            // Contoh: playerUtama.GetComponent<PlayerMovement>().enabled = true;
            // Jika kamu pakai FirstPersonController, ganti namanya di bawah ini:
            var skripJalan = playerUtama.GetComponent("PlayerMovement") as MonoBehaviour; 
            if (skripJalan != null) skripJalan.enabled = true;
        }
    }
}
