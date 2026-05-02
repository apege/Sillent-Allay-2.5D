using UnityEngine;

// ============================================================
//  GameData.cs
//  Menyimpan dan memuat pilihan karakter & trait player.
//  Menggunakan PlayerPrefs agar data tetap ada walau keluar game.
//  Akses dari mana saja lewat GameData.Instance
// ============================================================

public class GameData : MonoBehaviour
{
    // ----------------------------------------------------------
    // SINGLETON
    // ----------------------------------------------------------
    public static GameData Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load data yang tersimpan saat game dibuka
        LoadData();
    }

    // ----------------------------------------------------------
    // DATA KARAKTER
    // ----------------------------------------------------------
    public enum CharacterType { None, Nara, Raka }

    public CharacterType selectedCharacter = CharacterType.None;

    // ----------------------------------------------------------
    // DATA TRAIT
    // ----------------------------------------------------------
    public enum TraitType
    {
        None,
        Introvert,   // Courage naik lebih lambat, Healing lebih efektif
        Empatik,     // Healing lebih efektif ke NPC juga
        Penakut,     // Trauma naik lebih cepat
        Pemberani,   // Courage baseline lebih tinggi
        Sensitif     // Sanity lebih mudah turun dan naik
    }

    public TraitType selectedTrait = TraitType.None;

    // ----------------------------------------------------------
    // MODIFIER DARI TRAIT (dipakai MentalStateManager)
    // ----------------------------------------------------------

    // Multiplier Trauma yang masuk (Penakut = 1.5x, Pemberani = 0.8x, dll)
    public float GetTraumaMultiplier()
    {
        switch (selectedTrait)
        {
            case TraitType.Penakut:   return 1.5f;
            case TraitType.Pemberani: return 0.8f;
            default:                  return 1.0f;
        }
    }

    // Multiplier Healing (Empatik & Introvert lebih efektif)
    public float GetHealingMultiplier()
    {
        switch (selectedTrait)
        {
            case TraitType.Introvert: return 1.3f;
            case TraitType.Empatik:   return 1.5f;
            default:                  return 1.0f;
        }
    }

    // Bonus Courage awal (Pemberani mulai lebih tinggi)
    public float GetCourageBonus()
    {
        switch (selectedTrait)
        {
            case TraitType.Pemberani: return 20f;
            default:                  return 0f;
        }
    }

    // Multiplier perubahan Sanity (Sensitif lebih cepat naik/turun)
    public float GetSanityMultiplier()
    {
        switch (selectedTrait)
        {
            case TraitType.Sensitif: return 1.5f;
            default:                 return 1.0f;
        }
    }

    // ----------------------------------------------------------
    // DESKRIPSI TRAIT (untuk ditampilkan di UI)
    // ----------------------------------------------------------
    public string GetTraitDescription(TraitType trait)
    {
        switch (trait)
        {
            case TraitType.Introvert:
                return "Lebih nyaman sendiri.\nHealing lebih efektif, tapi Courage naik lebih lambat.";
            case TraitType.Empatik:
                return "Mudah merasakan perasaan orang lain.\nHealing sangat efektif, hubungan sosial lebih kuat.";
            case TraitType.Penakut:
                return "Mudah merasa takut.\nTrauma bertambah lebih cepat dari kejadian buruk.";
            case TraitType.Pemberani:
                return "Tidak mudah menyerah.\nCourage awal lebih tinggi, dampak Trauma lebih kecil.";
            case TraitType.Sensitif:
                return "Perasaan yang dalam.\nSanity lebih mudah turun, tapi juga lebih cepat pulih.";
            default:
                return "";
        }
    }

    // ----------------------------------------------------------
    // SAVE & LOAD ke PlayerPrefs
    // ----------------------------------------------------------
    public void SaveData()
    {
        PlayerPrefs.SetInt("SelectedCharacter", (int)selectedCharacter);
        PlayerPrefs.SetInt("SelectedTrait",     (int)selectedTrait);
        PlayerPrefs.Save();
        Debug.Log($"[GameData] Data disimpan → Karakter: {selectedCharacter}, Trait: {selectedTrait}");
    }

    public void LoadData()
    {
        selectedCharacter = (CharacterType)PlayerPrefs.GetInt("SelectedCharacter", 0);
        selectedTrait     = (TraitType)PlayerPrefs.GetInt("SelectedTrait", 0);
        Debug.Log($"[GameData] Data dimuat → Karakter: {selectedCharacter}, Trait: {selectedTrait}");
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.DeleteKey("SelectedTrait");
        selectedCharacter = CharacterType.None;
        selectedTrait     = TraitType.None;
        Debug.Log("[GameData] Data direset.");
    }
}