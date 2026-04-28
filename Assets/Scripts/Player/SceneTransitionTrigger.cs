using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Nama scene tujuan (pastikan sudah ada di Build Settings)")]
    [SerializeField] private string targetSceneName;

    [Header("Alternative: Scene Index")]
    [Tooltip("Atau gunakan index scene (0, 1, 2, dst)")]
    [SerializeField] private int targetSceneIndex = -1;

    [Header("Settings")]
    [SerializeField] private bool useSceneName = true;
    [SerializeField] private bool disableAfterTrigger = true;
    [SerializeField] private bool stopPlayerOnContact = true;

    [Header("Visual Settings")]
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);

    [Header("Spawn Settings")]
    [Tooltip("ID spawn point di scene tujuan")]
    [SerializeField] private string targetSpawnPointID;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang menyentuh adalah Player
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            // Stop player immediately jika diaktifkan
            if (stopPlayerOnContact)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            TriggerTransition();
        }
    }

    private void TriggerTransition()
    {
        // Cek apakah SceneTransitionManager ada
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager tidak ditemukan di scene! Pastikan TransitionCanvas dengan SceneTransitionManager ada di scene.");
            return;
        }

        // Cegah trigger ganda
        if (disableAfterTrigger)
        {
            hasTriggered = true;
        }

        if (!string.IsNullOrEmpty(targetSpawnPointID))
        {
            SpawnPointManager.nextSpawnPointID = targetSpawnPointID;
        }

        // Mulai transisi
        if (useSceneName && !string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Memulai transisi ke scene: {targetSceneName}");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else if (!useSceneName && targetSceneIndex >= 0)
        {
            Debug.Log($"Memulai transisi ke scene index: {targetSceneIndex}");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError("Target scene tidak valid! Pastikan Anda mengisi Target Scene Name atau Target Scene Index dengan benar.");
        }
    }

    /// <summary>
    /// Untuk debugging - tampilkan area trigger di Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            // Draw wireframe
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }

    /// <summary>
    /// Tampilkan label di Scene view
    /// </summary>
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"Scene Transition\n→ {(useSceneName ? targetSceneName : $"Index: {targetSceneIndex}")}"
        );
#endif
    }

    /// <summary>
    /// Manual trigger untuk testing atau dipanggil dari script lain
    /// </summary>
    public void ManualTrigger()
    {
        if (!hasTriggered)
        {
            TriggerTransition();
        }
    }

    /// <summary>
    /// Reset trigger agar bisa digunakan lagi
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}