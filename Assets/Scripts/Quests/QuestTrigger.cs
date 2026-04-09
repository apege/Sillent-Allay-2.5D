using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("Quest yang di-complete trigger ini")]
    public string questID;

    public enum TriggerType { OnTriggerEnter, OnInteract, Manual }
    public TriggerType triggerType = TriggerType.OnTriggerEnter;

    [Header("Tag object yang bisa trigger (biasanya 'Player')")]
    public string targetTag = "Player";

    // ===================== COLLIDER TRIGGER =====================
    void OnTriggerEnter(Collider other)
    {
        if (triggerType != TriggerType.OnTriggerEnter) return;
        if (!other.CompareTag(targetTag)) return;

        TriggerComplete();
    }

    // ===================== MANUAL / NPC / ITEM =====================
    // Panggil fungsi ini dari script NPC atau Item pickup
    public void TriggerComplete()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManager tidak ditemukan!");
            return;
        }

        QuestManager.Instance.CompleteQuest(questID);

        // Optional: disable trigger biar ga dipanggil 2x
        gameObject.SetActive(false);
    }
}