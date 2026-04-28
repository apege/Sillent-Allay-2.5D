using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;
    public bool lockZ = true;

    [Header("Camera Bounds (Optional)")]
    public bool useBounds = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("Look Ahead")]
    public bool useLookAhead = true;
    public float lookAheadDistance = 2f;
    public float lookAheadSpeed = 2f;
    public float lookAheadReturnSpeed = 1f;

    [Header("Dead Zone")]
    public bool useDeadZone = false;
    public float deadZoneWidth = 2f;
    public float deadZoneHeight = 1f;

    [Header("Camera Shake")]
    public float shakeDuration = 0f;
    public float shakeMagnitude = 0.1f;
    public float shakeFrequency = 25f;

    private Vector3 currentVelocity;
    private float lookAheadPos;
    private float shakeTimer;
    private Vector3 originalPos;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("Target tidak ditemukan! Assign player ke field Target atau beri tag 'Player' ke player GameObject.");
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = CalculateTargetPosition();

        // Apply bounds
        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothSpeed);

        // Apply camera shake
        if (shakeDuration > 0)
        {
            shakeTimer += Time.deltaTime * shakeFrequency;
            float shakeOffsetX = Mathf.Sin(shakeTimer) * shakeMagnitude;
            float shakeOffsetY = Mathf.Cos(shakeTimer * 1.3f) * shakeMagnitude;

            smoothedPosition += new Vector3(shakeOffsetX, shakeOffsetY, 0);

            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0)
            {
                shakeDuration = 0;
                shakeTimer = 0;
            }
        }

        transform.position = smoothedPosition;
    }

    Vector3 CalculateTargetPosition()
    {
        Vector3 targetPos = target.position + offset;

        // Dead Zone (kamera gak gerak kalau player di tengah)
        if (useDeadZone)
        {
            float deltaX = targetPos.x - transform.position.x;
            float deltaY = targetPos.y - transform.position.y;

            if (Mathf.Abs(deltaX) < deadZoneWidth / 2f)
            {
                targetPos.x = transform.position.x;
            }

            if (Mathf.Abs(deltaY) < deadZoneHeight / 2f)
            {
                targetPos.y = transform.position.y;
            }
        }

        // Look Ahead (kamera ngeliatin arah gerak player)
        if (useLookAhead)
        {
            float targetLookAhead = 0f;
            Rigidbody rb = target.GetComponent<Rigidbody>();

            if (rb != null)
            {
                if (rb.linearVelocity.x > 0.5f)
                {
                    targetLookAhead = lookAheadDistance;
                }
                else if (rb.linearVelocity.x < -0.5f)
                {
                    targetLookAhead = -lookAheadDistance;
                }
            }

            // Smooth look ahead transition
            float speed = (Mathf.Abs(targetLookAhead) > Mathf.Abs(lookAheadPos)) ? lookAheadSpeed : lookAheadReturnSpeed;
            lookAheadPos = Mathf.Lerp(lookAheadPos, targetLookAhead, Time.deltaTime * speed);
            targetPos.x += lookAheadPos;
        }

        // Lock Z axis untuk 2.5D
        if (lockZ)
        {
            targetPos.z = offset.z;
        }

        return targetPos;
    }

    // Function untuk trigger camera shake dari script lain
    public void ShakeCamera(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTimer = 0;
    }

    // Visualisasi bounds di Scene view
    void OnDrawGizmosSelected()
    {
        // Draw bounds
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }

        // Draw dead zone
        if (useDeadZone && target != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 deadZoneCenter = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f);
            Vector3 deadZoneSize = new Vector3(deadZoneWidth, deadZoneHeight, 0.1f);
            Gizmos.DrawWireCube(deadZoneCenter, deadZoneSize);
        }

        // Draw look ahead position
        if (useLookAhead && target != null && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector3 lookAheadPoint = new Vector3(target.position.x + lookAheadPos, target.position.y, target.position.z);
            Gizmos.DrawWireSphere(lookAheadPoint, 0.3f);
        }
    }
}