using UnityEngine;

/// <summary>
/// Destructible prop behavior for crates, barrels, etc.
/// Takes damage and breaks apart when destroyed.
/// </summary>
public class DestructibleProp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private int scoreValue = 25;

    [Header("Destruction")]
    [SerializeField] private int debrisCount = 4;
    [SerializeField] private float debrisForce = 300f;
    [SerializeField] private Color debrisColor = new Color(0.6f, 0.4f, 0.2f); // Wood color

    private float currentHealth;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        int layer = LayerMask.NameToLayer("Destructible");
        if (layer >= 0) gameObject.layer = layer;
        else Debug.LogWarning("Layer 'Destructible' not found. Run DuckRevolution > Setup Game.");
    }

    /// <summary>
    /// Deal damage to this prop.
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Flash on damage
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (currentHealth <= 0f)
        {
            Destroy();
        }
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// Destroy this prop with debris effect.
    /// </summary>
    private void Destroy()
    {
        // Add score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Spawn debris particles
        for (int i = 0; i < debrisCount; i++)
        {
            CreateDebris();
        }

        // Screen shake
        CameraFollow.ShakeCamera(0.15f);

        Destroy(gameObject);
    }

    /// <summary>
    /// Create a simple debris piece.
    /// </summary>
    private void CreateDebris()
    {
        GameObject debris = new GameObject("Debris");
        debris.transform.position = transform.position;

        SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
        sr.color = debrisColor;
        sr.sortingLayerName = "Props";

        // Small random scale
        float scale = Random.Range(0.1f, 0.3f);
        debris.transform.localScale = new Vector3(scale, scale, 1f);

        // Add physics
        Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2f;

        // Random explosion force
        Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized;
        rb.AddForce(force * debrisForce);
        rb.AddTorque(Random.Range(-10f, 10f));

        // Self-destruct debris after 2 seconds
        Destroy(debris, 2f);
    }
}
