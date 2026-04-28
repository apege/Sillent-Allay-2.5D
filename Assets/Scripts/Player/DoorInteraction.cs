using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Press E to Enter — versi door interaction.
/// Reuse SceneTransitionManager yang sudah ada (sama seperti SceneTransitionTrigger).
/// 
/// Setup:
///   1. Attach script ini ke GameObject pintu
///   2. Tambahkan Collider / Collider2D, centang Is Trigger = TRUE
///   3. Assign field di Inspector
/// </summary>
public class DoorInteraction : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private int targetSceneIndex = -1;
    [SerializeField] private bool useSceneName = true;

    [Header("Spawn Settings")]
    [Tooltip("ID spawn point di scene tujuan (sama seperti di SceneTransitionTrigger)")]
    [SerializeField] private string targetSpawnPointID;

    [Header("Prompt UI")]
    [Tooltip("GameObject prompt 'Press E to Enter'")]
    [SerializeField] private GameObject promptUI;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0f, 0.25f);

    // ─────────────────────────
    private bool _playerInRange = false;
    private bool _hasTriggered = false;
    private InputAction _interactAction;

    // ─────────────────────────
    private void Awake()
    {
        _interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonSouth"); // A (Xbox) / X (PS)

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void OnEnable()
    {
        _interactAction.Enable();
        _interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();
    }

    // ─────────────────────────
    // Deteksi player masuk / keluar area pintu
    // Support 3D dan 2D sekaligus
    // ─────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) SetInRange(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) SetInRange(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) SetInRange(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) SetInRange(false);
    }

    // ─────────────────────────
    // Input
    // ─────────────────────────
    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_playerInRange && !_hasTriggered)
            TriggerTransition();
    }

    // ─────────────────────────
    // Transition — logika sama persis seperti SceneTransitionTrigger
    // ─────────────────────────
    private void TriggerTransition()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[DoorInteraction] SceneTransitionManager tidak ditemukan! " +
                           "Pastikan TransitionCanvas ada di scene.");
            return;
        }

        _hasTriggered = true;

        if (promptUI != null)
            promptUI.SetActive(false);

        // Set spawn point tujuan (pakai sistem SpawnPointManager yang sudah ada)
        if (!string.IsNullOrEmpty(targetSpawnPointID))
            SpawnPointManager.nextSpawnPointID = targetSpawnPointID;

        if (useSceneName && !string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"[DoorInteraction] Transisi ke scene: {targetSceneName}");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else if (!useSceneName && targetSceneIndex >= 0)
        {
            Debug.Log($"[DoorInteraction] Transisi ke scene index: {targetSceneIndex}");
            SceneTransitionManager.Instance.TransitionToScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError("[DoorInteraction] Target scene tidak valid! Isi targetSceneName atau targetSceneIndex.");
            _hasTriggered = false;
        }
    }

    // ─────────────────────────
    private void SetInRange(bool inRange)
    {
        _playerInRange = inRange;

        if (promptUI != null)
            promptUI.SetActive(inRange);

        if (!inRange)
            _hasTriggered = false;
    }

    // ─────────────────────────
    // Gizmo — biar kelihatan di Scene view
    // ─────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        BoxCollider box = GetComponent<BoxCollider>();
        BoxCollider2D box2d = GetComponent<BoxCollider2D>();

        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (box2d != null)
        {
            Gizmos.DrawWireSphere(transform.position, 0.8f);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"[E] → {(useSceneName ? targetSceneName : $"Index {targetSceneIndex}")}"
        );
#endif
    }
}