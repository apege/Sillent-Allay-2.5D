using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private string cutsceneSceneName;
    [SerializeField] private string cutsceneID; // ID unik biar bisa punya banyak cutscene

    [Header("Tag Player")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Cek apakah cutscene ini sudah pernah ditonton
        if (PlayerPrefs.GetInt(cutsceneID, 0) == 1) return;

        // Simpan dulu bahwa cutscene ini sudah ditonton
        PlayerPrefs.SetInt(cutsceneID, 1);
        PlayerPrefs.Save();

        // Transisi ke scene cutscene
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(cutsceneSceneName);
        else
            Debug.LogError("[CutsceneTrigger] SceneTransitionManager tidak ditemukan!");
    }

    // Untuk reset saat testing
    [ContextMenu("Reset Cutscene")]
    public void ResetCutscene()
    {
        PlayerPrefs.DeleteKey(cutsceneID);
        Debug.Log($"[CutsceneTrigger] Cutscene {cutsceneID} direset!");
    }
}