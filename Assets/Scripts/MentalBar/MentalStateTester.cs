using UnityEngine;

// ============================================================
//  MentalStateTester.cs
//  Script untuk menguji sistem Mental State.
//  
//  CARA PAKAI:
//  1. Attach script ini ke sembarang GameObject di scene
//  2. Jalankan game
//  3. Tekan tombol keyboard untuk simulasi kejadian
//  
//  KONTROL KEYBOARD:
//  [T] - Tambah Trauma (bertemu pembully)
//  [H] - Healing session (menulis jurnal)
//  [B] - Tindakan berani
//  [F] - Situasi menakutkan
//  [R] - Reset semua status
//  [I] - Info: cetak semua nilai ke Console
// ============================================================

public class MentalStateTester : MonoBehaviour
{
    // ----------------------------------------------------------
    // PENGATURAN (bisa diubah di Inspector)
    // ----------------------------------------------------------
    [Header("=== Jumlah Perubahan per Aksi ===")]
    public float bullyingTraumaAmount = 20f;  // Trauma saat ketemu pembully
    public float healingTraumaReduce  = 15f;  // Trauma berkurang saat healing
    public float healingCourageBoost  = 10f;  // Courage bertambah saat healing
    public float braveCourageAmount   = 15f;  // Courage saat tindakan berani
    public float fearCourageReduce    = 10f;  // Courage berkurang saat takut
    public float fearTraumaAmount     = 8f;   // Trauma sedikit naik saat takut

    // ============================================================
    //  UPDATE - Cek input keyboard setiap frame
    // ============================================================
    private void Update()
    {
        // Shortcut: ambil reference ke Manager
        var m = MentalStateManager.Instance;

        // Jika Manager tidak ada, berhenti
        if (m == null)
        {
            Debug.LogError("[Tester] MentalStateManager tidak ditemukan!");
            return;
        }

        // ----------------------------------------------------------
        // [T] Simulasi: Bertemu Pembully
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("=== KEJADIAN: Bertemu Pembully! ===");
            m.AddTrauma(bullyingTraumaAmount);
            m.ReduceCourage(5f);
        }

        // ----------------------------------------------------------
        // [H] Simulasi: Menulis Jurnal / Healing
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("=== KEJADIAN: Menulis Jurnal (Healing Session) ===");
            m.HealingSession(healingTraumaReduce, healingCourageBoost);
        }

        // ----------------------------------------------------------
        // [B] Simulasi: Mengambil Tindakan Berani
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("=== KEJADIAN: Mengambil Tindakan Berani! ===");
            m.AddCourage(braveCourageAmount);
        }

        // ----------------------------------------------------------
        // [F] Simulasi: Situasi Menakutkan / Tertekan
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("=== KEJADIAN: Situasi Menakutkan! ===");
            m.ReduceCourage(fearCourageReduce);
            m.AddTrauma(fearTraumaAmount);
        }

        // ----------------------------------------------------------
        // [R] Reset semua status
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("=== RESET: Semua status dikembalikan ke awal ===");
            m.ResetAllStats();
        }

        // ----------------------------------------------------------
        // [I] Cetak info lengkap ke Console
        // ----------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintInfo(m);
        }
    }

    /// <summary>
    /// Cetak semua nilai status ke Console Unity.
    /// </summary>
    private void PrintInfo(MentalStateManager m)
    {
        Debug.Log("╔══════════════════════════════╗");
        Debug.Log("║   STATUS MENTAL KARAKTER     ║");
        Debug.Log("╠══════════════════════════════╣");
        Debug.Log($"║  Trauma    : {m.trauma:F1} / 100       ║");
        Debug.Log($"║  Keberanian: {m.courage:F1} / 100       ║");
        Debug.Log($"║  Sanity    : {m.sanity:F1} / 100       ║");
        Debug.Log("╠══════════════════════════════╣");
        Debug.Log($"║  Mode Psikologis : {(m.isPsychologicalModeActive ? "AKTIF ⚠" : "Tidak Aktif")}  ║");
        Debug.Log($"║  Sanity Kritis   : {(m.isLowSanityActive ? "AKTIF ⚠" : "Tidak Aktif")}  ║");
        Debug.Log("╚══════════════════════════════╝");
    }

    // ============================================================
    //  CONTOH FUNGSI - Bisa dipanggil lewat Button UI di scene
    // ============================================================

    // Hubungkan ke Button "Bully" di UI
    public void OnBullyButtonPressed()
    {
        MentalStateManager.Instance?.AddTrauma(bullyingTraumaAmount);
    }

    // Hubungkan ke Button "Healing" di UI
    public void OnHealingButtonPressed()
    {
        MentalStateManager.Instance?.HealingSession(healingTraumaReduce, healingCourageBoost);
    }

    // Hubungkan ke Button "Berani" di UI
    public void OnBraveButtonPressed()
    {
        MentalStateManager.Instance?.AddCourage(braveCourageAmount);
    }

    // Hubungkan ke Button "Reset" di UI
    public void OnResetButtonPressed()
    {
        MentalStateManager.Instance?.ResetAllStats();
    }
}