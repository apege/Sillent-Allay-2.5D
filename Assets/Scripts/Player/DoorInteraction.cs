using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private int targetSceneIndex = -1;
    [SerializeField] private bool useSceneName = true;

    [Header("Spawn Settings")]
    [Tooltip("ID spawn point di scene tujuan")]
    [SerializeField] private string targetSpawnPointID;

    [Header("Prompt UI")]
    [Tooltip("Assign 'Prompt UI Canvas' (parent canvas), BUKAN GameObject 'E'")]
    [SerializeField] private GameObject promptUI;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.8f, 0f, 0.25f);

    private bool _playerInRange = false;
    private bool _hasTriggered = false;
    private InputAction _interactAction;

    private void Awake()
    {
        _interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonSouth");

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

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_playerInRange && !_hasTriggered)
            TriggerTransition();
    }

    private void TriggerTransition()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[DoorInteraction] SceneTransitionManager tidak ditemukan!");
            return;
        }

         _hasTriggered = true;

        // Complete quest saat keluar
        QuestTrigger questTrigger = GetComponent<QuestTrigger>();
        if (questTrigger != null)
            questTrigger.TriggerComplete();

        if (promptUI != null)
            promptUI.SetActive(false);

        if (promptUI != null)
            promptUI.SetActive(false);

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
            Debug.LogError("[DoorInteraction] Target scene tidak valid!");
            _hasTriggered = false;
        }
    }

    private void SetInRange(bool inRange)
    {
        _playerInRange = inRange;
        Debug.Log($"[DoorInteraction] SetInRange: {inRange}, promptUI: {promptUI}");

        if (promptUI != null)
            promptUI.SetActive(inRange);

        if (!inRange)
            _hasTriggered = false;
    }

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