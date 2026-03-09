using UnityEngine;

/// <summary>
/// A projectile bullet fired by the player or enemies.
/// Moves in a straight line, damages characters it hits, then destroys itself.
/// Uses Rigidbody2D velocity (set externally by WeaponSystem / EnemyBase).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage         = 25f;
    public float lifeTime       = 3f;
    public bool  isPlayerBullet = true;

    [Header("FX")]
    [SerializeField] private GameObject hitFXPrefab;

    // ---- State ----
    private bool _hasHit;

    // ------------------------------------------------
    void Start()
    {
        // Auto-destroy after lifeTime so stray bullets don't pile up
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;

        // Don't collide with bullets or with the firer's own team
        if (other.CompareTag("Bullet") || other.CompareTag("Grenade")) return;
        if (isPlayerBullet  && other.CompareTag("Player")) return;
        if (!isPlayerBullet && other.CompareTag("Enemy"))  return;

        // Damage target
        HealthSystem hs = other.GetComponent<HealthSystem>();
        if (hs != null)
            hs.TakeDamage(damage);

        // Damage destructible props
        DestructibleObject dest = other.GetComponent<DestructibleObject>();
        if (dest != null)
            dest.TakeDamage(damage);

        // Only react to solid things (not trigger zones)
        if (hs != null || dest != null || !other.isTrigger)
        {
            SpawnHitFX();
            _hasHit = true;
            Destroy(gameObject);
        }
    }

    private void SpawnHitFX()
    {
        if (hitFXPrefab != null)
            Destroy(Instantiate(hitFXPrefab, transform.position, Quaternion.identity), 0.5f);
    }
}
