using UnityEngine;

/// <summary>
/// Simple camera follow script that smoothly tracks the player.
/// Attach to the Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Boundaries")]
    [SerializeField] private bool useBounds;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 20f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDecay = 5f;

    private float shakeMagnitude;
    private Vector3 shakeOffset;

    private void Start()
    {
        // Auto-find player if target not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Set initial position
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player again (might have been spawned)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            return;
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Clamp to boundaries
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, desiredPosition, smoothSpeed * Time.deltaTime
        );

        // Apply screen shake
        if (shakeMagnitude > 0.01f)
        {
            shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0f, shakeDecay * Time.deltaTime);
        }
        else
        {
            shakeOffset = Vector3.zero;
            shakeMagnitude = 0f;
        }

        transform.position = smoothedPosition + (Vector3)shakeOffset;
    }

    /// <summary>
    /// Trigger a screen shake effect (for explosions, etc.)
    /// </summary>
    public void Shake(float magnitude)
    {
        shakeMagnitude = magnitude;
    }

    /// <summary>
    /// Static helper to shake the camera from anywhere.
    /// </summary>
    public static void ShakeCamera(float magnitude)
    {
        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
            cam.Shake(magnitude);
    }
}
