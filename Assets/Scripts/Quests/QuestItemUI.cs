using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescText;
    public Image checkIcon;         // icon centang (aktifkan kalau complete)
    public Image overlayImage;      // overlay gelap (aktifkan kalau locked)
    public GameObject lockedBadge;  // optional: tulisan "LOCKED"

    public void Setup(QuestData data)
    {
        questNameText.text = data.questName;
        questDescText.text = data.questDescription;

        switch (data.status)
        {
            case QuestStatus.Locked:
                // Overlay gelap, sembunyiin centang
                if (overlayImage) overlayImage.gameObject.SetActive(true);
                if (checkIcon) checkIcon.gameObject.SetActive(false);
                if (lockedBadge) lockedBadge.SetActive(true);
                break;

            case QuestStatus.Active:
                // Normal, belum centang
                if (overlayImage) overlayImage.gameObject.SetActive(false);
                if (checkIcon) checkIcon.gameObject.SetActive(false);
                if (lockedBadge) lockedBadge.SetActive(false);
                break;

            case QuestStatus.Completed:
                // Centang muncul, overlay hilang
                if (overlayImage) overlayImage.gameObject.SetActive(false);
                if (checkIcon) checkIcon.gameObject.SetActive(true);
                if (lockedBadge) lockedBadge.SetActive(false);
                break;
        }
    }
}