using UnityEngine;

/// <summary>
/// Handles shooting (feather/pellet bullets) and grenade throwing for the player.
///
/// Attach this to the Player GameObject.
/// Set up a FirePoint child transform to define the muzzle position.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    // ---- Bullet Settings ----
    [Header("Bullet")]
    public GameObject bulletPrefab;
    [Tooltip("Where bullets spawn (child transform)")]
    public Transform  firePoint;
    [SerializeField] private float fireRate    = 0.15f;   // seconds between shots
    [SerializeField] private float bulletSpeed = 20f;

    // ---- Grenade Settings ----
    [Header("Grenade")]
    public  GameObject grenadePrefab;
    [SerializeField] private float grenadeThrowForce = 10f;
    [SerializeField] private float grenadeCooldown   = 2f;

    // ---- Muzzle Flash (optional) ----
    [Header("FX")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    // ---- State ----
    private float _nextFireTime;
    private float _nextGrenadeTime;
    private SpriteRenderer _sr;

    // ------------------------------------------------
    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    // ---- Public API ----

    /// <summary>Fire a bullet from the muzzle. Respects fire rate.</summary>
    public void Shoot()
    {
        if (Time.time < _nextFireTime) return;
        if (bulletPrefab == null)
        {
            Debug.LogWarning("WeaponSystem: No bulletPrefab assigned!");
            return;
        }

        _nextFireTime = Time.time + fireRate;

        // Determine facing direction from sprite flip
        float dir = (_sr != null && _sr.flipX) ? -1f : 1f;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.SetActive(true); // ensure active even if prefab template was inactive

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = new Vector2(dir * bulletSpeed, 0f);

        // Flip the bullet sprite if firing left
        SpriteRenderer bsr = bullet.GetComponent<SpriteRenderer>();
        if (bsr != null) bsr.flipX = dir < 0f;

        // Muzzle flash
        if (muzzleFlashPrefab != null)
            Destroy(Instantiate(muzzleFlashPrefab, spawnPos, Quaternion.identity), 0.08f);
    }

    /// <summary>Throw an egg grenade with an arc. Respects cooldown.</summary>
    public void ThrowGrenade()
    {
        if (Time.time < _nextGrenadeTime) return;
        if (grenadePrefab == null)
        {
            Debug.LogWarning("WeaponSystem: No grenadePrefab assigned!");
            return;
        }

        _nextGrenadeTime = Time.time + grenadeCooldown;

        float dir = (_sr != null && _sr.flipX) ? -1f : 1f;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up;
        GameObject grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        grenade.SetActive(true); // ensure active even if prefab template was inactive

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Throw forward and slightly upward for an arc
            rb.velocity = new Vector2(dir * grenadeThrowForce, grenadeThrowForce * 0.6f);
            rb.angularVelocity = dir * -360f; // spin for style
        }
    }
}
