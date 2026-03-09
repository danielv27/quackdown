using UnityEngine;

/// <summary>
/// Destructible prop behavior for crates, barrels, etc.
/// Barrels chain-explode when near other explosions.
/// </summary>
public class DestructibleProp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private int scoreValue = 25;
    [SerializeField] private bool isExplosive = false; // Barrels chain-explode

    [Header("Destruction")]
    [SerializeField] private int debrisCount = 5;
    [SerializeField] private float debrisForce = 350f;
    [SerializeField] private Color debrisColor = new Color(0.6f, 0.4f, 0.2f);

    [Header("Explosive Chain")]
    [SerializeField] private float chainExplosionRadius = 3f;
    [SerializeField] private float chainExplosionDamage = 40f;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        int layer = LayerMask.NameToLayer("Destructible");
        if (layer >= 0) gameObject.layer = layer;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        // Flash white on damage
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Invoke(nameof(ResetColor), 0.08f);
        }

        if (currentHealth <= 0f)
            DestroyProp();
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void DestroyProp()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        for (int i = 0; i < debrisCount; i++)
            SpawnDebris();

        if (isExplosive)
            ChainExplosion();
        else
            CameraFollow.ShakeCamera(0.15f);

        AudioManager.PlaySFX(isExplosive ? "explosion" : "crate_break");
        Destroy(gameObject);
    }

    private void SpawnDebris()
    {
        var debris = new GameObject("PropDebris");
        debris.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.3f;

        var sr = debris.AddComponent<SpriteRenderer>();
        sr.color = debrisColor;
        sr.sortingOrder = 3;

        float scale = Random.Range(0.1f, 0.3f);
        debris.transform.localScale = new Vector3(scale, scale, 1f);

        var debrisRb = debris.AddComponent<Rigidbody2D>();
        debrisRb.gravityScale = 2f;
        Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized * debrisForce;
        debrisRb.AddForce(force);
        debrisRb.AddTorque(Random.Range(-12f, 12f));

        Destroy(debris, 2.5f);
    }

    private void ChainExplosion()
    {
        CameraFollow.ShakeCamera(0.3f);
        JuiceManager.Instance?.KillStop();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chainExplosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                float dmg = chainExplosionDamage * (1f - dist / chainExplosionRadius);
                health.TakeDamage(dmg);
            }

            // Chain to other explosives nearby
            DestructibleProp otherProp = hit.GetComponent<DestructibleProp>();
            if (otherProp != null && otherProp != this && otherProp.isExplosive)
                otherProp.TakeDamage(chainExplosionDamage);

            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                hitRb.AddForce(dir * 400f);
            }
        }

        // Explosion visual
        var effect = new GameObject("ChainExplosion");
        effect.transform.position = transform.position;
        var effectSr = effect.AddComponent<SpriteRenderer>();
        effectSr.color = new Color(1f, 0.5f, 0.1f);
        var fx = effect.AddComponent<ExplosionEffect>();
        fx.Initialize(chainExplosionRadius * 0.7f, new Color(1f, 0.5f, 0.1f));

        UIManager.Instance?.ShowTextPopup("CHAIN REACTION!", transform.position + Vector3.up);
    }
}
