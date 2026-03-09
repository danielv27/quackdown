using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Projectile behavior for feather shots and enemy bullets.
/// Moves in a direction and deals damage on collision.
/// Supports sprite overrides and piercing for different weapon types.
/// Supports ObjectPool return for zero-alloc spawning.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    [Header("Trail")]
    [SerializeField] private bool addTrail = true;
    [SerializeField] private float trailTime = 0.08f;
    [SerializeField] private Color trailStartColor = new Color(1f, 1f, 0.8f, 0.8f);
    [SerializeField] private Color trailEndColor = new Color(1f, 0.8f, 0f, 0f);

    private float damage;
    private bool isFriendly;
    private Vector2 direction;
    private bool initialized;
    private bool piercing;
    private ObjectPool<GameObject> pool;
    private TrailRenderer existingTrail;

    public void SetPool(ObjectPool<GameObject> objectPool) => pool = objectPool;

    /// <summary>Initialize the projectile with direction, damage, and team.</summary>
    public void Initialize(Vector2 dir, float dmg, bool friendly)
    {
        direction = dir.normalized;
        damage = dmg;
        isFriendly = friendly;
        initialized = true;
        piercing = false;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        int projLayer = LayerMask.NameToLayer("Projectile");
        if (projLayer >= 0) gameObject.layer = projLayer;

        if (addTrail && existingTrail == null)
            existingTrail = SetupTrail();
        else if (existingTrail != null)
            existingTrail.Clear();

        CancelInvoke(nameof(ReturnOrDestroy));
        Invoke(nameof(ReturnOrDestroy), lifetime);
    }

    private void ReturnOrDestroy()
    {
        if (pool != null)
        {
            initialized = false;
            pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private TrailRenderer SetupTrail()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = 0.15f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = trailStartColor;
        trail.endColor = trailEndColor;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return trail;
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void SetPiercing(bool pierce) => piercing = pierce;

    public void SetSprite(Sprite sprite, Color color)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        if (sprite != null) sr.sprite = sprite;
        sr.color = color;
        // Update trail color to match projectile
        trailStartColor = new Color(color.r, color.g, color.b, 0.8f);
        trailEndColor = new Color(color.r, color.g, color.b, 0f);
    }

    private void Update()
    {
        if (!initialized) return;
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (isFriendly && other.CompareTag("Player")) return;
        if (!isFriendly && other.CompareTag("Enemy")) return;
        if (other.GetComponent<Projectile>() != null) return;
        if (other.GetComponent<EggGrenade>() != null) return;

        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage);
            health.Knockback(direction, 3f);
        }

        DestructibleProp prop = other.GetComponent<DestructibleProp>();
        if (prop != null)
            prop.TakeDamage(damage);

        if (!piercing)
            ReturnOrDestroy();
    }
}
