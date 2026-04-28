using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [Header("Prefab 1 baris quest")]
    public GameObject questItemPrefab;  // prefab card quest

    [Header("Parent scroll content")]
    public Transform questListParent;   // Content di dalam ScrollView

    void OnEnable()
    {
        RefreshUI();
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += RefreshUI;
    }

    void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= RefreshUI;
    }

    public void RefreshUI()
    {
        // Bersihkan list lama
        foreach (Transform child in questListParent)
            Destroy(child.gameObject);

        if (QuestManager.Instance == null) return;

        foreach (var quest in QuestManager.Instance.allQuests)
        {
            GameObject item = Instantiate(questItemPrefab, questListParent);
            QuestItemUI ui = item.GetComponent<QuestItemUI>();
            if (ui != null) ui.Setup(quest);
        }
    }
}