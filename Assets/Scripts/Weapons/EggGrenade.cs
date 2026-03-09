using UnityEngine;

/// <summary>
/// Egg grenade behavior - bounces around, then explodes dealing area damage.
/// Supports radius multipliers from powerups. Chain-explodes destructible props.
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
    private bool isFriendly = true;
    private bool hasExploded;
    private float radiusMultiplier = 1f;
    private SpriteRenderer sr;

    public void SetFriendly(bool friendly) => isFriendly = friendly;
    public void SetRadiusMultiplier(float mult) => radiusMultiplier = mult;

    private void Start()
    {
        timer = fuseTime;
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Flash faster as it's about to explode
        if (sr != null)
        {
            float flashSpeed = 3f + (1f - Mathf.Clamp01(timer / fuseTime)) * 15f;
            float flash = Mathf.PingPong(Time.time * flashSpeed, 1f);
            sr.color = Color.Lerp(Color.white, Color.red, flash);
        }

        if (timer <= 0f && !hasExploded)
            Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        float radius = explosionRadius * radiusMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            if (isFriendly && hit.CompareTag("Player")) continue;
            if (!isFriendly && hit.CompareTag("Enemy")) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            float falloff = 1f - Mathf.Clamp01(dist / radius);

            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(explosionDamage * falloff);
                // Knockback
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                health.Knockback(dir, explosionForce * 0.01f * falloff);
            }

            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 forceDir = (hit.transform.position - transform.position).normalized;
                hitRb.AddForce(forceDir * explosionForce * falloff);
            }

            DestructibleProp prop = hit.GetComponent<DestructibleProp>();
            prop?.TakeDamage(explosionDamage);
        }

        // Show explosion popup
        if (UIManager.Instance != null)
        {
            string[] texts = { "BOOM!", "KABOOM!", "EGG-SPLOSION!", "*CRACK*", "SCRAMBLED!" };
            UIManager.Instance.ShowTextPopup(texts[Random.Range(0, texts.Length)], transform.position + Vector3.up);
        }

        // Big shake and kill stop
        CameraFollow.ShakeCamera(0.35f * radiusMultiplier);
        JuiceManager.Instance?.KillStop();
        AudioManager.PlaySFX("explosion");

        // Create explosion visual
        CreateExplosionEffect(radius);

        Destroy(gameObject);
    }

    private void CreateExplosionEffect(float radius)
    {
        GameObject effect = new GameObject("ExplosionEffect");
        effect.transform.position = transform.position;

        SpriteRenderer effectSr = effect.AddComponent<SpriteRenderer>();
        effectSr.color = explosionColor;
        effectSr.sortingOrder = 10;

        ExplosionEffect fx = effect.AddComponent<ExplosionEffect>();
        fx.Initialize(radius, explosionColor);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * radiusMultiplier);
    }
}
