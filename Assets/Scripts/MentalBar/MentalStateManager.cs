using UnityEngine;
using UnityEngine.Events;

// ============================================================
//  MentalStateManager.cs
//  Script utama yang mengatur semua status mental karakter.
//  Gunakan sebagai Singleton agar bisa diakses dari mana saja
//  dan tetap ada di semua scene (DontDestroyOnLoad).
// ============================================================

public class MentalStateManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // SINGLETON SETUP
    // Memastikan hanya ada satu instance di seluruh game
    // ----------------------------------------------------------
    public static MentalStateManager Instance { get; private set; }

    private void Awake()
    {
        // Jika sudah ada instance lain, hancurkan yang baru
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Jangan hancurkan saat pindah scene
        DontDestroyOnLoad(gameObject);
    }

    // ----------------------------------------------------------
    // NILAI STATUS MENTAL (0 - 100)
    // ----------------------------------------------------------
    [Header("=== Nilai Awal Status Mental ===")]

    [Range(0f, 100f)]
    public float trauma = 0f;       // Tingkat trauma karakter

    [Range(0f, 100f)]
    public float courage = 50f;     // Tingkat keberanian karakter

    [Range(0f, 100f)]
    public float sanity = 100f;     // Kestabilan mental karakter

    // ----------------------------------------------------------
    // PENGATURAN SISTEM
    // ----------------------------------------------------------
    [Header("=== Pengaturan Sistem ===")]

    [Tooltip("Seberapa cepat Sanity berubah setiap detik")]
    public float sanityUpdateSpeed = 2f;

    [Tooltip("Ambang batas Trauma untuk mode psikologis")]
    public float traumaThreshold = 75f;

    [Tooltip("Ambang batas Sanity untuk efek stres berat")]
    public float sanityLowThreshold = 30f;

    [Tooltip("Ambang batas Keberanian untuk mengurangi dampak trauma")]
    public float courageHighThreshold = 70f;

    [Tooltip("Seberapa besar pengurangan trauma saat keberanian tinggi (0-1)")]
    [Range(0f, 1f)]
    public float courageTraumaReduction = 0.5f;

    // ----------------------------------------------------------
    // STATE FLAGS (status aktif/tidaknya kondisi tertentu)
    // ----------------------------------------------------------
    [Header("=== Status Aktif ===")]
    [HideInInspector] public bool isPsychologicalModeActive = false;
    [HideInInspector] public bool isLowSanityActive = false;

    // ----------------------------------------------------------
    // EVENTS (untuk memberitahu script lain saat status berubah)
    // Script lain bisa "subscribe" ke event ini
    // ----------------------------------------------------------
    [Header("=== Events ===")]
    public UnityEvent onPsychologicalModeActivated;   // Dipanggil saat mode psikologis aktif
    public UnityEvent onPsychologicalModeDeactivated; // Dipanggil saat mode psikologis nonaktif
    public UnityEvent onLowSanityActivated;           // Dipanggil saat sanity sangat rendah
    public UnityEvent onLowSanityDeactivated;         // Dipanggil saat sanity kembali normal

    // Dipakai oleh UI untuk mengetahui ada perubahan nilai
    public UnityEvent onStatsChanged;

    // ============================================================
    //  UPDATE - dipanggil setiap frame
    // ============================================================
    private void Update()
    {
        UpdateSanity();
        CheckThresholds();
    }

    // ============================================================
    //  FUNGSI TRAUMA
    // ============================================================

    /// <summary>
    /// Tambahkan Trauma. Jika Keberanian tinggi, dampaknya dikurangi.
    /// Contoh: AddTrauma(20f) saat bertemu pembully.
    /// </summary>
    public void AddTrauma(float amount)
    {
        // Jika keberanian tinggi, kurangi dampak trauma yang masuk
        if (courage >= courageHighThreshold)
        {
            float reduction = amount * courageTraumaReduction;
            amount -= reduction;
            Debug.Log($"[MentalState] Keberanian tinggi! Trauma dikurangi sebesar {reduction:F1}");
        }

        trauma = Mathf.Clamp(trauma + amount, 0f, 100f);
        Debug.Log($"[MentalState] Trauma bertambah +{amount:F1} → Trauma: {trauma:F1}");

        onStatsChanged?.Invoke();
    }

    /// <summary>
    /// Kurangi Trauma.
    /// Contoh: ReduceTrauma(15f) saat menulis jurnal atau healing.
    /// </summary>
    public void ReduceTrauma(float amount)
    {
        trauma = Mathf.Clamp(trauma - amount, 0f, 100f);
        Debug.Log($"[MentalState] Trauma berkurang -{amount:F1} → Trauma: {trauma:F1}");

        onStatsChanged?.Invoke();
    }

    // ============================================================
    //  FUNGSI KEBERANIAN
    // ============================================================

    /// <summary>
    /// Tambahkan Keberanian.
    /// Contoh: AddCourage(10f) saat memilih tindakan berani.
    /// </summary>
    public void AddCourage(float amount)
    {
        courage = Mathf.Clamp(courage + amount, 0f, 100f);
        Debug.Log($"[MentalState] Keberanian bertambah +{amount:F1} → Keberanian: {courage:F1}");

        onStatsChanged?.Invoke();
    }

    /// <summary>
    /// Kurangi Keberanian.
    /// Contoh: ReduceCourage(10f) saat karakter ketakutan.
    /// </summary>
    public void ReduceCourage(float amount)
    {
        courage = Mathf.Clamp(courage - amount, 0f, 100f);
        Debug.Log($"[MentalState] Keberanian berkurang -{amount:F1} → Keberanian: {courage:F1}");

        onStatsChanged?.Invoke();
    }

    // ============================================================
    //  FUNGSI SANITY (dipanggil otomatis di Update)
    // ============================================================

    /// <summary>
    /// Update Sanity secara otomatis berdasarkan Trauma dan Keberanian.
    /// - Trauma tinggi → Sanity turun
    /// - Keberanian tinggi → Sanity naik / stabil
    /// </summary>
    public void UpdateSanity()
    {
        // Hitung target sanity berdasarkan kondisi saat ini
        // Formula: Sanity ideal = 100 - Trauma + (Courage * 0.5)
        // Lalu di-clamp antara 0-100
        float targetSanity = Mathf.Clamp(100f - trauma + (courage * 0.3f), 0f, 100f);

        // Geser sanity perlahan ke arah target
        sanity = Mathf.MoveTowards(sanity, targetSanity, sanityUpdateSpeed * Time.deltaTime);

        onStatsChanged?.Invoke();
    }

    // ============================================================
    //  CEK BATAS (Threshold) - dipanggil di Update
    // ============================================================

    /// <summary>
    /// Cek apakah kondisi tertentu harus diaktifkan atau dinonaktifkan.
    /// </summary>
    private void CheckThresholds()
    {
        // --- Cek Mode Psikologis (Trauma >= 75) ---
        if (trauma >= traumaThreshold && !isPsychologicalModeActive)
        {
            isPsychologicalModeActive = true;
            Debug.LogWarning("[MentalState] ⚠ MODE PSIKOLOGIS AKTIF! Trauma sangat tinggi.");
            onPsychologicalModeActivated?.Invoke();
        }
        else if (trauma < traumaThreshold && isPsychologicalModeActive)
        {
            isPsychologicalModeActive = false;
            Debug.Log("[MentalState] Mode Psikologis dinonaktifkan.");
            onPsychologicalModeDeactivated?.Invoke();
        }

        // --- Cek Sanity Rendah (Sanity <= 30) ---
        if (sanity <= sanityLowThreshold && !isLowSanityActive)
        {
            isLowSanityActive = true;
            Debug.LogWarning("[MentalState] ⚠ SANITY KRITIS! Aktifkan efek visual/audio stres.");
            onLowSanityActivated?.Invoke();
        }
        else if (sanity > sanityLowThreshold && isLowSanityActive)
        {
            isLowSanityActive = false;
            Debug.Log("[MentalState] Sanity kembali normal.");
            onLowSanityDeactivated?.Invoke();
        }
    }

    // ============================================================
    //  UTILITY - Fungsi tambahan yang berguna
    // ============================================================

    /// <summary>
    /// Reset semua status ke nilai default.
    /// Berguna saat memulai game baru.
    /// </summary>
    public void ResetAllStats()
    {
        trauma  = 0f;
        courage = 50f;
        sanity  = 100f;
        isPsychologicalModeActive = false;
        isLowSanityActive = false;
        Debug.Log("[MentalState] Semua status direset ke default.");
        onStatsChanged?.Invoke();
    }

    /// <summary>
    /// Lakukan sesi healing: kurangi trauma, tambah keberanian.
    /// Contoh: HealingSession(15f, 10f) saat menulis jurnal.
    /// </summary>
    public void HealingSession(float traumaReduction, float courageBoost)
    {
        Debug.Log("[MentalState] 💚 Sesi healing dimulai...");
        ReduceTrauma(traumaReduction);
        AddCourage(courageBoost);
    }

    // Properti publik untuk kemudahan akses nilai (0.0 - 1.0 untuk slider)
    public float TraumaNormalized  => trauma  / 100f;
    public float CourageNormalized => courage / 100f;
    public float SanityNormalized  => sanity  / 100f;
}