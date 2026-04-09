using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static string nextSpawnPointID = "";

    [Header("Spawn Point Identifier")]
    [SerializeField] private string spawnPointID;

    private void Start()
    {
        // Cek apakah ini spawn point yang dituju
        if (!string.IsNullOrEmpty(nextSpawnPointID) && nextSpawnPointID == spawnPointID)
        {
            // Cari player dan pindahkan ke posisi spawn point ini
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
                Debug.Log($"Player spawned at: {spawnPointID}");
            }

            // Reset setelah dipakai
            nextSpawnPointID = "";
        }
    }

    private void OnDrawGizmos()
    {
        // Visual di scene editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"Spawn: {spawnPointID}");
#endif
    }
}