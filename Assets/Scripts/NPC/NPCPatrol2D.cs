using UnityEngine;

// NPC PATROL 2.5D - KHUSUS GAME SIDE-SCROLLING
// NPC cuma jalan kiri-kanan (X axis) kayak Swordigo
// Cocok buat game 2.5D platformer

public class NPCPatrol2D : MonoBehaviour
{
    [Header("Patrol Points (Kiri-Kanan)")]
    public float leftPoint = -5f; // Titik paling kiri (X position)
    public float rightPoint = 5f; // Titik paling kanan (X position)
    public bool useLocalPosition = true; // Relative dari posisi awal NPC

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 0f; // Tunggu berapa lama di ujung (set 0 biar langsung balik)

    [Header("Facing Direction")]
    public FacingMode facingMode = FacingMode.RotateY;
    public enum FacingMode { FlipSprite, FlipXScale, RotateY }
    public SpriteRenderer spriteRenderer;
    public float rightRotationY = -270f;
    public float leftRotationY = -90f;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string walkAnimationName = "Walk";
    public string idleAnimationName = "Idle";

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.yellow;

    private float targetX;
    private bool movingRight = true;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private Vector3 startPosition;
    private float actualLeftPoint;
    private float actualRightPoint;

    void Start()
    {
        startPosition = transform.position;

        // Calculate actual points
        if (useLocalPosition)
        {
            actualLeftPoint = startPosition.x + leftPoint;
            actualRightPoint = startPosition.x + rightPoint;
        }
        else
        {
            actualLeftPoint = leftPoint;
            actualRightPoint = rightPoint;
        }

        // Set target awal (mulai ke kanan)
        targetX = actualRightPoint;
        movingRight = true;

        // Auto-detect SpriteRenderer kalo belum di-set
        if (facingMode == FacingMode.FlipSprite && spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Set facing awal
        UpdateFacing();
    }

    void Update()
    {
        if (isWaiting)
        {
            // Tunggu di ujung
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtPoint)
            {
                // Selesai tunggu, balik arah
                isWaiting = false;
                waitTimer = 0f;
                SwitchDirection();
            }

            // Play idle animation
            if (animator != null)
            {
                animator.Play(idleAnimationName);
            }
        }
        else
        {
            // Jalan ke target
            Move();
        }
    }

    void Move()
    {
        // Gerak ke target X (keep Y dan Z tetap)
        float step = moveSpeed * Time.deltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, step);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Play walk animation
        if (animator != null)
        {
            animator.Play(walkAnimationName);
        }

        // Update facing direction
        UpdateFacing();

        // Cek udah sampai target belum
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
        {
            // Sampai ujung!
            OnReachPoint();
        }
    }

    void UpdateFacing()
    {
        switch (facingMode)
        {
            case FacingMode.FlipSprite:
                if (spriteRenderer != null)
                    spriteRenderer.flipX = !movingRight;
                break;

            case FacingMode.FlipXScale:
                Vector3 scale = transform.localScale;
                scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
                break;

            case FacingMode.RotateY:
                Vector3 rotation = transform.eulerAngles;
                rotation.y = movingRight ? rightRotationY : leftRotationY;
                transform.eulerAngles = rotation;
                break;
        }
    }

    void OnReachPoint()
    {
        isWaiting = true;
    }

    void SwitchDirection()
    {
        movingRight = !movingRight;
        targetX = movingRight ? actualRightPoint : actualLeftPoint;
        UpdateFacing(); // ✅ LANGSUNG UPDATE FACING PAS GANTI ARAH!
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;

        float leftX = useLocalPosition ? startPos.x + leftPoint : leftPoint;
        float rightX = useLocalPosition ? startPos.x + rightPoint : rightPoint;

        // Draw patrol line
        Gizmos.color = gizmoColor;
        Vector3 leftPos = new Vector3(leftX, startPos.y, startPos.z);
        Vector3 rightPos = new Vector3(rightX, startPos.y, startPos.z);

        Gizmos.DrawLine(leftPos, rightPos);

        // Draw end points
        Gizmos.DrawWireSphere(leftPos, 0.3f);
        Gizmos.DrawWireSphere(rightPos, 0.3f);

        // Draw current position in play mode
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            // Draw target
            Gizmos.color = Color.red;
            Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
            Gizmos.DrawLine(transform.position, targetPos);
        }

        // Draw labels
#if UNITY_EDITOR
        UnityEditor.Handles.Label(leftPos + Vector3.up, "LEFT");
        UnityEditor.Handles.Label(rightPos + Vector3.up, "RIGHT");
#endif
    }
}