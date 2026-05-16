using UnityEngine;

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
        // Tidak ada SetActive di sini — DoorInteraction kontrol via parent canvas
    }

    private void LateUpdate()
    {
        if (doorTransform == null || promptPanel == null) return;
        FollowDoor();
    }

    private void FollowDoor()
    {
        Vector3 worldPos = doorTransform.position + worldOffset;
        Vector3 screenPos = _mainCam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f)
        {
            promptPanel.gameObject.SetActive(false);
            return;
        }
        promptPanel.gameObject.SetActive(true);

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