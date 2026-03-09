using UnityEngine;

/// <summary>
/// Egg grenade behavior - bounces around, then explodes dealing area damage.
/// THE EGGS OF REVOLUTION!
/// </summary>
public class EggGrenade : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float explosionForce = 500f;

    [Header("Visual")]
    [SerializeField] private Color explosionColor = new Color(1f, 0.8f, 0f);

    private float timer;
    private bool isFriendly = true; // Player's grenade by default
    private bool hasExploded;

    /// <summary>
    /// Set whether this grenade was thrown by the player or an enemy.
    /// </summary>
    public void SetFriendly(bool friendly)
    {
        isFriendly = friendly;
    }

    private void Start()
    {
        timer = fuseTime;
        gameObject.layer = LayerMask.NameToLayer("Projectile");
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Flash faster as it's about to explode
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float flash = Mathf.PingPong(Time.time * (3f - timer) * 3f, 1f);
            sr.color = Color.Lerp(Color.white, Color.red, flash);
        }

        if (timer <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    /// <summary>
    /// BOOM! Explode and deal damage to everything in radius.
    /// </summary>
    private void Explode()
    {
        hasExploded = true;

        Debug.Log("BOOM! *egg explosion*");

        // Find everything in explosion radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            // Don't damage the thrower's team
            if (isFriendly && hit.CompareTag("Player")) continue;
            if (!isFriendly && hit.CompareTag("Enemy")) continue;

            // Deal damage
            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                // Damage falls off with distance
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                float damageMultiplier = 1f - (dist / explosionRadius);
                health.TakeDamage(explosionDamage * damageMultiplier);
            }

            // Apply explosion force
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 forceDir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(forceDir * explosionForce);
            }

            // Destroy destructible props
            DestructibleProp prop = hit.GetComponent<DestructibleProp>();
            if (prop != null)
            {
                prop.TakeDamage(explosionDamage);
            }
        }

        // Show explosion popup
        if (UIManager.Instance != null)
        {
            string[] explosionTexts = { "BOOM!", "KABOOM!", "EGG-SPLOSION!", "*CRACK*" };
            UIManager.Instance.ShowTextPopup(
                explosionTexts[Random.Range(0, explosionTexts.Length)],
                transform.position + Vector3.up
            );
        }

        // Create simple explosion visual effect
        CreateExplosionEffect();

        Destroy(gameObject);
    }

    /// <summary>
    /// Creates a simple expanding circle as an explosion effect.
    /// </summary>
    private void CreateExplosionEffect()
    {
        // Create a simple explosion circle
        GameObject effect = new GameObject("ExplosionEffect");
        effect.transform.position = transform.position;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.color = explosionColor;
        sr.sortingLayerName = "Projectiles";

        // Use a simple circle - will be replaced with actual sprite
        // For now, scale up and fade out
        ExplosionEffect fx = effect.AddComponent<ExplosionEffect>();
        fx.Initialize(explosionRadius, explosionColor);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
