using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questID;        // ID unik, e.g. "quest_01"
    public string questName;
    [TextArea] public string questDescription;

    [Header("Status")]
    public QuestStatus status = QuestStatus.Locked;
}

public enum QuestStatus
{
    Locked,     // belum bisa dikerjain
    Active,     // sedang berjalan
    Completed   // selesai
}