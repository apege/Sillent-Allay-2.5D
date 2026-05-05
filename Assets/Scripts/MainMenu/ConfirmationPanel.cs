using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ============================================================
//  ConfirmationPanel.cs
//  Script ringan untuk ngisi data di PanelConfirmation.
//  TIDAK rebuild/destroy apapun — desain kamu tetap aman!
//
//  CARA PAKAI:
//  1. Attach script ini ke GameObject "PanelConfirmation"
//  2. Drag semua slot teks & tombol di Inspector
//  3. Selesai — data otomatis terisi saat panel muncul
// ============================================================

public class ConfirmationPanel : MonoBehaviour
{
    // ----------------------------------------------------------
    // SLOT TEKS — drag dari dalam PanelConfirmation
    // ----------------------------------------------------------
    [Header("=== Teks ===")]
    public TMP_Text txtCharacterName;   // Nama karakter (Nara / Raka)
    public TMP_Text txtTraitName;       // Nama trait (Introvert, dll)

    [Header("=== Badge Efek ===")]
    public Image    badge1Image;        // Background badge 1
    public TMP_Text badge1Text;         // Teks badge 1
    public Image    badge2Image;        // Background badge 2
    public TMP_Text badge2Text;         // Teks badge 2

    [Header("=== Avatar ===")]
    public Image imgAvatar;             // Foto karakter (opsional)

    // ----------------------------------------------------------
    // TOMBOL
    // ----------------------------------------------------------
    [Header("=== Tombol ===")]
    public Button btnMulai;             // Tombol "Mulai Game"
    public Button btnKembali;           // Tombol "Kembali"

    // ----------------------------------------------------------
    // WARNA BADGE
    // ----------------------------------------------------------
    [Header("=== Warna Badge ===")]
    public Color badgeColorPositif = new Color(0.37f, 0.61f, 0.13f, 0.25f);
    public Color badgeColorNegatif = new Color(0.78f, 0.23f, 0.23f, 0.25f);
    public Color badgeColorNetral  = new Color(0.5f,  0.5f,  0.5f,  0.25f);
    public Color textColorPositif  = new Color(0.55f, 0.80f, 0.33f, 1f);
    public Color textColorNegatif  = new Color(0.88f, 0.44f, 0.44f, 1f);
    public Color textColorNetral   = new Color(0.70f, 0.70f, 0.70f, 1f);

    // ----------------------------------------------------------
    // SCENE
    // ----------------------------------------------------------
    [Header("=== Scene ===")]
    public string cutsceneSceneName = "Cutscene";

    // ============================================================
    //  START — pasang listener tombol
    // ============================================================
    private void Start()
    {
        btnMulai?.onClick.AddListener(OnMulai);
        btnKembali?.onClick.AddListener(OnKembali);
    }

    // ============================================================
    //  TAMPILKAN PANEL — dipanggil dari CharacterSelectManager
    // ============================================================
    public void ShowPanel(string characterName, string traitName,
                          string b1Txt, bool b1Positif,
                          string b2Txt, bool b2Positif,
                          Sprite avatarSprite = null)
    {
        gameObject.SetActive(true);

        // Isi teks
        if (txtCharacterName != null) txtCharacterName.text = characterName;
        if (txtTraitName     != null) txtTraitName.text     = traitName;

        // Isi badge 1
        SetBadge(badge1Image, badge1Text, b1Txt, b1Positif ? "g" : "r");

        // Isi badge 2
        SetBadge(badge2Image, badge2Text, b2Txt, b2Positif ? "g" : "r");

        // Avatar
        if (imgAvatar != null && avatarSprite != null)
            imgAvatar.sprite = avatarSprite;
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // ============================================================
    //  SET BADGE
    // ============================================================
    private void SetBadge(Image img, TMP_Text txt, string text, string colorCode)
    {
        if (img == null || txt == null) return;

        txt.text = text;

        switch (colorCode)
        {
            case "g":
                img.color = badgeColorPositif;
                txt.color = textColorPositif;
                break;
            case "r":
                img.color = badgeColorNegatif;
                txt.color = textColorNegatif;
                break;
            default:
                img.color = badgeColorNetral;
                txt.color = textColorNetral;
                break;
        }
    }

    // ============================================================
    //  TOMBOL
    // ============================================================
    private void OnMulai()
    {
        // Simpan data & load scene
        var manager = Object.FindFirstObjectByType<CharacterSelectManager>();
        manager?.OnConfirmationStart();
    }

    private void OnKembali()
    {
        HidePanel();
        var manager = Object.FindFirstObjectByType<CharacterSelectManager>();
        manager?.OnConfirmationBack();
    }
}