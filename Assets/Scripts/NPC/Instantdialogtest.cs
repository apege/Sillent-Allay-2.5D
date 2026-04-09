using UnityEngine;
using TMPro;
using System.Collections;

// SCRIPT TEST: Dialog langsung muncul pas play mode
// Buat mastiin UI lu berfungsi 100%

public class InstantDialogTest : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;

    [Header("Test Settings")]
    public string testMessage = "Ehh Nara, Bagaimana Kabar mu Hari ini?";
    public string testName = "Bu Idah";

    void Start()
    {
        // Tunggu 0.5 detik biar semua object loaded
        StartCoroutine(ShowDialogDelayed());
    }

    IEnumerator ShowDialogDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("=== INSTANT DIALOG TEST ===");

        if (dialogBox == null)
        {
            Debug.LogError("DIALOGBOX NULL!");
            yield break;
        }

        if (dialogText == null)
        {
            Debug.LogError("DIALOGTEXT NULL!");
            yield break;
        }

        // Aktifkan dialog
        dialogBox.SetActive(true);
        Debug.Log("DialogBox activated");

        // Set canvas group alpha
        CanvasGroup cg = dialogBox.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = dialogBox.AddComponent<CanvasGroup>();
        }
        cg.alpha = 1f;

        // Set scale
        RectTransform rt = dialogBox.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
        }

        // Set text
        dialogText.text = testMessage;
        Debug.Log("Text set to: " + testMessage);

        if (npcNameText != null)
        {
            npcNameText.text = testName;
            Debug.Log("Name set to: " + testName);
        }

        Debug.Log("=== DIALOG SHOULD BE VISIBLE NOW ===");
        Debug.Log("Kalo masih ga keliatan, masalahnya di UI positioning atau Canvas settings!");
    }
}