using UnityEngine;

public class SimpleBackgroundParallax : MonoBehaviour
{
    [Header("Camera")]
    public Transform cam;

    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    [Tooltip("Seberapa cepat background ikut camera (0=diam, 1=ikut penuh)")]
    public float parallaxSpeed = 0.5f;

    [Header("Movement")]
    public bool followX = true;
    public bool followY = true;

    [Header("Scale Background to Fit Camera")]
    public bool autoScale = true;
    public float scaleMultiplier = 1.5f;

    [Header("Infinite Repeat")]
    public bool repeatInfiniteX = false;
    public bool repeatInfiniteY = false;

    private Vector3 startPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }

        startPos = transform.position;

        // Auto scale background biar pas dengan camera view
        if (autoScale)
        {
            ScaleToCamera();
        }

        // Hitung ukuran texture untuk infinite repeat
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            textureUnitSizeX = sprite.bounds.size.x;
            textureUnitSizeY = sprite.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Parallax movement
        Vector3 newPos = startPos;

        if (followX)
        {
            float parallaxX = cam.position.x * parallaxSpeed;
            newPos.x += parallaxX;
        }

        if (followY)
        {
            float parallaxY = cam.position.y * parallaxSpeed;
            newPos.y += parallaxY;
        }

        transform.position = newPos;

        // Infinite repeat X
        if (repeatInfiniteX)
        {
            float temp = cam.position.x * (1 - parallaxSpeed);

            if (temp > startPos.x + textureUnitSizeX)
            {
                startPos.x += textureUnitSizeX;
            }
            else if (temp < startPos.x - textureUnitSizeX)
            {
                startPos.x -= textureUnitSizeX;
            }
        }

        // Infinite repeat Y
        if (repeatInfiniteY)
        {
            float temp = cam.position.y * (1 - parallaxSpeed);

            if (temp > startPos.y + textureUnitSizeY)
            {
                startPos.y += textureUnitSizeY;
            }
            else if (temp < startPos.y - textureUnitSizeY)
            {
                startPos.y -= textureUnitSizeY;
            }
        }
    }

    void ScaleToCamera()
    {
        if (cam == null) return;

        // Hitung viewport size
        float height = 2f * cam.GetComponent<Camera>().orthographicSize;
        float width = height * cam.GetComponent<Camera>().aspect;

        // Get sprite size
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            // Scale sprite to fit camera view
            Vector3 spriteSize = sprite.bounds.size;

            float scaleX = (width / spriteSize.x) * scaleMultiplier;
            float scaleY = (height / spriteSize.y) * scaleMultiplier;

            // Pakai scale yang lebih besar biar cover semua
            float scale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    // Call ini kalau mau adjust scale
    public void UpdateScale()
    {
        if (autoScale)
        {
            ScaleToCamera();
        }
    }
}