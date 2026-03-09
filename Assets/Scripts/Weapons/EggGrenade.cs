using System.Collections;
using UnityEngine;

/// <summary>
/// Egg grenade thrown by the player.
/// Rolls/bounces, then explodes after a fuse delay, dealing area damage.
///
/// Requires:
///   - Rigidbody2D
///   - CircleCollider2D (set as trigger for the explosion, non-trigger for physics)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EggGrenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private float fuseTime        = 2.5f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float damage          = 60f;

    [Header("FX")]
    [SerializeField] private GameObject explosionFXPrefab;

    [Header("Bounce")]
    [SerializeField] private PhysicsMaterial2D bounceMaterial;

    // ---- State ----
    private bool _exploded;

    // ------------------------------------------------
    void Start()
    {
        // Assign bouncy physics material at runtime
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2f;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (bounceMaterial != null)
            col.sharedMaterial = bounceMaterial;

        // Tag so bullets don't prematurely destroy it
        gameObject.tag = "Grenade";

        StartCoroutine(FuseRoutine());
    }

    // ---- Fuse ----
    private IEnumerator FuseRoutine()
    {
        // Blink faster as fuse runs out
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < fuseTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fuseTime;
            // Blink interval shrinks as t → 1
            float blinkInterval = Mathf.Lerp(0.4f, 0.05f, t);
            if (sr != null)
                sr.color = (Mathf.Sin(elapsed / blinkInterval * Mathf.PI) > 0f)
                    ? Color.white : Color.red;
            yield return null;
        }

        Explode();
    }

    // ---- Explosion ----
    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        // Spawn VFX
        if (explosionFXPrefab != null)
            Instantiate(explosionFXPrefab, transform.position, Quaternion.identity);

        // Apply area damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            // Damage enemies
            HealthSystem hs = hit.GetComponent<HealthSystem>();
            if (hs != null && !hit.CompareTag("Player"))
                hs.TakeDamage(damage);

            // Damage destructibles
            DestructibleObject dest = hit.GetComponent<DestructibleObject>();
            if (dest != null)
                dest.TakeDamage(damage);
        }

        // Screen-shake (simple camera impulse)
        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
            cam.StartCoroutine(ScreenShake(Camera.main.transform, 0.3f, 0.4f));

        Destroy(gameObject);
    }

    // ---- Screen Shake ----
    private IEnumerator ScreenShake(Transform camTransform, float duration, float magnitude)
    {
        Vector3 originPos = camTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            camTransform.localPosition = originPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        camTransform.localPosition = originPos;
    }

    // ---- Debug Gizmo ----
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
