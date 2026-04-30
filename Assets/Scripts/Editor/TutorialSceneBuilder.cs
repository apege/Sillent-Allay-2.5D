#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Tutorial;

/// <summary>
/// Editor tool untuk membuat seluruh hierarki Tutorial di scene secara otomatis.
///
/// Cara pakai:
///   Menu bar → Tools → Tutorial → Build Tutorial Scene
///
/// Yang dibuat otomatis:
///   ── TutorialCanvas (Canvas + semua komponen)
///   ── TutorialManager (controller + input detector)
/// </summary>
public class TutorialSceneBuilder : EditorWindow
{
    // ──────────────────────────────────────────────
    //  Sprite slots — assign di window sebelum klik Build
    // ──────────────────────────────────────────────
    private Sprite _keyA;
    private Sprite _keyD;
    private Sprite _keySpace;
    private Sprite _keyShift;
    private Sprite _keyEscape;
    private TMP_FontAsset _customFont;

    private bool _startOnAwake = true;
    private bool _showSkipButton = true;
    private float _barHeight = 90f;

    private Vector2 _scroll;

    // ──────────────────────────────────────────────
    //  Menu entry
    // ──────────────────────────────────────────────
    [MenuItem("Tools/Tutorial/Build Tutorial Scene")]
    public static void Open()
    {
        var window = GetWindow<TutorialSceneBuilder>("Tutorial Scene Builder");
        window.minSize = new Vector2(360f, 520f);
        window.Show();
    }

    // ──────────────────────────────────────────────
    //  GUI
    // ──────────────────────────────────────────────
    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawHeader("Sprite Tombol Keyboard");
        _keyA = (Sprite)EditorGUILayout.ObjectField("Key  A", _keyA, typeof(Sprite), false);
        _keyD = (Sprite)EditorGUILayout.ObjectField("Key  D", _keyD, typeof(Sprite), false);
        _keySpace = (Sprite)EditorGUILayout.ObjectField("Key  Space", _keySpace, typeof(Sprite), false);
        _keyShift = (Sprite)EditorGUILayout.ObjectField("Key  Shift", _keyShift, typeof(Sprite), false);
        _keyEscape = (Sprite)EditorGUILayout.ObjectField("Key  Escape", _keyEscape, typeof(Sprite), false);

        EditorGUILayout.Space(8);
        DrawHeader("Font & Pengaturan");
        _customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Custom Font (TMP)", _customFont, typeof(TMP_FontAsset), false);
        _startOnAwake = EditorGUILayout.Toggle("Start On Awake", _startOnAwake);
        _showSkipButton = EditorGUILayout.Toggle("Show Skip Button", _showSkipButton);
        _barHeight = EditorGUILayout.Slider("Bar Height", _barHeight, 60f, 140f);

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Sprite kosong akan memakai fallback kotak coklat berteks. " +
            "Kamu bisa assign sprite kapan saja lewat Inspector setelah build.",
            MessageType.Info);

        EditorGUILayout.Space(12);

        // Tombol utama
        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            fixedHeight = 36f
        };

        if (GUILayout.Button("Build Tutorial Scene", btnStyle))
            Build();

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Hapus Tutorial Objects dari Scene"))
            Cleanup();

        EditorGUILayout.EndScrollView();
    }

    // ──────────────────────────────────────────────
    //  Build
    // ──────────────────────────────────────────────
    private void Build()
    {
        // Cegah duplikasi
        if (GameObject.Find("TutorialCanvas") != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Sudah ada TutorialCanvas",
                "TutorialCanvas sudah ada di scene. Hapus dulu dan buat baru?",
                "Ya, buat ulang", "Batal"))
                return;

            Cleanup();
        }

        Undo.SetCurrentGroupName("Build Tutorial Scene");
        int group = Undo.GetCurrentGroup();

        // ── 1. TutorialCanvas ──────────────────────
        GameObject canvasGO = new GameObject("TutorialCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create TutorialCanvas");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Komponen Tutorial
        TutorialCanvasSetup setup = canvasGO.AddComponent<TutorialCanvasSetup>();
        setup.keyA = _keyA;
        setup.keyD = _keyD;
        setup.keySpace = _keySpace;
        setup.keyShift = _keyShift;
        setup.keyEscape = _keyEscape;
        setup.barHeight = _barHeight;
        if (_customFont != null) setup.customFont = _customFont;

        TutorialUI tutUI = canvasGO.AddComponent<TutorialUI>();

        // ── 2. EventSystem (kalau belum ada) ───────
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── 3. TutorialManager ─────────────────────
        GameObject mgrGO = new GameObject("TutorialManager");
        Undo.RegisterCreatedObjectUndo(mgrGO, "Create TutorialManager");

        TutorialInputDetector detector = mgrGO.AddComponent<TutorialInputDetector>();
        TutorialManager manager = mgrGO.AddComponent<TutorialManager>();

        // Hubungkan referensi via SerializedObject agar tercatat Undo
        SerializedObject soMgr = new SerializedObject(manager);
        soMgr.FindProperty("tutorialUI").objectReferenceValue = tutUI;
        soMgr.FindProperty("inputDetector").objectReferenceValue = detector;
        soMgr.FindProperty("showSkipButton").boolValue = _showSkipButton;
        soMgr.FindProperty("startOnAwake").boolValue = _startOnAwake;
        soMgr.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(group);

        // Pilih TutorialManager di Hierarchy supaya gampang lihat
        Selection.activeGameObject = mgrGO;

        SetScriptExecutionOrder();

        Debug.Log("[TutorialSceneBuilder] Selesai! TutorialCanvas & TutorialManager sudah dibuat.");
        EditorUtility.DisplayDialog(
            "Berhasil!",
            "TutorialCanvas dan TutorialManager sudah dibuat di scene.\n\n" +
            "Script Execution Order sudah diatur otomatis:\n" +
            "  TutorialCanvasSetup = -10\n" +
            "  TutorialUI          =   0\n\n" +
            "Assign sprite tombol lewat Inspector TutorialCanvasSetup jika belum.",
            "OK");
    }

    // ──────────────────────────────────────────────
    //  Script Execution Order — otomatis
    // ──────────────────────────────────────────────
    private static void SetScriptExecutionOrder()
    {
        foreach (MonoScript script in MonoImporter.GetAllRuntimeMonoScripts())
        {
            if (script.GetClass() == null) continue;

            if (script.GetClass() == typeof(TutorialCanvasSetup))
            {
                if (MonoImporter.GetExecutionOrder(script) != -10)
                    MonoImporter.SetExecutionOrder(script, -10);
            }
            else if (script.GetClass() == typeof(TutorialUI))
            {
                if (MonoImporter.GetExecutionOrder(script) != 0)
                    MonoImporter.SetExecutionOrder(script, 0);
            }
        }
    }

    // ──────────────────────────────────────────────
    //  Cleanup
    // ──────────────────────────────────────────────
    private static void Cleanup()
    {
        string[] targets = { "TutorialCanvas", "TutorialManager" };
        foreach (string name in targets)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
                Debug.Log($"[TutorialSceneBuilder] Dihapus: {name}");
            }
        }
    }

    // ──────────────────────────────────────────────
    //  UI Helper
    // ──────────────────────────────────────────────
    private static void DrawHeader(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        Rect r = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        EditorGUILayout.Space(4);
    }
}
#endif