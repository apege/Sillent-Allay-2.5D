using UnityEngine;

// Script buat bikin UI World Space always face camera
// Attach ke Canvas yang pake World Space

public class BillboardUI : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("[Billboard] Main Camera not found!");
        }
    }
    
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Always face camera
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
