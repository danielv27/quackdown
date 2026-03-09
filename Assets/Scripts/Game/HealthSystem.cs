using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable health/damage component.
/// Attach to any character (player or enemy) that can take damage.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public bool  isPlayer  = false;

    // ---- State ----
    public float CurrentHealth { get; private set; }
    public bool  IsDead        { get; private set; }
    public float MaxHealth     => maxHealth;

    // ---- Events (wired in Inspector or via code) ----
    [Header("Events")]
    public UnityEvent<float> onHealthChanged;   // passes current health
    public UnityEvent        onDeath;

    // ------------------------------------------------
    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // ---- Public API ----

    /// <summary>Apply damage to this character.</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        onHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    /// <summary>Restore health (e.g., powerup pickup).</summary>
    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        onHealthChanged?.Invoke(CurrentHealth);
    }

    /// <summary>Instantly kill this character.</summary>
    public void Kill() => Die();

    // ---- Internal ----
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        onDeath?.Invoke();

        // If this is the player, notify GameManager
        if (isPlayer && GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();

        // Slightly dramatic death delay then destroy
        Destroy(gameObject, 0.5f);
    }
}
