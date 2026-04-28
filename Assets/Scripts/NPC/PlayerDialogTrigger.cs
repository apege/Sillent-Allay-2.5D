using UnityEngine;

public class PlayerDialogTrigger : MonoBehaviour
{
    // Script ini opsional, bisa dipake di player buat detect NPC terdekat
    // Berguna kalo mau tampilin indicator kayak "Press E to talk"
    
    [Header("Detection Settings")]
    public float detectionRadius = 3f;
    public LayerMask npcLayer;
    
    [Header("UI Indicator (Optional)")]
    public GameObject interactIndicator; // Misalnya icon "E" atau "Press E"
    
    private NPCDialogSystem nearestNPC;
    
    void Update()
    {
        DetectNearbyNPC();
    }
    
    void DetectNearbyNPC()
    {
        Collider[] npcs = Physics.OverlapSphere(transform.position, detectionRadius, npcLayer);
        
        float nearestDistance = Mathf.Infinity;
        NPCDialogSystem closestNPC = null;
        
        foreach (Collider npc in npcs)
        {
            NPCDialogSystem dialogSystem = npc.GetComponent<NPCDialogSystem>();
            if (dialogSystem != null)
            {
                float distance = Vector3.Distance(transform.position, npc.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    closestNPC = dialogSystem;
                }
            }
        }
        
        nearestNPC = closestNPC;
        
        // Show/hide indicator
        if (interactIndicator != null)
        {
            interactIndicator.SetActive(nearestNPC != null);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
