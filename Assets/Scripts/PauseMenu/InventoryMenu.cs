using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class InventoryMenu : MonoBehaviour
{
    public GameObject[] Tabs;
    public Image[] TabsButton;
    public Sprite InactiveTabBG, ActiveTabBG;
    public Vector2 InactiveTabButtonSize, ActiveButtonSize;
    public GameObject InventoryPanel;
    private bool isOpen = false;

    void Start()
    {
        Debug.Log("InventoryPanel: " + InventoryPanel);
        InventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC kepencet! isOpen: " + isOpen);
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        Debug.Log("Toggle! sekarang isOpen: " + isOpen);
        InventoryPanel.SetActive(isOpen);
    }

    public void SwitchToTab(int TabID)
    {
        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
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