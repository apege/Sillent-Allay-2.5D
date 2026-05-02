using UnityEngine;
using UnityEngine.UI;
using TMPro; // Hapus jika tidak pakai TextMeshPro, ganti TMP_Text dengan Text

// ============================================================
//  CharacterPanelDisplay.cs
//  Script untuk menampilkan 4 kotak di tab Character.
//  Setiap kotak otomatis ganti gambar berdasarkan nilai
//  Mental State (Trauma, Courage, Sanity) dari MentalStateManager.
//
//  SETUP DI UNITY:
//  1. Attach script ini ke GameObject "CharacterPanel" (atau Canvas)
//  2. Isi 4 slot Image di Inspector
//  3. Isi sprite-sprite sesuai kondisi di Inspector
//  4. Jalankan — gambar otomatis ganti sesuai kondisi karakter
// ============================================================

public class CharacterPanelDisplay : MonoBehaviour
{
    // ----------------------------------------------------------
    // REFERENSI 4 KOTAK IMAGE DI UI
    // Drag komponen Image dari setiap kotak ke sini
    // ----------------------------------------------------------
    [Header("=== 4 Kotak Image di Character Panel ===")]
    public Image box1_StatusMental;    // Kotak 1: Status Mental
    public Image box2_Trait;           // Kotak 2: Trait / Kepribadian
    public Image box3_Memory;          // Kotak 3: Kenangan terakhir
    public Image box4_Relationship;    // Kotak 4: Hubungan / Mood sosial

    // ----------------------------------------------------------
    // LABEL TEKS DI BAWAH SETIAP KOTAK (opsional)
    // ----------------------------------------------------------
    [Header("=== Label Teks (Opsional) ===")]
    public TMP_Text box1_Label;
    public TMP_Text box2_Label;
    public TMP_Text box3_Label;
    public TMP_Text box4_Label;

    // ==========================================================
    // KOTAK 1 — STATUS MENTAL
    // Ganti gambar sesuai kondisi Sanity & Trauma
    // ==========================================================
    [Header("=== Kotak 1: Sprite Status Mental ===")]
    [Tooltip("Sanity > 70 dan Trauma < 30")]
    public Sprite statusStabil;         // Contoh: wajah tenang

    [Tooltip("Sanity 30-70 atau Trauma 30-60")]
    public Sprite statusTertekan;       // Contoh: wajah khawatir

    [Tooltip("Sanity < 30 atau Trauma > 75")]
    public Sprite statusTrauma;         // Contoh: wajah panik/ketakutan

    // ==========================================================
    // KOTAK 2 — TRAIT / KEPRIBADIAN
    // Bisa diset manual (tidak berubah-ubah)
    // ==========================================================
    [Header("=== Kotak 2: Sprite Trait Karakter ===")]
    [Tooltip("Sprite trait yang dipasang permanent")]
    public Sprite traitSprite;          // Contoh: icon introvert, empatik, dll

    [Tooltip("Nama trait yang ditampilkan")]
    public string traitName = "Introvert";

    // ==========================================================
    // KOTAK 3 — KENANGAN / MEMORY
    // Ganti gambar berdasarkan kejadian terbaru
    // ==========================================================
    [Header("=== Kotak 3: Sprite Kenangan ===")]
    public Sprite memoryNormal;         // Belum ada kejadian besar
    public Sprite memoryBullying;       // Setelah bertemu pembully
    public Sprite memoryHealing;        // Setelah sesi healing/jurnal
    public Sprite memoryBrave;          // Setelah tindakan berani

    // ==========================================================
    // KOTAK 4 — HUBUNGAN / MOOD SOSIAL
    // Berubah berdasarkan Courage (keberanian sosial)
    // ==========================================================
    [Header("=== Kotak 4: Sprite Hubungan ===")]
    public Sprite relationshipTerbuka;  // Courage >= 70: mudah bersosialisasi
    public Sprite relationshipNormal;   // Courage 30-70: biasa
    public Sprite relationshipTertutup; // Courage < 30: menarik diri

    // ----------------------------------------------------------
    // INTERNAL — tracking kejadian terakhir
    // ----------------------------------------------------------
    private MentalStateManager _manager;

    // Enum untuk tipe kejadian terakhir
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

        // Subscribe ke event perubahan stats
        _manager.onStatsChanged.AddListener(RefreshAllBoxes);

        // Pasang trait (kotak 2) — nilainya tetap, langsung dipasang di Start
        SetupTraitBox();

        // Update pertama kali
        RefreshAllBoxes();

        Debug.Log("[CharacterPanel] Berhasil terhubung ke MentalStateManager.");
    }

    // ============================================================
    //  REFRESH SEMUA KOTAK — dipanggil setiap kali stats berubah
    // ============================================================
    public void RefreshAllBoxes()
    {
        if (_manager == null) return;

        UpdateBox1_StatusMental();
        UpdateBox3_Memory();
        UpdateBox4_Relationship();
        // Kotak 2 (Trait) tidak perlu di-update karena tetap
    }

    // ============================================================
    //  KOTAK 1 — STATUS MENTAL
    // ============================================================
    private void UpdateBox1_StatusMental()
    {
        if (box1_StatusMental == null) return;

        float trauma = _manager.trauma;
        float sanity = _manager.sanity;

        // Tentukan kondisi dan pilih sprite + label
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

        // Aktifkan/nonaktifkan sprite berdasarkan apakah ada gambarnya
        box1_StatusMental.enabled = (box1_StatusMental.sprite != null);
    }

    // ============================================================
    //  KOTAK 2 — TRAIT (dipasang sekali di Start)
    // ============================================================
    private void SetupTraitBox()
    {
        if (box2_Trait == null) return;

        if (traitSprite != null)
            box2_Trait.sprite = traitSprite;

        if (box2_Label != null)
            box2_Label.text = traitName;

        box2_Trait.enabled = (box2_Trait.sprite != null);
    }

    // ============================================================
    //  KOTAK 3 — KENANGAN TERAKHIR
    // ============================================================
    private void UpdateBox3_Memory()
    {
        if (box3_Memory == null) return;

        // Pilih sprite berdasarkan kejadian terakhir yang dicatat
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
    //  FUNGSI PUBLIK — dipanggil dari script lain saat ada kejadian
    // ============================================================

    /// <summary>
    /// Catat kejadian bullying → update kenangan (Kotak 3)
    /// Panggil ini dari script scene saat player bertemu pembully.
    /// Contoh: CharacterPanelDisplay.Instance.RecordEvent_Bullying();
    /// </summary>
    public void RecordEvent_Bullying()
    {
        lastEvent = LastEvent.Bullying;
        UpdateBox3_Memory();
    }

    /// <summary>
    /// Catat kejadian healing → update kenangan (Kotak 3)
    /// </summary>
    public void RecordEvent_Healing()
    {
        lastEvent = LastEvent.Healing;
        UpdateBox3_Memory();
    }

    /// <summary>
    /// Catat tindakan berani → update kenangan (Kotak 3)
    /// </summary>
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