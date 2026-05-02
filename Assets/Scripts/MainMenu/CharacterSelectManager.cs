using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ============================================================
//  CharacterSelectManager.cs (Updated — dengan Badge Efek)
// ============================================================

public class CharacterSelectManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // PANEL UI
    // ----------------------------------------------------------
    [Header("=== Panel UI ===")]
    public GameObject panelCharacterSelect;
    public GameObject panelTraitSelect;
    public GameObject panelConfirmation; // tidak wajib, bisa kosong

    [Header("=== Auto Builder ===")]
    [Tooltip("Drag GameObject yang ada ConfirmationPanelBuilder.cs")]
    public ConfirmationPanelBuilder confirmationBuilder;

    // ----------------------------------------------------------
    // KOTAK KARAKTER
    // ----------------------------------------------------------
    [Header("=== Karakter ===")]
    public Button btnNara;
    public Button btnRaka;
    public Image  naraImage;
    public Image  rakaImage;
    public Sprite naraSprite;
    public Sprite rakaSprite;

    public Color colorSelected   = new Color(1f, 0.85f, 0.4f);
    public Color colorUnselected = new Color(1f, 1f, 1f, 0.5f);

    // ----------------------------------------------------------
    // TRAIT BUTTONS
    // ----------------------------------------------------------
    [Header("=== Trait Buttons ===")]
    public Button btnIntrovert;
    public Button btnEmpatik;
    public Button btnPenakut;
    public Button btnPemberani;
    public Button btnSensitif;

    // ----------------------------------------------------------
    // INFO TRAIT
    // ----------------------------------------------------------
    [Header("=== Info Trait ===")]
    public TMP_Text txtTraitName;
    public TMP_Text txtTraitDescription;

    // ----------------------------------------------------------
    // BADGE EFEK
    // Drag Badge1 dan Badge2 (Image component) ke sini
    // ----------------------------------------------------------
    [Header("=== Badge Efek ===")]
    public Image    badge1Image;      // Image komponen Badge1
    public TMP_Text badge1Text;       // TMP_Text di dalam Badge1
    public Image    badge2Image;      // Image komponen Badge2
    public TMP_Text badge2Text;       // TMP_Text di dalam Badge2

    [Tooltip("Sprite badge warna hijau (efek positif)")]
    public Sprite badgeSpriteGreen;
    [Tooltip("Sprite badge warna merah (efek negatif)")]
    public Sprite badgeSpriteRed;
    [Tooltip("Sprite badge warna abu (efek netral)")]
    public Sprite badgeSpriteGray;

    // Warna teks badge
    private readonly Color _colorTextGreen = new Color(0.23f, 0.43f, 0.07f); // #3B6D11
    private readonly Color _colorTextRed   = new Color(0.64f, 0.18f, 0.18f); // #A32D2D
    private readonly Color _colorTextGray  = new Color(0.37f, 0.37f, 0.35f); // #5F5E5A

    // ----------------------------------------------------------
    // PANEL KONFIRMASI — dihandle oleh ConfirmationPanelBuilder
    // ----------------------------------------------------------

    // ----------------------------------------------------------
    // SCENE
    // ----------------------------------------------------------
    [Header("=== Scene ===")]
    public string cutsceneSceneName = "cutscene";

    // ----------------------------------------------------------
    // INTERNAL
    // ----------------------------------------------------------
    private GameData.CharacterType _selectedCharacter = GameData.CharacterType.None;
    private GameData.TraitType     _selectedTrait     = GameData.TraitType.None;

    // ============================================================
    //  START
    // ============================================================
    private void Start()
    {
        HideAllPanels();

        btnNara?.onClick.AddListener(() => SelectCharacter(GameData.CharacterType.Nara));
        btnRaka?.onClick.AddListener(() => SelectCharacter(GameData.CharacterType.Raka));

        btnIntrovert?.onClick.AddListener(() => SelectTrait(GameData.TraitType.Introvert));
        btnEmpatik?.onClick.AddListener(()   => SelectTrait(GameData.TraitType.Empatik));
        btnPenakut?.onClick.AddListener(()   => SelectTrait(GameData.TraitType.Penakut));
        btnPemberani?.onClick.AddListener(() => SelectTrait(GameData.TraitType.Pemberani));
        btnSensitif?.onClick.AddListener(()  => SelectTrait(GameData.TraitType.Sensitif));

        // Tombol konfirmasi dihandle oleh ConfirmationPanelBuilder
    }

    // ============================================================
    //  BUKA PANEL KARAKTER (dipanggil dari tombol GAME BARU)
    // ============================================================
    public void OpenCharacterSelect()
    {
        _selectedCharacter = GameData.CharacterType.None;
        _selectedTrait     = GameData.TraitType.None;

        HideAllPanels();
        panelCharacterSelect?.SetActive(true);
        UpdateCharacterHighlight();
    }

    // ============================================================
    //  PILIH KARAKTER
    // ============================================================
    private void SelectCharacter(GameData.CharacterType character)
    {
        _selectedCharacter = character;
        Debug.Log($"[CharacterSelect] Karakter dipilih: {character}");
        UpdateCharacterHighlight();
        Invoke(nameof(GoToTraitSelect), 0.3f);
    }

    private void UpdateCharacterHighlight()
    {
        if (naraImage != null)
            naraImage.color = (_selectedCharacter == GameData.CharacterType.Nara)
                ? colorSelected : colorUnselected;

        if (rakaImage != null)
            rakaImage.color = (_selectedCharacter == GameData.CharacterType.Raka)
                ? colorSelected : colorUnselected;
    }

    private void GoToTraitSelect()
    {
        panelCharacterSelect?.SetActive(false);
        panelTraitSelect?.SetActive(true);

        // Reset teks info
        if (txtTraitName != null)        txtTraitName.text = "Pilih Sifatmu";
        if (txtTraitDescription != null) txtTraitDescription.text = "Klik salah satu sifat untuk melihat detailnya.";

        // Sembunyikan badge sampai trait diklik
        badge1Image?.gameObject.SetActive(false);
        badge2Image?.gameObject.SetActive(false);
    }

    // ============================================================
    //  PILIH TRAIT
    // ============================================================
    private void SelectTrait(GameData.TraitType trait)
    {
        _selectedTrait = trait;
        Debug.Log($"[CharacterSelect] Trait dipilih: {trait}");

        // Update nama dan deskripsi
        if (txtTraitName != null)
            txtTraitName.text = trait.ToString();

        if (txtTraitDescription != null && GameData.Instance != null)
            txtTraitDescription.text = GameData.Instance.GetTraitDescription(trait);

        // Update badge efek
        UpdateBadges(trait);

        // Pindah ke konfirmasi setelah 0.8 detik
        Invoke(nameof(GoToConfirmation), 0.8f);
    }

    // ============================================================
    //  UPDATE BADGE — otomatis ganti teks, sprite, warna
    // ============================================================
    private void UpdateBadges(GameData.TraitType trait)
    {
        string b1Text, b2Text, b1Color, b2Color;

        switch (trait)
        {
            case GameData.TraitType.Introvert:
                b1Text = "Healing +30%";      b1Color = "g";
                b2Text = "Courage lambat";    b2Color = "r";
                break;
            case GameData.TraitType.Empatik:
                b1Text = "Healing +50%";      b1Color = "g";
                b2Text = "Sosial kuat";       b2Color = "g";
                break;
            case GameData.TraitType.Penakut:
                b1Text = "Trauma +50%";       b1Color = "r";
                b2Text = "Courage berkurang"; b2Color = "r";
                break;
            case GameData.TraitType.Pemberani:
                b1Text = "Courage +20";       b1Color = "g";
                b2Text = "Trauma -20%";       b2Color = "g";
                break;
            case GameData.TraitType.Sensitif:
                b1Text = "Sanity fluktuatif"; b1Color = "a";
                b2Text = "Pulih cepat";       b2Color = "g";
                break;
            default:
                badge1Image?.gameObject.SetActive(false);
                badge2Image?.gameObject.SetActive(false);
                return;
        }

        SetBadge(badge1Image, badge1Text, b1Text, b1Color);
        SetBadge(badge2Image, badge2Text, b2Text, b2Color);
    }

    /// <summary>
    /// Terapkan sprite, teks, dan warna ke satu badge.
    /// colorCode: "g" = hijau, "r" = merah, "a" = abu
    /// </summary>
    private void SetBadge(Image img, TMP_Text txt, string text, string colorCode)
    {
        if (img == null || txt == null) return;

        img.gameObject.SetActive(true);
        txt.text = text;

        switch (colorCode)
        {
            case "g":
                if (badgeSpriteGreen != null) img.sprite = badgeSpriteGreen;
                txt.color = _colorTextGreen;
                break;
            case "r":
                if (badgeSpriteRed != null) img.sprite = badgeSpriteRed;
                txt.color = _colorTextRed;
                break;
            default: // "a" = abu
                if (badgeSpriteGray != null) img.sprite = badgeSpriteGray;
                txt.color = _colorTextGray;
                break;
        }
    }

    // ============================================================
    //  PANEL KONFIRMASI
    // ============================================================
    private void GoToConfirmation()
    {
        panelTraitSelect?.SetActive(false);

        // Ambil data badge sesuai trait
        GetBadgeData(_selectedTrait,
            out string b1Txt, out bool b1Pos,
            out string b2Txt, out bool b2Pos);

        // Ambil sprite karakter
        Sprite avatarSprite = (_selectedCharacter == GameData.CharacterType.Nara)
            ? naraSprite : rakaSprite;

        // Tampilkan panel via builder
        if (confirmationBuilder != null)
        {
            confirmationBuilder.ShowPanel(
                _selectedCharacter.ToString(),
                _selectedTrait.ToString(),
                b1Txt, b1Pos, b2Txt, b2Pos,
                avatarSprite
            );
        }
        else
        {
            // Fallback: pakai panel manual kalau builder tidak ada
            panelConfirmation?.SetActive(true);
        }
    }

    // Data badge per trait
    private void GetBadgeData(GameData.TraitType trait,
        out string b1Txt, out bool b1Pos,
        out string b2Txt, out bool b2Pos)
    {
        switch (trait)
        {
            case GameData.TraitType.Introvert:
                b1Txt="Healing +30%"; b1Pos=true;
                b2Txt="Courage lambat"; b2Pos=false; break;
            case GameData.TraitType.Empatik:
                b1Txt="Healing +50%"; b1Pos=true;
                b2Txt="Sosial kuat"; b2Pos=true; break;
            case GameData.TraitType.Penakut:
                b1Txt="Trauma +50%"; b1Pos=false;
                b2Txt="Courage berkurang"; b2Pos=false; break;
            case GameData.TraitType.Pemberani:
                b1Txt="Courage +20"; b1Pos=true;
                b2Txt="Trauma -20%"; b2Pos=true; break;
            case GameData.TraitType.Sensitif:
                b1Txt="Sanity fluktuatif"; b1Pos=false;
                b2Txt="Pulih cepat"; b2Pos=true; break;
            default:
                b1Txt=""; b1Pos=true;
                b2Txt=""; b2Pos=true; break;
        }
    }

    // ============================================================
    //  KONFIRMASI → SIMPAN → LOAD CUTSCENE
    // ============================================================
    // Dipanggil oleh ConfirmationPanelBuilder saat klik Mulai
    public void OnConfirmationStart() => OnConfirm();
    public void OnConfirmationBack()  => OnBack();

    private void OnConfirm()
    {
        if (_selectedCharacter == GameData.CharacterType.None ||
            _selectedTrait     == GameData.TraitType.None)
        {
            Debug.LogWarning("[CharacterSelect] Pilihan belum lengkap!");
            return;
        }

        if (GameData.Instance != null)
        {
            GameData.Instance.selectedCharacter = _selectedCharacter;
            GameData.Instance.selectedTrait     = _selectedTrait;
            GameData.Instance.SaveData();
        }

        ApplyTraitBonus();

        Debug.Log($"[CharacterSelect] Memulai game → {cutsceneSceneName}");
        SceneManager.LoadScene(cutsceneSceneName);
    }

    private void OnBack()
    {
        confirmationBuilder?.HidePanel();
        panelConfirmation?.SetActive(false);
        panelTraitSelect?.SetActive(true);
    }

    // ============================================================
    //  APPLY BONUS TRAIT KE MENTAL STATE
    // ============================================================
    private void ApplyTraitBonus()
    {
        var mental = MentalStateManager.Instance;
        var data   = GameData.Instance;
        if (mental == null || data == null) return;

        mental.ResetAllStats();

        float courageBonus = data.GetCourageBonus();
        if (courageBonus > 0)
            mental.AddCourage(courageBonus);

        Debug.Log($"[CharacterSelect] Trait bonus applied → Courage bonus: {courageBonus}");
    }

    // ============================================================
    //  UTILITY
    // ============================================================
    private void HideAllPanels()
    {
        panelCharacterSelect?.SetActive(false);
        panelTraitSelect?.SetActive(false);
        panelConfirmation?.SetActive(false);
        confirmationBuilder?.HidePanel();
    }
}