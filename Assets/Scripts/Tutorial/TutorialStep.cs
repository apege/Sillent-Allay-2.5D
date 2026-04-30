using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Jenis aksi yang harus dilakukan player untuk menyelesaikan sebuah step.
    /// </summary>
    public enum TutorialActionType
    {
        MoveHorizontal,     // Tekan A atau D
        Jump,               // Tekan Space
        Sprint,             // Tekan Shift + A/D
        OpenMenu,           // Tekan Escape
        UIIntroduction      // Tidak butuh input — hanya tampilkan info UI
    }

    /// <summary>
    /// Menyimpan semua data yang dibutuhkan untuk satu langkah tutorial.
    /// Bisa dikonfigurasi langsung dari Inspector Unity (jika diekspos via TutorialManager).
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        [Header("Konten Instruksi")]
        [Tooltip("Judul singkat langkah tutorial ini.")]
        public string stepTitle = "Langkah Tutorial";

        [TextArea(2, 5)]
        [Tooltip("Teks instruksi lengkap yang ditampilkan ke pemain.")]
        public string instructionText = "Lakukan sesuatu untuk melanjutkan.";

        [Header("Logika Aksi")]
        [Tooltip("Jenis aksi yang harus dideteksi sebelum lanjut ke step berikutnya.")]
        public TutorialActionType requiredAction;

        [Tooltip("Jika true, step ini langsung selesai setelah durasi tertentu (untuk UI Introduction).")]
        public bool isTimedStep = false;

        [Tooltip("Durasi (detik) sebelum otomatis lanjut. Hanya aktif jika isTimedStep = true.")]
        public float timedStepDuration = 3f;

        [Header("Highlight UI (Opsional)")]
        [Tooltip("Nama GameObject UI yang akan di-highlight (misalnya panel Quest, Inventory, dll).")]
        public string highlightTargetName = "";

        [Tooltip("Warna highlight yang akan diterapkan ke target UI.")]
        public Color highlightColor = new Color(1f, 0.9f, 0f, 0.6f);
    }
}