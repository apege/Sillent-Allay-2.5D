using System.Collections;
using UnityEngine;
using TMPro;

public class ketik : MonoBehaviour
{
    public TextMeshProUGUI teksTMP;
    public AudioSource sfxNangis;
    public float kecepatanKetik = 0.05f;

    [Header("Pengaturan Transisi")]
    public GameObject kotakDialogUI;
    public GameObject kameraCutscene;
    public GameObject playerUtama;

    private string pesanPenuh;

    void Start()
    {
        pesanPenuh = teksTMP.text;
        teksTMP.text = "";

        if (sfxNangis != null)
        {
            sfxNangis.Play();
        }

        StartCoroutine(MulaiNgetik());
    }

    IEnumerator MulaiNgetik()
    {
        // Ngetik teks
        foreach (char huruf in pesanPenuh.ToCharArray())
        {
            teksTMP.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        // Tunggu sebentar
        yield return new WaitForSeconds(2.0f);

        // Sembunyikan UI
        if (kotakDialogUI != null)
            kotakDialogUI.SetActive(false);

        if (kameraCutscene != null)
            kameraCutscene.SetActive(false);

        // Aktifkan movement player
        if (playerUtama != null)
        {
            var skripJalan = playerUtama.GetComponent("PlayerMovement") as MonoBehaviour;

            if (skripJalan != null)
                skripJalan.enabled = true;
        }
    }
}