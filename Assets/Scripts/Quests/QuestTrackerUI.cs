using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    [Header("Assign ini ke object Text di QuestListContainer")]
    public TextMeshProUGUI questText;

    void Start()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += RefreshUI;

        Invoke("RefreshUI", 0.1f);
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= RefreshUI;
    }

    void RefreshUI()
    {
        if (QuestManager.Instance == null) return;

        string result = "";
        foreach (var quest in QuestManager.Instance.allQuests)
        {
            if (quest.status == QuestStatus.Active)
                result += "- " + quest.questName + "\n";
        }

        questText.text = result == "" ? "" : result;
    }
}