using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Hapus baris ini jika tidak menggunakan TextMeshPro

// ============================================================
//  MentalStateUI.cs
//  Script untuk menampilkan status mental ke layar via UI.
//  Attach script ini ke GameObject yang berisi panel UI.
//  
//  CATATAN: Script ini otomatis "subscribe" ke event dari
//  MentalStateManager agar UI selalu sinkron.
// ============================================================

public class MentalStateUI : MonoBehaviour
{
    // ----------------------------------------------------------
    // REFERENSI UI - Hubungkan di Inspector Unity
    // ----------------------------------------------------------
    [Header("=== Trauma Bar ===")]
    public Slider traumaSlider;          // Slider untuk Trauma
    public Image  traumaFillImage;       // Image fill (untuk ubah warna)
    public TMP_Text traumaValueText;     // Teks angka (opsional)

    [Header("=== Keberanian Bar ===")]
    public Slider courageSlider;         // Slider untuk Keberanian
    public Image  courageFillImage;      // Image fill (untuk ubah warna)
    public TMP_Text courageValueText;    // Teks angka (opsional)

    [Header("=== Sanity Bar ===")]
    public Slider sanitySlider;          // Slider untuk Sanity
    public Image  sanityFillImage;       // Image fill (untuk ubah warna)
    public TMP_Text sanityValueText;     // Teks angka (opsional)

    // ----------------------------------------------------------
    // WARNA BAR (bisa diubah di Inspector)
    // ----------------------------------------------------------
    [Header("=== Warna Bar ===")]
    public Color traumaColorNormal  = new Color(0.8f, 0.2f, 0.2f);  // Merah
    public Color traumaColorHigh    = new Color(1f, 0f, 0f);         // Merah terang (kritis)

    public Color courageColorNormal = new Color(0.2f, 0.6f, 1f);    // Biru
    public Color courageColorHigh   = new Color(0f, 1f, 0.5f);      // Hijau (tinggi)

    public Color sanityColorNormal  = new Color(0.5f, 0.8f, 1f);    // Biru muda
    public Color sanityColorLow     = new Color(0.8f, 0f, 0.8f);    // Ungu (kritis)

    // ----------------------------------------------------------
    // REFERENSI KE MANAGER
    // ----------------------------------------------------------
    private MentalStateManager _manager;

    // ============================================================
    //  INISIALISASI
    // ============================================================
    private void Start()
    {
        // Cari MentalStateManager secara otomatis
        _manager = MentalStateManager.Instance;

        if (_manager == null)
        {
            Debug.LogError("[MentalStateUI] MentalStateManager tidak ditemukan! " +
                           "Pastikan ada GameObject dengan MentalStateManager di scene.");
            return;
        }

        // Daftarkan diri ke event onStatsChanged
        // Setiap kali stats berubah, UpdateUI() akan dipanggil
        _manager.onStatsChanged.AddListener(UpdateUI);

        // Setup nilai min/max slider
        SetupSliders();

        // Update UI pertama kali
        UpdateUI();

        Debug.Log("[MentalStateUI] UI berhasil terhubung ke MentalStateManager.");
    }

    /// <summary>
    /// Pastikan semua slider punya nilai min 0 dan max 1.
    /// </summary>
    private void SetupSliders()
    {
        if (traumaSlider  != null) { traumaSlider.minValue  = 0; traumaSlider.maxValue  = 1; }
        if (courageSlider != null) { courageSlider.minValue = 0; courageSlider.maxValue = 1; }
        if (sanitySlider  != null) { sanitySlider.minValue  = 0; sanitySlider.maxValue  = 1; }
    }

    // ============================================================
    //  UPDATE UI - Dipanggil setiap kali stats berubah
    // ============================================================

    /// <summary>
    /// Perbarui semua elemen UI sesuai nilai terkini dari Manager.
    /// </summary>
    public void UpdateUI()
    {
        if (_manager == null) return;

        UpdateTraumaUI();
        UpdateCourageUI();
        UpdateSanityUI();
    }

    // --- Trauma ---
    private void UpdateTraumaUI()
    {
        float value = _manager.TraumaNormalized; // Nilai 0.0 - 1.0

        // Update slider
        if (traumaSlider != null)
            traumaSlider.value = value;

        // Update teks angka (contoh: "75/100")
        if (traumaValueText != null)
            traumaValueText.text = $"{_manager.trauma:F0}";

        // Ubah warna berdasarkan kondisi
        if (traumaFillImage != null)
        {
            traumaFillImage.color = _manager.isPsychologicalModeActive
                ? traumaColorHigh
                : traumaColorNormal;
        }
    }

    // --- Keberanian ---
    private void UpdateCourageUI()
    {
        float value = _manager.CourageNormalized;

        if (courageSlider != null)
            courageSlider.value = value;

        if (courageValueText != null)
            courageValueText.text = $"{_manager.courage:F0}";

        if (courageFillImage != null)
        {
            // Hijau kalau keberanian tinggi, biru kalau normal
            courageFillImage.color = _manager.courage >= _manager.courageHighThreshold
                ? courageColorHigh
                : courageColorNormal;
        }
    }

    // --- Sanity ---
    private void UpdateSanityUI()
    {
        float value = _manager.SanityNormalized;

        if (sanitySlider != null)
            sanitySlider.value = value;

        if (sanityValueText != null)
            sanityValueText.text = $"{_manager.sanity:F0}";

        if (sanityFillImage != null)
        {
            // Ungu kalau sanity kritis, biru muda kalau normal
            sanityFillImage.color = _manager.isLowSanityActive
                ? sanityColorLow
                : sanityColorNormal;
        }
    }

    // ============================================================
    //  CLEANUP - Hapus listener saat object dihancurkan
    // ============================================================
    private void OnDestroy()
    {
        // Penting! Selalu hapus listener agar tidak memory leak
        if (_manager != null)
            _manager.onStatsChanged.RemoveListener(UpdateUI);
    }
}