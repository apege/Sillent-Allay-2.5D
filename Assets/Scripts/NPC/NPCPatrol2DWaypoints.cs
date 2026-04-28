using UnityEngine;

// NPC PATROL 2.5D - MULTIPLE WAYPOINTS
// Untuk game side-scrolling tapi dengan lebih dari 2 titik
// Misal: NPC jalan dari A → B → C → kembali ke A

public class NPCPatrol2DWaypoints : MonoBehaviour
{
    [Header("Waypoints (X Positions Only)")]
    public float[] waypointsX; // Array X positions
    public bool useLocalPosition = true; // Relative dari posisi awal
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float waitTimeAtWaypoint = 2f;
    public PatrolMode2D patrolMode = PatrolMode2D.Loop;
    
    [Header("Facing Direction")]
    public bool flipSprite = true;
    public SpriteRenderer spriteRenderer;
    public bool flipXScale = false;
    
    [Header("Animation (Optional)")]
    public Animator animator;
    public string walkAnimationName = "Walk";
    public string idleAnimationName = "Idle";
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    public enum PatrolMode2D
    {
        Loop,       // A → B → C → A
        PingPong,   // A → B → C → B → A
        Once        // A → B → C (stop)
    }
    
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool movingForward = true;
    private Vector3 startPosition;
    private float[] actualWaypointsX;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Calculate actual waypoint positions
        actualWaypointsX = new float[waypointsX.Length];
        for (int i = 0; i < waypointsX.Length; i++)
        {
            actualWaypointsX[i] = useLocalPosition ? 
                startPosition.x + waypointsX[i] : waypointsX[i];
        }
        
        if (waypointsX.Length == 0)
        {
            Debug.LogWarning($"[NPCPatrol2D] {gameObject.name} tidak punya waypoints!");
            enabled = false;
            return;
        }
        
        // Auto-detect SpriteRenderer
        if (flipSprite && spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
    
    void Update()
    {
        if (waypointsX.Length == 0) return;
        
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                NextWaypoint();
            }
            
            if (animator != null)
            {
                animator.Play(idleAnimationName);
            }
        }
        else
        {
            MoveToWaypoint();
        }
    }
    
    void MoveToWaypoint()
    {
        float targetX = actualWaypointsX[currentWaypointIndex];
        
        // Move towards target X
        float step = moveSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, step);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        
        // Update facing direction
        bool shouldFaceRight = targetX > transform.position.x;
        UpdateFacing(shouldFaceRight);
        
        // Play animation
        if (animator != null)
        {
            animator.Play(walkAnimationName);
        }
        
        // Check if reached
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            OnReachWaypoint();
        }
    }
    
    void UpdateFacing(bool faceRight)
    {
        if (flipSprite && spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }
        else if (flipXScale)
        {
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
    
    void OnReachWaypoint()
    {
        isWaiting = true;
    }
    
    void NextWaypoint()
    {
        switch (patrolMode)
        {
            case PatrolMode2D.Loop:
                currentWaypointIndex = (currentWaypointIndex + 1) % waypointsX.Length;
                break;
                
            case PatrolMode2D.PingPong:
                if (movingForward)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypointsX.Length - 1)
                    {
                        movingForward = false;
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex <= 0)
                    {
                        movingForward = true;
                    }
                }
                break;
                
            case PatrolMode2D.Once:
                if (currentWaypointIndex < waypointsX.Length - 1)
                {
                    currentWaypointIndex++;
                }
                else
                {
                    enabled = false; // Stop patrol
                }
                break;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || waypointsX == null || waypointsX.Length == 0) return;
        
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        
        // Draw waypoints and connections
        for (int i = 0; i < waypointsX.Length; i++)
        {
            float x = useLocalPosition ? startPos.x + waypointsX[i] : waypointsX[i];
            Vector3 waypointPos = new Vector3(x, startPos.y, startPos.z);
            
            // Draw waypoint
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(waypointPos, 0.3f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(waypointPos + Vector3.up * 0.5f, $"WP{i}");
            #endif
            
            // Draw line to next waypoint
            if (i < waypointsX.Length - 1)
            {
                float nextX = useLocalPosition ? startPos.x + waypointsX[i + 1] : waypointsX[i + 1];
                Vector3 nextPos = new Vector3(nextX, startPos.y, startPos.z);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypointPos, nextPos);
            }
        }
        
        // Draw loop connection
        if (patrolMode == PatrolMode2D.Loop && waypointsX.Length > 1)
        {
            float firstX = useLocalPosition ? startPos.x + waypointsX[0] : waypointsX[0];
            float lastX = useLocalPosition ? startPos.x + waypointsX[waypointsX.Length - 1] : waypointsX[waypointsX.Length - 1];
            
            Vector3 firstPos = new Vector3(firstX, startPos.y, startPos.z);
            Vector3 lastPos = new Vector3(lastX, startPos.y, startPos.z);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(lastPos, firstPos);
        }
        
        // Draw current target in play mode
        if (Application.isPlaying && waypointsX.Length > 0)
        {
            float targetX = actualWaypointsX[currentWaypointIndex];
            Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }
}
