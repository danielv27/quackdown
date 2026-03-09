using UnityEngine;

/// <summary>
/// Simple destructible prop (crate, barrel, etc.).
/// When destroyed it spawns an optional particle effect and plays a sound.
/// Tag this object as "Destructible".
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health = 30f;

    [Header("FX")]
    [Tooltip("Optional particle/prefab spawned on destruction")]
    [SerializeField] private GameObject destructionFXPrefab;

    [Header("Score")]
    [SerializeField] private int scoreValue = 10;

    // ---- State ----
    private bool _destroyed;

    // ------------------------------------------------

    /// <summary>Apply damage to this prop.</summary>
    public void TakeDamage(float amount)
    {
        if (_destroyed) return;

        health -= amount;
        if (health <= 0f)
            Explode();
    }

    private void Explode()
    {
        if (_destroyed) return;
        _destroyed = true;

        // Spawn destruction effect
        if (destructionFXPrefab != null)
            Instantiate(destructionFXPrefab, transform.position, Quaternion.identity);

        // Award score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Destroy self
        Destroy(gameObject);
    }

    // Allow bullets/grenades to hit via trigger as well
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet") || other.CompareTag("Grenade"))
            TakeDamage(25f);
    }
}
