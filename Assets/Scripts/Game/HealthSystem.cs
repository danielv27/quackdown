using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shared health/damage system used by the player, enemies, and destructible props.
/// Attach to any GameObject that can take damage.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged; // Passes current health percentage

    [Header("Visual Feedback")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private Color damageFlashColor = Color.white; // White reads universally as "hit"
    [SerializeField] private float flashDuration = 0.08f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashTimer;
    private bool isDead;
    private float shieldAmount;

    /// <summary>Initialize with a specific max health value (used by EnemyBase from EnemyData).</summary>
    public void Initialize(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        isDead = false;
        shieldAmount = 0f;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        if (currentHealth <= 0f)
            currentHealth = maxHealth;
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    /// <summary>Deal damage to this entity.</summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Shield absorbs damage first
        if (shieldAmount > 0f)
        {
            float absorbed = Mathf.Min(shieldAmount, damage);
            shieldAmount -= absorbed;
            damage -= absorbed;
            if (damage <= 0f)
            {
                FlashDamage();
                // Shield-absorbed hit: small blue sparks
                ParticleManager.SpawnHitSpark(transform.position, Vector2.up, new Color(0.4f, 0.7f, 1f));
                return;
            }
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        onHealthChanged?.Invoke(currentHealth / maxHealth);

        FlashDamage();

        // Hit particles
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color hitColor = sr != null ? sr.color : Color.yellow;
        ParticleManager.SpawnHitSpark(transform.position, Vector2.up, Color.yellow);

        // Hit stop on damage
        JuiceManager.Instance?.HitStop();
        AudioManager.PlaySFX("hit");

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>Apply knockback force to this entity's Rigidbody2D.</summary>
    public void Knockback(Vector2 direction, float force)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    /// <summary>Apply a damage-absorbing shield that blocks the next N points of damage.</summary>
    public void ApplyShield(float amount)
    {
        shieldAmount += amount;
        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextPopup("SHIELDED!", transform.position + Vector3.up);
    }

    /// <summary>Heal this entity.</summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void FlashDamage()
    {
        if (flashOnDamage && spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            flashTimer = flashDuration;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        onDeath?.Invoke();

        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.Die();
            return;
        }

        if (CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    public float GetHealthPercent() => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public float GetShield() => shieldAmount;
}
