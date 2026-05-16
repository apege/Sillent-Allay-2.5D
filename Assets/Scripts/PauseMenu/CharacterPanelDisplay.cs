using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  CharacterPanelDisplay.cs
//  Script untuk menampilkan 4 kotak di tab Character.
//  Kotak 2 (Trait) otomatis ngambil data dari GameData
//  sesuai pilihan player di awal game.
// ============================================================

public class CharacterPanelDisplay : MonoBehaviour
{
    // ----------------------------------------------------------
    // REFERENSI 4 KOTAK IMAGE DI UI
    // ----------------------------------------------------------
    [Header("=== 4 Kotak Image di Character Panel ===")]
    public Image box1_StatusMental;
    public Image box2_Trait;
    public Image box3_Memory;
    public Image box4_Relationship;

    // ----------------------------------------------------------
    // LABEL TEKS (opsional)
    // ----------------------------------------------------------
    [Header("=== Label Teks (Opsional) ===")]
    public TMP_Text box1_Label;
    public TMP_Text box2_Label;
    public TMP_Text box3_Label;
    public TMP_Text box4_Label;

    // ==========================================================
    // KOTAK 1 — STATUS MENTAL
    // ==========================================================
    [Header("=== Kotak 1: Sprite Status Mental ===")]
    [Tooltip("Sanity > 70 dan Trauma < 30")]
    public Sprite statusStabil;

    [Tooltip("Sanity 30-70 atau Trauma 30-60")]
    public Sprite statusTertekan;

    [Tooltip("Sanity < 30 atau Trauma > 75")]
    public Sprite statusTrauma;

    // ==========================================================
    // KOTAK 2 — TRAIT (otomatis dari GameData)
    // ==========================================================
    [Header("=== Kotak 2: Sprite Trait Karakter ===")]
    [Tooltip("Sprite untuk trait Introvert")]
    public Sprite traitSpriteIntrovert;

    [Tooltip("Sprite untuk trait Empatik")]
    public Sprite traitSpriteEmpatik;

    [Tooltip("Sprite untuk trait Penakut")]
    public Sprite traitSpritePenakut;

    [Tooltip("Sprite untuk trait Pemberani")]
    public Sprite traitSpritePemberani;

    [Tooltip("Sprite untuk trait Sensitif")]
    public Sprite traitSpriteSensitif;

    [Tooltip("Sprite fallback kalau GameData kosong")]
    public Sprite traitSprite;

    [Tooltip("Label fallback kalau GameData kosong")]
    public string traitName = "Introvert";

    // ==========================================================
    // KOTAK 3 — KENANGAN / MEMORY
    // ==========================================================
    [Header("=== Kotak 3: Sprite Kenangan ===")]
    public Sprite memoryNormal;
    public Sprite memoryBullying;
    public Sprite memoryHealing;
    public Sprite memoryBrave;

    // ==========================================================
    // KOTAK 4 — HUBUNGAN / MOOD SOSIAL
    // ==========================================================
    [Header("=== Kotak 4: Sprite Hubungan ===")]
    public Sprite relationshipTerbuka;
    public Sprite relationshipNormal;
    public Sprite relationshipTertutup;

    // ----------------------------------------------------------
    // INTERNAL
    // ----------------------------------------------------------
    private MentalStateManager _manager;

    public enum LastEvent { None, Bullying, Healing, Brave }
    [HideInInspector] public LastEvent lastEvent = LastEvent.None;

    // ============================================================
    //  INISIALISASI
    // ============================================================
    private void Start()
    {
        _manager = MentalStateManager.Instance;

        if (_manager == null)
        {
            Debug.LogError("[CharacterPanel] MentalStateManager tidak ditemukan!");
            return;
        }

        _manager.onStatsChanged.AddListener(RefreshAllBoxes);

        // Kotak 2: ambil trait dari GameData
        SetupTraitBox();

        RefreshAllBoxes();

        Debug.Log("[CharacterPanel] Berhasil terhubung ke MentalStateManager.");
    }

    // ============================================================
    //  REFRESH SEMUA KOTAK
    // ============================================================
    public void RefreshAllBoxes()
    {
        if (_manager == null) return;

        UpdateBox1_StatusMental();
        UpdateBox3_Memory();
        UpdateBox4_Relationship();
        // Kotak 2 tidak di-refresh karena trait tidak berubah di tengah game
    }

    // ============================================================
    //  KOTAK 1 — STATUS MENTAL
    // ============================================================
    private void UpdateBox1_StatusMental()
    {
        if (box1_StatusMental == null) return;

        float trauma = _manager.trauma;
        float sanity = _manager.sanity;

        if (sanity > 70f && trauma < 30f)
        {
            box1_StatusMental.sprite = statusStabil;
            if (box1_Label != null) box1_Label.text = "Stabil";
        }
        else if (_manager.isPsychologicalModeActive || sanity <= 30f)
        {
            box1_StatusMental.sprite = statusTrauma;
            if (box1_Label != null) box1_Label.text = "Trauma Berat";
        }
        else
        {
            box1_StatusMental.sprite = statusTertekan;
            if (box1_Label != null) box1_Label.text = "Tertekan";
        }

        box1_StatusMental.enabled = (box1_StatusMental.sprite != null);
    }

    // ============================================================
    //  KOTAK 2 — TRAIT (ngambil dari GameData)
    // ============================================================
    private void SetupTraitBox()
    {
        if (box2_Trait == null) return;

        // Cek apakah GameData ada dan trait sudah dipilih
        if (GameData.Instance != null && GameData.Instance.selectedTrait != GameData.TraitType.None)
        {
            GameData.TraitType trait = GameData.Instance.selectedTrait;

            // Pilih sprite sesuai trait
            Sprite selectedSprite = GetTraitSprite(trait);
            if (selectedSprite != null)
                box2_Trait.sprite = selectedSprite;

            // Label nama trait
            if (box2_Label != null)
                box2_Label.text = trait.ToString();

            Debug.Log($"[CharacterPanel] Trait dari GameData: {trait}");
        }
        else
        {
            // Fallback: pakai sprite manual dari Inspector
            if (traitSprite != null)
                box2_Trait.sprite = traitSprite;
            if (box2_Label != null)
                box2_Label.text = traitName;

            Debug.LogWarning("[CharacterPanel] GameData tidak ditemukan, pakai fallback trait.");
        }

        box2_Trait.enabled = (box2_Trait.sprite != null);
    }

    // Mapping trait → sprite
    private Sprite GetTraitSprite(GameData.TraitType trait)
    {
        switch (trait)
        {
            case GameData.TraitType.Introvert:  return traitSpriteIntrovert;
            case GameData.TraitType.Empatik:    return traitSpriteEmpatik;
            case GameData.TraitType.Penakut:    return traitSpritePenakut;
            case GameData.TraitType.Pemberani:  return traitSpritePemberani;
            case GameData.TraitType.Sensitif:   return traitSpriteSensitif;
            default:                            return traitSprite;
        }
    }

    // ============================================================
    //  KOTAK 3 — KENANGAN TERAKHIR
    // ============================================================
    private void UpdateBox3_Memory()
    {
        if (box3_Memory == null) return;

        switch (lastEvent)
        {
            case LastEvent.Bullying:
                box3_Memory.sprite = memoryBullying;
                if (box3_Label != null) box3_Label.text = "Bertemu Pembully";
                break;
            case LastEvent.Healing:
                box3_Memory.sprite = memoryHealing;
                if (box3_Label != null) box3_Label.text = "Menulis Jurnal";
                break;
            case LastEvent.Brave:
                box3_Memory.sprite = memoryBrave;
                if (box3_Label != null) box3_Label.text = "Tindakan Berani";
                break;
            default:
                box3_Memory.sprite = memoryNormal;
                if (box3_Label != null) box3_Label.text = "Belum Ada Kejadian";
                break;
        }

        box3_Memory.enabled = (box3_Memory.sprite != null);
    }

    // ============================================================
    //  KOTAK 4 — HUBUNGAN / MOOD SOSIAL
    // ============================================================
    private void UpdateBox4_Relationship()
    {
        if (box4_Relationship == null) return;

        float courage = _manager.courage;

        if (courage >= 70f)
        {
            box4_Relationship.sprite = relationshipTerbuka;
            if (box4_Label != null) box4_Label.text = "Terbuka";
        }
        else if (courage >= 30f)
        {
            box4_Relationship.sprite = relationshipNormal;
            if (box4_Label != null) box4_Label.text = "Biasa";
        }
        else
        {
            box4_Relationship.sprite = relationshipTertutup;
            if (box4_Label != null) box4_Label.text = "Menarik Diri";
        }

        box4_Relationship.enabled = (box4_Relationship.sprite != null);
    }

    // ============================================================
    //  FUNGSI PUBLIK — dipanggil dari script lain
    // ============================================================
    public void RecordEvent_Bullying()
    {
        lastEvent = LastEvent.Bullying;
        UpdateBox3_Memory();
    }

    public void RecordEvent_Healing()
    {
        lastEvent = LastEvent.Healing;
        UpdateBox3_Memory();
    }

    public void RecordEvent_Brave()
    {
        lastEvent = LastEvent.Brave;
        UpdateBox3_Memory();
    }

    // ============================================================
    //  CLEANUP
    // ============================================================
    private void OnDestroy()
    {
        if (_manager != null)
            _manager.onStatsChanged.RemoveListener(RefreshAllBoxes);
    }
}