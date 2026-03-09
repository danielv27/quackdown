using UnityEngine;

/// <summary>
/// Projectile behavior for feather shots and enemy bullets.
/// Moves in a direction and deals damage on collision.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    private float damage;
    private bool isFriendly; // true = player projectile, false = enemy projectile
    private Vector2 direction;
    private bool initialized;

    /// <summary>
    /// Initialize the projectile with direction, damage, and team.
    /// </summary>
    public void Initialize(Vector2 dir, float dmg, bool friendly)
    {
        direction = dir.normalized;
        damage = dmg;
        isFriendly = friendly;
        initialized = true;

        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Set layer to Projectile
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // Self-destruct after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized) return;

        // Move the projectile
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit things on the same team
        if (isFriendly && other.CompareTag("Player")) return;
        if (!isFriendly && other.CompareTag("Enemy")) return;

        // Don't hit other projectiles
        if (other.GetComponent<Projectile>() != null) return;

        // Try to deal damage
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Hit destructible props
        DestructibleProp prop = other.GetComponent<DestructibleProp>();
        if (prop != null)
        {
            prop.TakeDamage(damage);
        }

        // Destroy projectile on hit
        Destroy(gameObject);
    }
}
