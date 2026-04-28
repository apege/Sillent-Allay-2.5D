using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest List (drag semua QuestData di sini, urut dari quest 1)")]
    public QuestData[] allQuests;

    public event Action OnQuestUpdated;

    private string SavePath => Application.persistentDataPath + "/quest_save.json";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadQuests();

        // Quest pertama auto Active kalau masih Locked
        if (allQuests.Length > 0 && allQuests[0].status == QuestStatus.Locked)
        {
            allQuests[0].status = QuestStatus.Active;
            SaveQuests();
        }
    }

    // Dipanggil dari QuestTrigger
    public void CompleteQuest(string questID)
    {
        for (int i = 0; i < allQuests.Length; i++)
        {
            if (allQuests[i].questID == questID && allQuests[i].status == QuestStatus.Active)
            {
                allQuests[i].status = QuestStatus.Completed;

                // Unlock quest berikutnya
                if (i + 1 < allQuests.Length)
                    allQuests[i + 1].status = QuestStatus.Active;

                SaveQuests();
                OnQuestUpdated?.Invoke();

                Debug.Log($"Quest '{allQuests[i].questName}' selesai!");
                break;
            }
        }
    }

    // ===================== SAVE / LOAD =====================
    [Serializable]
    private class QuestSaveData
    {
        public string questID;
        public QuestStatus status;
    }

    [Serializable]
    private class SaveWrapper
    {
        public List<QuestSaveData> quests = new List<QuestSaveData>();
    }

    public void SaveQuests()
    {
        SaveWrapper wrapper = new SaveWrapper();
        foreach (var q in allQuests)
            wrapper.quests.Add(new QuestSaveData { questID = q.questID, status = q.status });

        File.WriteAllText(SavePath, JsonUtility.ToJson(wrapper, true));
        Debug.Log("Quest saved to: " + SavePath);
    }

    public void LoadQuests()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(json);

        foreach (var savedQuest in wrapper.quests)
        {
            foreach (var q in allQuests)
            {
                if (q.questID == savedQuest.questID)
                {
                    q.status = savedQuest.status;
                    break;
                }
            }
        }

        Debug.Log("Quest loaded!");
    }

    // Untuk debug / testing di Inspector
    [ContextMenu("Reset All Quests")]
    public void ResetAllQuests()
    {
        foreach (var q in allQuests)
            q.status = QuestStatus.Locked;

        if (allQuests.Length > 0)
            allQuests[0].status = QuestStatus.Active;

        SaveQuests();
        OnQuestUpdated?.Invoke();
    }
}