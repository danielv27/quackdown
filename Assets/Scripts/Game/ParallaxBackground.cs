using UnityEngine;

/// <summary>
/// Simple parallax background with multiple layers.
/// Each layer scrolls at a fraction of the camera speed for depth effect.
/// Attach to a parent container with child layer sprites.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        [Range(0f, 1f)]
        [Tooltip("0 = fixed (sky), 1 = moves with camera")]
        public float parallaxFactor = 0.3f;
        public bool loopHorizontally = true;
        [HideInInspector] public float startX;
        [HideInInspector] public float spriteWidth;
    }

    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private Camera targetCamera;

    private Vector3 previousCamPos;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null) return;

        previousCamPos = targetCamera.transform.position;

        foreach (var layer in layers)
        {
            if (layer.layerTransform == null) continue;
            layer.startX = layer.layerTransform.position.x;

            // Get sprite width for seamless looping
            SpriteRenderer sr = layer.layerTransform.GetComponent<SpriteRenderer>();
            layer.spriteWidth = sr != null ? sr.bounds.size.x : 20f;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 delta = targetCamera.transform.position - previousCamPos;
        previousCamPos = targetCamera.transform.position;

        foreach (var layer in layers)
        {
            if (layer.layerTransform == null) continue;

            // Move layer by fraction of camera movement
            Vector3 pos = layer.layerTransform.position;
            pos.x += delta.x * layer.parallaxFactor;
            layer.layerTransform.position = pos;

            // Seamless horizontal loop
            if (layer.loopHorizontally && layer.spriteWidth > 0f)
            {
                float distFromCam = layer.layerTransform.position.x - targetCamera.transform.position.x;
                if (Mathf.Abs(distFromCam) > layer.spriteWidth * 0.5f)
                {
                    pos.x += distFromCam > 0f ? -layer.spriteWidth : layer.spriteWidth;
                    layer.layerTransform.position = pos;
                }
            }
        }
    }
}
