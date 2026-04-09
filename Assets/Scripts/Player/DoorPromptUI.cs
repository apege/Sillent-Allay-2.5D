using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach ke GameObject "DoorPromptUI" (child dari Canvas Screen Space Overlay).
/// Prompt akan otomatis ngikutin posisi pintu di layar, tepat di atasnya.
///
/// Hierarchy yang dibutuhin:
///
///   [Canvas] Screen Space - Overlay
///   └── DoorPromptUI          ← script ini di sini
///       └── PromptPanel       ← background panel
///           ├── KeyBadge      ← Image kotak tombol "E"
///           │   └── KeyText   ← TMP text "E"
///           └── LabelText     ← TMP text "Masuk"
/// </summary>
public class DoorPromptUI : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform pintu di world space — prompt akan ngikutin posisi ini")]
    public Transform doorTransform;

    [Tooltip("Offset dari posisi pintu ke atas (world unit)")]
    public Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

    [Header("References")]
    public RectTransform promptPanel;

    private Camera _mainCam;
    private Canvas _canvas;
    private RectTransform _canvasRect;

    private void Awake()
    {
        _mainCam = Camera.main;
        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();

        // Sembunyiin di awal — DoorInteraction yang akan Show/Hide
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (doorTransform == null || promptPanel == null) return;
        FollowDoor();
    }

    private void FollowDoor()
    {
        // Convert posisi world pintu + offset ke screen point
        Vector3 worldPos = doorTransform.position + worldOffset;
        Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);

        // Kalau pintu di belakang kamera, sembunyiin
        if (screenPos.z < 0f)
        {
            promptPanel.gameObject.SetActive(false);
            return;
        }
        promptPanel.gameObject.SetActive(true);

        // Convert screen point ke local point di canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPos,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCam,
            out localPoint
        );

        promptPanel.localPosition = localPoint;
    }
}