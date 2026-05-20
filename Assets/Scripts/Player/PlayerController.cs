using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float sprintSpeed = 12f;
    public float jumpForce = 15f;
    public float gravity = -30f;
    public float groundDrag = 6f;
    public float airDrag = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Jump Settings")]
    public int maxJumps = 2;
    private int jumpsRemaining;
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;
    private float lastAttackTime;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float lastDashTime;
    private bool isDashing;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Suara jalan di rumput / tanah biasa")]
    public AudioClip walkSound;
    [Tooltip("Suara lari di rumput / tanah biasa")]
    public AudioClip runSound;
    [Tooltip("Suara jalan khusus di dalam sekolah (ubin/beton)")]
    public AudioClip walkLantaiSound;
    [Tooltip("Suara lari khusus di dalam sekolah (ubin/beton)")]
    public AudioClip runLantaiSound;
    public float walkSoundInterval = 0.5f;  // Jeda antar step jalan
    public float runSoundInterval = 0.35f;   // Jeda antar step lari
    private float nextFootstepTime;

    // Input System Variables
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction dashAction;
    private InputAction sprintAction;

    // Private variables
    private Rigidbody rb;
    private bool isGrounded;
    private float horizontalInput;
    private bool isFacingRight = true;
    private Vector3 velocity;
    private bool isAttacking;
    private bool jumpPressed;

    void Awake()
    {
        // Get PlayerInput component
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component tidak ditemukan! Tambahkan PlayerInput component ke GameObject ini.");
            return;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError("Input Actions belum di-assign! Buat Input Actions asset dan assign ke PlayerInput component.");
            return;
        }

        // Bind input actions dengan error checking
        try
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
            dashAction = playerInput.actions["Dash"];
            sprintAction = playerInput.actions["Sprint"];

            // Subscribe to jump event
            jumpAction.performed += ctx => jumpPressed = true;
            jumpAction.canceled += ctx => jumpPressed = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error binding input actions: " + e.Message);
            Debug.LogError("Pastikan Input Actions memiliki: Move, Jump, Attack, Dash, dan Sprint actions!");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        jumpsRemaining = maxJumps;

        // SET INITIAL ROTATION KE 90° (default facing right)
        Vector3 rotation = transform.eulerAngles;
        rotation.y = 90f;
        transform.eulerAngles = rotation;
        isFacingRight = true;

        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = check.transform;
        }

        if (attackPoint == null)
        {
            GameObject point = new GameObject("AttackPoint");
            point.transform.parent = transform;
            point.transform.localPosition = new Vector3(1.5f, 0, 0);
            attackPoint = point.transform;
        }

        // Setup Audio Source kalo belum ada
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Pastikan audio source ga auto play
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (isDashing) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (moveAction != null)
        {
            horizontalInput = moveAction.ReadValue<Vector2>().x;
        }

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressed = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || jumpsRemaining > 0))
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        if (attackAction != null && attackAction.triggered && !isAttacking)
        {
            Attack();
        }

        if (dashAction != null && dashAction.triggered && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }

        // FIX BUAT Y = 90° DEFAULT
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 rotation = transform.eulerAngles;

            if (horizontalInput > 0) // Input D = kanan (maju)
            {
                rotation.y = 90f;  // Tetap di 90°
                isFacingRight = true;
            }
            else // Input A = kiri (mundur)
            {
                rotation.y = 270f; // Balik 180° dari 90°
                isFacingRight = false;
            }

            transform.eulerAngles = rotation;
        }

        PlayFootstepSound();  // PLAY FOOTSTEP SOUND DETECT GROUND
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        MovePlayer();
        ApplyGravity();
    }

    void MovePlayer()
    {
        float currentMoveSpeed = (sprintAction != null && sprintAction.IsPressed()) ? sprintSpeed : moveSpeed;

        float targetVelocityX = horizontalInput * currentMoveSpeed;
        float smoothing = isGrounded ? 10f : 8f;
        float newVelocityX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, Time.fixedDeltaTime * smoothing);

        rb.linearVelocity = new Vector3(newVelocityX, rb.linearVelocity.y, 0f);

        if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    void Jump()
    {
        if (isGrounded || coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsRemaining--;
            coyoteTimeCounter = 0f;
        }
        else if (jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * jumpForce * 0.9f, ForceMode.Impulse);
            jumpsRemaining--;
        }
    }

    void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Hit enemy: " + enemy.name);
        }
    }

    void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);

        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        Invoke("StopDash", dashDuration);
    }

    void StopDash()
    {
        isDashing = false;
    }

    void PlayFootstepSound()
    {
        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            if (Time.time >= nextFootstepTime)
            {
                bool isSprinting = (sprintAction != null && sprintAction.IsPressed());
                AudioClip clipToPlay = isSprinting ? runSound : walkSound;

                RaycastHit hit;
                if (Physics.Raycast(groundCheck.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.5f))
                {
                    if (hit.collider.CompareTag("Lantai"))
                    {
                        clipToPlay = isSprinting ? runLantaiSound : walkLantaiSound;
                    }
                }

                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }

                float interval = isSprinting ? runSoundInterval : walkSoundInterval;
                nextFootstepTime = Time.time + interval;
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDashing", isDashing);
        
        bool isSprinting = (sprintAction != null && sprintAction.IsPressed() && Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetBool("IsSprinting", isSprinting);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
