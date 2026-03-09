using UnityEngine;

/// <summary>
/// Smooth camera follow for a 2D side-scroller.
/// Attach to the Main Camera and assign the player transform.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Tooltip("How quickly the camera catches up (higher = snappier)")]
    [SerializeField] private float smoothSpeed = 5f;

    [Tooltip("Offset from the target (Z should stay negative for 2D)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Bounds (leave 0 to disable)")]
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX =  50f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY =  20f;

    // ---- Cached ----
    private Camera _cam;

    // ------------------------------------------------
    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position
        Vector3 desired = target.position + offset;

        // Half extents of camera viewport in world space
        float halfH = _cam.orthographicSize;
        float halfW = _cam.orthographicSize * _cam.aspect;

        // Clamp within level bounds (guard against level being narrower than camera)
        float clampMinX = minX + halfW;
        float clampMaxX = maxX - halfW;
        float clampMinY = minY + halfH;
        float clampMaxY = maxY - halfH;

        if (clampMinX < clampMaxX)
            desired.x = Mathf.Clamp(desired.x, clampMinX, clampMaxX);
        if (clampMinY < clampMaxY)
            desired.y = Mathf.Clamp(desired.y, clampMinY, clampMaxY);
        desired.z = offset.z; // keep the Z constant

        // Smooth lerp
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }

    /// <summary>Assign the player at runtime (called by PlayerController on Start).</summary>
    public void SetTarget(Transform t) => target = t;
}
