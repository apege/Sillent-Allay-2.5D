using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ============================================================
//  ConfirmationPanelBuilder.cs (v2 - Fixed Layout)
//  Auto-generate Panel Konfirmasi yang rapi.
//
//  CARA PAKAI:
//  1. Attach ke GameObject kosong "ConfirmationBuilder"
//  2. Isi Parent Canvas di Inspector
//  3. Klik kanan → "Build Confirmation Panel"
// ============================================================

public class ConfirmationPanelBuilder : MonoBehaviour
{
    [Header("=== Setup ===")]
    public Canvas parentCanvas;
    public string cutsceneSceneName = "cutscene";

    [Header("=== Hasil Build (otomatis) ===")]
    public GameObject confirmationPanel;
    public TMP_Text   txtCharacter;
    public TMP_Text   txtTrait;
    public TMP_Text   txtBadge1;
    public TMP_Text   txtBadge2;
    public Image      imgBadge1;
    public Image      imgBadge2;
    public Image      imgAvatar;

    // Warna
    private Color _gold       = new Color(0.83f, 0.67f, 0.38f);
    private Color _darkBg     = new Color(0.10f, 0.07f, 0.05f, 0.96f);
    private Color _cardBg     = new Color(1f, 1f, 1f, 0.04f);
    private Color _borderGold = new Color(0.70f, 0.55f, 0.31f, 0.25f);
    private Color _textMuted  = new Color(1f, 1f, 1f, 0.35f);
    private Color _textNormal = new Color(1f, 1f, 1f, 0.90f);
    private Color _greenBg    = new Color(0.37f, 0.61f, 0.13f, 0.25f);
    private Color _greenText  = new Color(0.55f, 0.80f, 0.33f, 1f);
    private Color _redBg      = new Color(0.78f, 0.23f, 0.23f, 0.25f);
    private Color _redText    = new Color(0.88f, 0.44f, 0.44f, 1f);

    // Ukuran modal
    private float _modalW = 400f;
    private float _modalH = 520f;

    // ============================================================
    [ContextMenu("Build Confirmation Panel")]
    public void BuildPanel()
    {
        if (parentCanvas == null)
        {
            Debug.LogError("[ConfirmationBuilder] Isi Parent Canvas dulu!");
            return;
        }

        // Hapus panel lama
        if (confirmationPanel != null)
            DestroyImmediate(confirmationPanel);

        // ── OVERLAY ──────────────────────────────────────────
        confirmationPanel = MakeGO("PanelConfirmation", parentCanvas.transform);
        Image overlay = confirmationPanel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.80f);
        Stretch(confirmationPanel);
        confirmationPanel.SetActive(false);

        // ── MODAL BOX ────────────────────────────────────────
        GameObject modal = MakeGO("ModalBox", confirmationPanel.transform);
        Image modalImg = modal.AddComponent<Image>();
        modalImg.color = _darkBg;
        Place(modal, 0, 0, _modalW, _modalH);

        // Border emas tipis via child image
        GameObject border = MakeGO("Border", modal.transform);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = _borderGold;
        Stretch(border, -1, -1, -1, -1); // 1px inset

        // ── JUDUL ────────────────────────────────────────────
        // "SIAP MEMULAI?" — posisi top
        TMP_Text judul = MakeText("TxtJudul", modal.transform,
            "SIAP MEMULAI?", 22f, _gold, TextAlignmentOptions.Center);
        Place(judul.gameObject, 0, 175f, _modalW - 40f, 36f);

        // Sub judul
        TMP_Text sub = MakeText("TxtSub", modal.transform,
            "Periksa pilihanmu sebelum melanjutkan", 12f, _textMuted, TextAlignmentOptions.Center);
        Place(sub.gameObject, 0, 140f, _modalW - 40f, 22f);

        // Divider
        MakeDivider("Div1", modal.transform, 108f);

        // ── CARD KARAKTER ────────────────────────────────────
        GameObject cardChar = MakeCard("CardKarakter", modal.transform, 0f, 58f, _modalW - 32f, 72f);

        // Avatar
        GameObject avatarGO = MakeGO("Avatar", cardChar.transform);
        imgAvatar = avatarGO.AddComponent<Image>();
        imgAvatar.color = new Color(0.70f, 0.55f, 0.31f, 0.2f);
        PlaceLocal(avatarGO, -(_modalW/2 - 16f - 28f), 0, 56f, 56f); // kiri

        // Label "KARAKTER"
        TMP_Text labelChar = MakeText("LabelKarakter", cardChar.transform,
            "KARAKTER", 9f, _textMuted, TextAlignmentOptions.Left);
        PlaceLocal(labelChar.gameObject, 12f, 14f, 200f, 16f);

        // Nama karakter
        txtCharacter = MakeText("TxtCharacter", cardChar.transform,
            "Nara", 17f, _textNormal, TextAlignmentOptions.Left);
        txtCharacter.fontStyle = FontStyles.Bold;
        PlaceLocal(txtCharacter.gameObject, 12f, -6f, 200f, 26f);

        // ── CARD TRAIT ───────────────────────────────────────
        GameObject cardTrait = MakeCard("CardTrait", modal.transform, 0f, -28f, _modalW - 32f, 72f);

        // Label "SIFAT"
        TMP_Text labelTrait = MakeText("LabelTrait", cardTrait.transform,
            "SIFAT", 9f, _textMuted, TextAlignmentOptions.Left);
        PlaceLocal(labelTrait.gameObject, -(_modalW/2 - 16f - 100f), 14f, 200f, 16f);

        // Nama trait
        txtTrait = MakeText("TxtTrait", cardTrait.transform,
            "Introvert", 17f, _textNormal, TextAlignmentOptions.Left);
        txtTrait.fontStyle = FontStyles.Bold;
        PlaceLocal(txtTrait.gameObject, -(_modalW/2 - 16f - 100f), -6f, 200f, 26f);

        // Badge 1
        GameObject b1GO = MakeBadgeGO("Badge1", cardTrait.transform, -50f, -26f, _greenBg);
        imgBadge1 = b1GO.GetComponent<Image>();
        txtBadge1 = MakeText("Txt", b1GO.transform, "Healing +30%", 10f, _greenText, TextAlignmentOptions.Center);
        Stretch(txtBadge1.gameObject, 8, 8, 3, 3);

        // Badge 2
        GameObject b2GO = MakeBadgeGO("Badge2", cardTrait.transform, 60f, -26f, _redBg);
        imgBadge2 = b2GO.GetComponent<Image>();
        txtBadge2 = MakeText("Txt", b2GO.transform, "Courage lambat", 10f, _redText, TextAlignmentOptions.Center);
        Stretch(txtBadge2.gameObject, 8, 8, 3, 3);

        // ── DIVIDER ──────────────────────────────────────────
        MakeDivider("Div2", modal.transform, -108f);

        // ── WARNING ──────────────────────────────────────────
        GameObject warnBox = MakeGO("WarningBox", modal.transform);
        Image warnImg = warnBox.AddComponent<Image>();
        warnImg.color = new Color(0.70f, 0.55f, 0.31f, 0.06f);
        Place(warnBox, 0, -148f, _modalW - 32f, 44f);

        TMP_Text warnTxt = MakeText("TxtWarning", warnBox.transform,
            "Setelah memulai, sifat tidak dapat diubah.  Pilih dengan bijak.",
            11f, _textMuted, TextAlignmentOptions.Center);
        Stretch(warnTxt.gameObject, 10, 10, 4, 4);

        // ── TOMBOL ───────────────────────────────────────────
        // Kembali
        GameObject btnBackGO = MakeGO("BtnBack", modal.transform);
        Image btnBackImg = btnBackGO.AddComponent<Image>();
        btnBackImg.color = new Color(1f, 1f, 1f, 0.05f);
        Button btnBack = btnBackGO.AddComponent<Button>();
        Place(btnBackGO, -90f, -185f, 120f, 40f);
        TMP_Text lblBack = MakeText("Lbl", btnBackGO.transform, "← Kembali", 13f,
            new Color(1f,1f,1f,0.45f), TextAlignmentOptions.Center);
        Stretch(lblBack.gameObject);
        btnBack.onClick.AddListener(OnBackClicked);

        // Mulai
        GameObject btnStartGO = MakeGO("BtnStart", modal.transform);
        Image btnStartImg = btnStartGO.AddComponent<Image>();
        btnStartImg.color = new Color(0.70f, 0.55f, 0.31f, 0.18f);
        Button btnStart = btnStartGO.AddComponent<Button>();
        Place(btnStartGO, 80f, -185f, 160f, 40f);
        TMP_Text lblStart = MakeText("Lbl", btnStartGO.transform, "Mulai Game →", 13f,
            _gold, TextAlignmentOptions.Center);
        lblStart.fontStyle = FontStyles.Bold;
        Stretch(lblStart.gameObject);
        btnStart.onClick.AddListener(OnStartClicked);

        Debug.Log("[ConfirmationBuilder] ✓ Panel konfirmasi berhasil dibuat!");
    }

    // ============================================================
    //  TAMPILKAN / SEMBUNYIKAN
    // ============================================================
    public void ShowPanel(string characterName, string traitName,
                          string badge1Txt, bool badge1IsPositive,
                          string badge2Txt, bool badge2IsPositive,
                          Sprite avatarSprite = null)
    {
        if (confirmationPanel == null) BuildPanel();
        confirmationPanel.SetActive(true);

        if (txtCharacter != null) txtCharacter.text = characterName;
        if (txtTrait     != null) txtTrait.text     = traitName;

        if (txtBadge1 != null) txtBadge1.text  = badge1Txt;
        if (imgBadge1 != null) imgBadge1.color = badge1IsPositive ? _greenBg : _redBg;
        if (txtBadge1 != null) txtBadge1.color = badge1IsPositive ? _greenText : _redText;

        if (txtBadge2 != null) txtBadge2.text  = badge2Txt;
        if (imgBadge2 != null) imgBadge2.color = badge2IsPositive ? _greenBg : _redBg;
        if (txtBadge2 != null) txtBadge2.color = badge2IsPositive ? _greenText : _redText;

        if (imgAvatar != null && avatarSprite != null)
            imgAvatar.sprite = avatarSprite;
    }

    public void HidePanel()
    {
        confirmationPanel?.SetActive(false);
    }

    // ============================================================
    //  BUTTON EVENTS
    // ============================================================
    private void OnBackClicked()
    {
        HidePanel();
        Object.FindFirstObjectByType<CharacterSelectManager>()?.OnConfirmationBack();
    }

    private void OnStartClicked()
    {
        HidePanel();
        Object.FindFirstObjectByType<CharacterSelectManager>()?.OnConfirmationStart();
    }

    // ============================================================
    //  HELPER FUNCTIONS
    // ============================================================
    GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    TMP_Text MakeText(string name, Transform parent, string text,
        float size, Color color, TextAlignmentOptions align)
    {
        var go  = MakeGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = align;
        return tmp;
    }

    // Posisi anchor center, pivot center
    void Place(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // Posisi lokal (untuk child di dalam card)
    void PlaceLocal(GameObject go, float x, float y, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // Stretch penuh ke parent
    void Stretch(GameObject go, float l=0, float r=0, float t=0, float b=0)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = new Vector2(l, b);
        rt.offsetMax  = new Vector2(-r, -t);
    }

    // Card dengan background
    GameObject MakeCard(string name, Transform parent, float x, float y, float w, float h)
    {
        var go  = MakeGO(name, parent);
        var img = go.AddComponent<Image>();
        img.color = _cardBg;
        Place(go, x, y, w, h);
        return go;
    }

    // Badge rounded (kotak kecil warna)
    GameObject MakeBadgeGO(string name, Transform parent, float x, float y, Color bg)
    {
        var go  = MakeGO(name, parent);
        var img = go.AddComponent<Image>();
        img.color = bg;
        PlaceLocal(go, x, y, 110f, 22f);
        return go;
    }

    // Garis pemisah horizontal
    void MakeDivider(string name, Transform parent, float y)
    {
        var go  = MakeGO(name, parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.70f, 0.55f, 0.31f, 0.15f);
        Place(go, 0, y, _modalW - 32f, 1f);
    }
}