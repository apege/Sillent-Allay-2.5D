using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform;
    public float parallaxFactor; // 0 = jauh, 1 = dekat
}

public class ParallaxBackground : MonoBehaviour
{
    public ParallaxLayer[] layers;
    private Vector3 lastCameraPos;

    void Start()
    {
        lastCameraPos = Camera.main.transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = Camera.main.transform.position - lastCameraPos;

        foreach (var layer in layers)
        {
            Vector3 layerMove = new Vector3(delta.x * layer.parallaxFactor, delta.y * layer.parallaxFactor, 0);
            layer.layerTransform.position += layerMove;
        }

        lastCameraPos = Camera.main.transform.position;
    }
}
