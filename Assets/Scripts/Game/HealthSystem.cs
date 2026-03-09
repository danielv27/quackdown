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
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float flashTimer;
    private bool isDead;

    /// <summary>
    /// Initialize with a specific max health value (used by EnemyBase from EnemyData).
    /// </summary>
    public void Initialize(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        // If health wasn't initialized externally, use the inspector value
        if (currentHealth <= 0f)
            currentHealth = maxHealth;
    }

    private void Update()
    {
        // Handle damage flash
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Deal damage to this entity.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        // Fire health changed event
        onHealthChanged?.Invoke(currentHealth / maxHealth);

        // Visual flash
        if (flashOnDamage && spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            flashTimer = flashDuration;
        }

        // Check for death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal this entity.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    /// <summary>
    /// Handle death.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        onDeath?.Invoke();

        // Check if this is an enemy and call its Die method
        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.Die();
            return;
        }

        // Check if this is the player
        if (CompareTag("Player"))
        {
            Debug.Log("THE DUCK HAS FALLEN! But the revolution lives on...");
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    /// <summary>
    /// Get current health as a percentage (0-1).
    /// </summary>
    public float GetHealthPercent()
    {
        return maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }

    /// <summary>
    /// Get current health value.
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Check if this entity is dead.
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}
