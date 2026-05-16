using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryMenu : MonoBehaviour
{
    public static InventoryMenu Instance { get; private set; }

    public GameObject[] Tabs;
    public Image[] TabsButton;
    public Sprite InactiveTabBG, ActiveTabBG;
    public Vector2 InactiveTabButtonSize, ActiveButtonSize;
    public GameObject InventoryPanel;
    private bool isOpen = false;

    void Awake()
    {
        Debug.Log($"[InventoryMenu] Awake dipanggil! Instance ada: {Instance != null}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[InventoryMenu] Duplikat ditemukan, destroy!");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.transform.root.gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isOpen = false;
        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);

        // ← 3 baris tambahan
        if (QuestCanvasManager.Instance != null)
            QuestCanvasManager.Instance.gameObject.SetActive(scene.name != "StartMenu");
    }

    void Start()
    {
        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "StartMenu") return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (InventoryPanel == null)
        {
            Debug.LogError("InventoryPanel null!", this);
            return;
        }

        isOpen = !isOpen;
        InventoryPanel.SetActive(isOpen);
    }

    public void SwitchToTab(int TabID)
    {
        foreach (GameObject go in Tabs)
            go.SetActive(false);
        Tabs[TabID].SetActive(true);

        foreach (Image im in TabsButton)
        {
            im.sprite = InactiveTabBG;
            im.rectTransform.sizeDelta = InactiveTabButtonSize;
        }
        TabsButton[TabID].sprite = ActiveTabBG;
        TabsButton[TabID].rectTransform.sizeDelta = ActiveButtonSize;
    }
}