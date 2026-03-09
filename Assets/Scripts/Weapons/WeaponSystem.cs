using UnityEngine;

/// <summary>
/// Handles the player's weapon systems: shooting feathers and throwing egg grenades.
/// Attach to the Player GameObject alongside PlayerController.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 25f;

    [Header("Egg Grenade")]
    [SerializeField] private GameObject eggGrenadePrefab;
    [SerializeField] private float grenadeThrowForce = 10f;
    [SerializeField] private float grenadeCooldown = 2f;

    private float fireTimer;
    private float grenadeTimer;

    private void Update()
    {
        // Decrement cooldown timers
        fireTimer -= Time.deltaTime;
        grenadeTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Shoots a feather projectile in the given direction.
    /// </summary>
    public void Shoot(Vector2 direction)
    {
        if (fireTimer > 0f) return;
        if (projectilePrefab == null || firePoint == null) return;

        fireTimer = fireRate;

        // Create projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(direction, projectileDamage, true); // true = player projectile
        }

        Debug.Log("PEW PEW! *feather shot*");
    }

    /// <summary>
    /// Throws an egg grenade in the given direction.
    /// THE EGGS OF REVOLUTION!
    /// </summary>
    public void ThrowGrenade(Vector2 direction)
    {
        if (grenadeTimer > 0f) return;
        if (eggGrenadePrefab == null || firePoint == null) return;

        grenadeTimer = grenadeCooldown;

        // Create egg grenade
        GameObject egg = Instantiate(eggGrenadePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D eggRb = egg.GetComponent<Rigidbody2D>();

        if (eggRb != null)
        {
            // Add arc to the throw
            Vector2 throwDir = direction;
            throwDir.y += 0.3f;
            throwDir.Normalize();
            eggRb.AddForce(throwDir * grenadeThrowForce, ForceMode2D.Impulse);
            eggRb.AddTorque(5f); // Spin the egg!
        }

        EggGrenade grenade = egg.GetComponent<EggGrenade>();
        if (grenade != null)
        {
            grenade.SetFriendly(true); // Player's grenade
        }

        Debug.Log("EGG GRENADE AWAY!");
    }
}
