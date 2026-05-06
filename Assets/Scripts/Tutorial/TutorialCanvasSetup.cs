using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tutorial
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class TutorialCanvasSetup : MonoBehaviour
    {
        [Header("Sprite Tombol")]
        public Sprite keyA;
        public Sprite keyD;
        public Sprite keySpace;
        public Sprite keyShift;
        public Sprite keyEscape;

        [Header("Font (Opsional)")]
        public TMP_FontAsset customFont;

        [Header("Warna")]
        public Color barColor = new Color(0.04f, 0.02f, 0.01f, 0.93f);
        public Color borderColor = new Color(0.48f, 0.31f, 0.10f, 1f);
        public Color textColorMain = new Color(0.94f, 0.85f, 0.60f, 1f);
        public Color textColorMuted = new Color(0.63f, 0.36f, 0.16f, 1f);
        public Color dotActiveColor = new Color(0.77f, 0.49f, 0.19f, 1f);
        public Color dotDoneColor = new Color(0.35f, 0.23f, 0.10f, 1f);
        public Color dotInactiveColor = new Color(0.23f, 0.13f, 0.06f, 1f);

        [Header("Ukuran")]
        public float barHeight = 90f;
        public float keySize = 60f;
        public float keyWideSize = 100f;

        // Referensi publik — diisi BuildHierarchy(), dibaca TutorialUI
        [HideInInspector] public CanvasGroup panelCanvasGroup;
        [HideInInspector] public TextMeshProUGUI stepLabelText;
        [HideInInspector] public TextMeshProUGUI instructionText;
        [HideInInspector] public Button skipButton;
        [HideInInspector] public Transform keysContainer;
        [HideInInspector] public Transform dotsContainer;

        private void Awake()
        {
            ConfigureCanvas();
            BuildHierarchy();
        }

        public void ShowKeysForAction(TutorialActionType action)
        {
            foreach (Transform child in keysContainer)
                Destroy(child.gameObject);

            switch (action)
            {
                case TutorialActionType.MoveHorizontal:
                    AddKey(keyA, keySize, "A");
                    AddSpacer(8f);
                    AddKey(keyD, keySize, "D");
                    break;
                case TutorialActionType.Jump:
                    AddKey(keySpace, keyWideSize, "Space");
                    break;
                case TutorialActionType.Sprint:
                    AddKey(keyShift, keyWideSize, "Shift");
                    AddSpacer(4f); AddPlus(); AddSpacer(4f);
                    AddKey(keyA, keySize, "A");
                    AddSpacer(4f);
                    AddKey(keyD, keySize, "D");
                    break;
                case TutorialActionType.OpenMenu:
                    AddKey(keyEscape, keySize, "Esc");
                    break;
                case TutorialActionType.UIIntroduction:
                    break;
            }
        }

        public void UpdateStepDots(int current, int total)
        {
            foreach (Transform child in dotsContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < total; i++)
            {
                GameObject dot = CreateObj($"Dot_{i}", dotsContainer);
                dot.GetComponent<RectTransform>().sizeDelta = new Vector2(7f, 7f);
                Image img = dot.AddComponent<Image>();
                img.color = i < current ? dotDoneColor :
                            i == current ? dotActiveColor :
                                           dotInactiveColor;
            }
        }

        private void ConfigureCanvas()
        {
            var c = GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;

            var cs = GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
        }

        private void BuildHierarchy()
        {
            // Panel
            GameObject panel = CreateObj("TutorialPanel", transform);
            panelCanvasGroup = panel.AddComponent<CanvasGroup>();
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 0f);
            panelRT.anchorMax = new Vector2(1f, 0f);
            panelRT.pivot = new Vector2(0.5f, 0f);
            panelRT.sizeDelta = new Vector2(0f, barHeight);
            panelRT.anchoredPosition = Vector2.zero;

            // BG
            var bg = CreateObj("BG", panel.transform);
            bg.AddComponent<Image>().color = barColor;
            Stretch(bg.GetComponent<RectTransform>());

            // Border
            var bdr = CreateObj("TopBorder", panel.transform);
            bdr.AddComponent<Image>().color = borderColor;
            var bdrRT = bdr.GetComponent<RectTransform>();
            bdrRT.anchorMin = new Vector2(0, 1); bdrRT.anchorMax = new Vector2(1, 1);
            bdrRT.pivot = new Vector2(0.5f, 1f); bdrRT.sizeDelta = new Vector2(0, 2);
            bdrRT.anchoredPosition = Vector2.zero;

            // HLayout
            var hRoot = CreateObj("HLayout", panel.transform);
            Stretch(hRoot.GetComponent<RectTransform>());
            var hlg = hRoot.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(24, 20, 12, 12);
            hlg.spacing = 20f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;

            // Instruction Area
            var ia = CreateObj("InstructionArea", hRoot.transform);
            ia.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var vlg = ia.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            // Dots
            var dotsGO = CreateObj("StepDots", ia.transform);
            dotsContainer = dotsGO.transform;
            var dhlg = dotsGO.AddComponent<HorizontalLayoutGroup>();
            dhlg.spacing = 5f; dhlg.childForceExpandWidth = false; dhlg.childForceExpandHeight = false;
            dotsGO.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            dotsGO.AddComponent<LayoutElement>().preferredHeight = 14f;

            // StepLabel
            var slGO = CreateObj("StepLabel", ia.transform);
            stepLabelText = slGO.AddComponent<TextMeshProUGUI>();
            stepLabelText.text = "Langkah 1 dari 5";
            stepLabelText.fontSize = 18f;
            stepLabelText.color = textColorMuted;
            stepLabelText.characterSpacing = 3f;
            if (customFont) stepLabelText.font = customFont;
            slGO.AddComponent<LayoutElement>().preferredHeight = 14f;

            // InstructionText
            var itGO = CreateObj("InstructionText", ia.transform);
            instructionText = itGO.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Tekan [A] atau [D] untuk bergerak";
            instructionText.fontSize = 32f;
            instructionText.color = textColorMain;
            if (customFont) instructionText.font = customFont;
            itGO.AddComponent<LayoutElement>().preferredHeight = 24f;

            // KeysContainer
            var keysGO = CreateObj("KeysContainer", hRoot.transform);
            keysContainer = keysGO.transform;
            var khlg = keysGO.AddComponent<HorizontalLayoutGroup>();
            khlg.spacing = 8f; khlg.childAlignment = TextAnchor.MiddleCenter;
            khlg.childForceExpandWidth = false; khlg.childForceExpandHeight = false;
            var kcf = keysGO.AddComponent<ContentSizeFitter>();
            kcf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            kcf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            keysGO.AddComponent<LayoutElement>().flexibleWidth = 0f;

            // Divider
            var div = CreateObj("Divider", hRoot.transform);
            div.AddComponent<Image>().color = new Color(0.23f, 0.13f, 0.06f, 1f);
            var divLE = div.AddComponent<LayoutElement>();
            divLE.preferredWidth = 1f; divLE.flexibleHeight = 1f;

            // Skip
            var skipGO = CreateObj("SkipButton", hRoot.transform);
            skipButton = skipGO.AddComponent<Button>();
            skipGO.AddComponent<Image>().color = Color.clear;
            var skipLE = skipGO.AddComponent<LayoutElement>();
            skipLE.preferredWidth = 48f; skipLE.preferredHeight = 28f;

            var skipLbl = CreateObj("Label", skipGO.transform);
            var skipTMP = skipLbl.AddComponent<TextMeshProUGUI>();
            skipTMP.text = "SKIP"; skipTMP.fontSize = 10f;
            skipTMP.color = textColorMuted;
            skipTMP.characterSpacing = 2f;
            skipTMP.alignment = TextAlignmentOptions.Center;
            if (customFont) skipTMP.font = customFont;
            Stretch(skipLbl.GetComponent<RectTransform>());

            UpdateStepDots(0, 5);
        }

        private void AddKey(Sprite sprite, float w, string label)
        {
            var wrap = CreateObj($"Key_{label}", keysContainer);
            var vg = wrap.AddComponent<VerticalLayoutGroup>();
            vg.spacing = 4f; vg.childAlignment = TextAnchor.UpperCenter;
            vg.childForceExpandWidth = false; vg.childForceExpandHeight = false;
            wrap.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var imgGO = CreateObj("Img", wrap.transform);
            var img = imgGO.AddComponent<Image>();
            if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
            else
            {
                img.color = new Color(0.77f, 0.49f, 0.19f, 1f);
                var fb = CreateObj("FbText", imgGO.transform);
                var t = fb.AddComponent<TextMeshProUGUI>();
                t.text = label; t.fontSize = 20f;
                t.color = new Color(0.96f, 0.82f, 0.44f, 1f);
                t.alignment = TextAlignmentOptions.Center;
                if (customFont) t.font = customFont;
                Stretch(fb.GetComponent<RectTransform>());
            }
            var le = imgGO.AddComponent<LayoutElement>();
            le.preferredWidth = w; le.preferredHeight = keySize;

            var lbl = CreateObj("Lbl", wrap.transform);
            var lt = lbl.AddComponent<TextMeshProUGUI>();
            lt.text = label.ToUpper(); lt.fontSize = 14f;
            lt.color = textColorMuted;
            lt.alignment = TextAlignmentOptions.Center;
            lt.characterSpacing = 1f;
            if (customFont) lt.font = customFont;
            lbl.AddComponent<LayoutElement>().preferredHeight = 12f;
        }

        private void AddPlus()
        {
            var go = CreateObj("Plus", keysContainer);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = "+"; t.fontSize = 14f;
            t.color = textColorMuted;
            t.alignment = TextAlignmentOptions.Center;
            if (customFont) t.font = customFont;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 16f; le.preferredHeight = keySize;
        }

        private void AddSpacer(float w)
        {
            var go = CreateObj("Spacer", keysContainer);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = w; le.preferredHeight = 1f;
        }

        private static GameObject CreateObj(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}