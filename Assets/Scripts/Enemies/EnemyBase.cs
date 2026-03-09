using UnityEngine;

public enum WaveModifier { None, DoubleTime, BulletHell, Swarm, Armored, Explosive }

/// <summary>
/// Base class for all enemies. Handles common behavior like health, patrol, chase, and attack.
/// Specific enemy types inherit from this and override behavior as needed.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] protected EnemyData enemyData;

    [Header("Ground Check")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckRadius = 0.2f;
    [SerializeField] protected LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected GameObject projectilePrefab;

    [Header("Drops")]
    [SerializeField] private PowerupData[] possibleDrops;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.15f;
    [SerializeField] private WeaponData weaponDrop;
    [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 0.08f;

    // Components
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected HealthSystem healthSystem;

    // State
    protected Transform playerTransform;
    protected bool facingRight = false;
    protected float attackTimer;
    protected bool isStunned;
    protected float stunTimer;
    protected bool isGrounded;

    // Wave modifier state
    private WaveModifier activeModifier = WaveModifier.None;
    private bool explodesOnDeath;

    // Enemy states
    protected enum EnemyState { Patrol, Chase, Attack, Stunned, Dead }
    protected EnemyState currentState = EnemyState.Patrol;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();

        if (enemyData != null)
        {
            if (enemyData.sprite != null)
                spriteRenderer.sprite = enemyData.sprite;
            spriteRenderer.color = enemyData.spriteColor;
        }
    }

    protected virtual void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (healthSystem != null && enemyData != null)
            healthSystem.Initialize(enemyData.maxHealth);
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
                currentState = EnemyState.Patrol;
            }
            return;
        }

        attackTimer -= Time.deltaTime;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float distToPlayer = GetDistanceToPlayer();

        if (distToPlayer <= GetAttackRange())
            currentState = EnemyState.Attack;
        else if (distToPlayer <= GetDetectionRange())
            currentState = EnemyState.Chase;
        else
            currentState = EnemyState.Patrol;

        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase:  Chase();  break;
            case EnemyState.Attack: Attack(); break;
        }
    }

    protected virtual void Patrol()
    {
        float speed = enemyData != null ? enemyData.patrolSpeed : 1.5f;
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        if (groundCheck != null && isGrounded)
        {
            Vector2 edgeCheckPos = (Vector2)groundCheck.position + new Vector2(dir * 0.5f, 0f);
            bool groundAhead = Physics2D.OverlapCircle(edgeCheckPos, groundCheckRadius, groundLayer);
            if (!groundAhead)
                FlipEnemy();
        }
    }

    protected virtual void Chase()
    {
        if (playerTransform == null) return;

        float speed = enemyData != null ? enemyData.moveSpeed : 3f;
        float dirToPlayer = playerTransform.position.x - transform.position.x;

        if (dirToPlayer > 0 && !facingRight) FlipEnemy();
        if (dirToPlayer < 0 && facingRight) FlipEnemy();

        float moveDir = Mathf.Sign(dirToPlayer);
        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
    }

    protected virtual void Attack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (playerTransform != null)
        {
            float dirToPlayer = playerTransform.position.x - transform.position.x;
            if (dirToPlayer > 0 && !facingRight) FlipEnemy();
            if (dirToPlayer < 0 && facingRight) FlipEnemy();
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = enemyData != null ? enemyData.attackCooldown : 1f;
        }
    }

    protected virtual void PerformAttack()
    {
        if (projectilePrefab != null && firePoint != null)
            ShootProjectile();
    }

    protected void ShootProjectile()
    {
        if (firePoint == null || projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null && playerTransform != null)
        {
            Vector2 dir = (playerTransform.position - firePoint.position).normalized;
            float damage = enemyData != null ? enemyData.damage : 10f;
            projectile.Initialize(dir, damage, false);
        }

        AudioManager.PlaySFX("enemy_shoot");
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        currentState = EnemyState.Stunned;
        rb.linearVelocity = Vector2.zero;

        // Knockback from QUACK
        if (playerTransform != null)
        {
            Vector2 knockDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            rb.AddForce(knockDir * 6f, ForceMode2D.Impulse);
        }
    }

    /// <summary>Apply a wave modifier to this enemy (called by WaveManager on spawn).</summary>
    public void ApplyModifier(WaveModifier modifier)
    {
        activeModifier = modifier;

        switch (modifier)
        {
            case WaveModifier.DoubleTime:
                // Speed applied each frame in patrol/chase
                break;
            case WaveModifier.BulletHell:
                if (enemyData != null)
                    attackTimer = -enemyData.attackCooldown * 0.5f; // Start with shorter cooldown
                break;
            case WaveModifier.Armored:
                if (healthSystem != null && enemyData != null)
                    healthSystem.Initialize(enemyData.maxHealth * 1.75f);
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.6f); // Gray tint
                break;
            case WaveModifier.Swarm:
                // Half health, fast and disposable
                if (healthSystem != null && enemyData != null)
                    healthSystem.Initialize(enemyData.maxHealth * 0.5f);
                break;
            case WaveModifier.Explosive:
                explodesOnDeath = true;
                spriteRenderer.color = new Color(1f, 0.5f, 0.1f); // Orange tint
                break;
        }
    }

    public virtual void Die()
    {
        currentState = EnemyState.Dead;

        // Score — multiplied by combo
        int baseScore = enemyData != null ? enemyData.scoreValue : 100;
        float multiplier = 1f;
        if (ComboSystem.Instance != null)
            multiplier = ComboSystem.Instance.RegisterKill();
        int finalScore = Mathf.RoundToInt(baseScore * multiplier);

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(finalScore);

        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyKilled();

        // Show kill popup (score + occasional funny quote)
        if (UIManager.Instance != null)
        {
            if (multiplier > 1.5f)
                UIManager.Instance.ShowTextPopup($"+{finalScore} x{multiplier:F1}!", transform.position + Vector3.up);
            else if (Random.value > 0.65f)
            {
                string[] quotes = { "DUCK JUSTICE!", "ONE LESS OPPRESSOR!", "QUACK QUACK!", "FREEDOM!", "FOR THE FLOCK!", "REVOLUTION!" };
                UIManager.Instance.ShowTextPopup(quotes[Random.Range(0, quotes.Length)], transform.position + Vector3.up);
            }
        }

        // Kill stop + shake
        JuiceManager.Instance?.KillStop();
        CameraFollow.ShakeCamera(0.18f);
        AudioManager.PlaySFX("enemy_death");

        // Feather burst on death
        Color enemyColor = spriteRenderer != null ? spriteRenderer.color : Color.grey;
        ParticleManager.SpawnFeatherBurst(transform.position, enemyColor, 12);
        ParticleManager.SpawnHitSpark(transform.position, Vector2.up, Color.yellow);

        // Spawn debris on death
        SpawnDeathDebris();

        // Explosive modifier
        if (explodesOnDeath)
            SpawnDeathExplosion();

        // Powerup drop
        TryDropPowerup();

        Destroy(gameObject);
    }

    private void SpawnDeathDebris()
    {
        int count = Random.Range(3, 7);
        Color debrisColor = spriteRenderer != null ? spriteRenderer.color : Color.gray;
        debrisColor = Color.Lerp(debrisColor, Color.gray, 0.4f);

        for (int i = 0; i < count; i++)
        {
            var debris = new GameObject("EnemyDebris");
            debris.transform.position = transform.position;

            var sr = debris.AddComponent<SpriteRenderer>();
            sr.color = debrisColor;
            sr.sortingOrder = 2;

            float scale = Random.Range(0.1f, 0.35f);
            debris.transform.localScale = new Vector3(scale, scale, 1f);

            var debrisRb = debris.AddComponent<Rigidbody2D>();
            debrisRb.gravityScale = 2f;
            Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 2f)).normalized * 280f;
            debrisRb.AddForce(force);
            debrisRb.AddTorque(Random.Range(-15f, 15f));

            int layer = LayerMask.NameToLayer("Projectile");
            if (layer >= 0) debris.layer = layer;

            Destroy(debris, 2.5f);
        }
    }

    private void SpawnDeathExplosion()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            HealthSystem h = hit.GetComponent<HealthSystem>();
            if (h != null && !hit.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                h.TakeDamage(30f * (1f - dist / 2.5f));
            }
        }
        CameraFollow.ShakeCamera(0.25f);
        UIManager.Instance?.ShowTextPopup("BOOM!", transform.position + Vector3.up);
    }

    private void TryDropPowerup()
    {
        // Try weapon drop first
        if (weaponDrop != null && Random.value <= weaponDropChance)
        {
            SpawnWeaponPickup();
            return; // Only one drop per enemy
        }

        if (possibleDrops == null || possibleDrops.Length == 0) return;
        if (Random.value > dropChance) return;

        PowerupData drop = possibleDrops[Random.Range(0, possibleDrops.Length)];
        if (drop == null) return;

        var go = new GameObject("PowerupDrop");
        go.transform.position = transform.position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = drop.glowColor;
        sr.sortingOrder = 5;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        var pickup = go.AddComponent<PowerupPickup>();
        pickup.SetData(drop);
    }

    private void SpawnWeaponPickup()
    {
        var go = new GameObject("WeaponPickup_" + weaponDrop.weaponName);
        go.transform.position = transform.position + Vector3.up * 0.3f;
        go.tag = "Pickup";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = weaponDrop.projectileColor;
        sr.sortingOrder = 5;

        var rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 2f;
        rb2d.AddForce(new Vector2(Random.Range(-2f, 2f), 4f), ForceMode2D.Impulse);

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.isTrigger = true;

        var pickup = go.AddComponent<WeaponPickup>();
        pickup.SetWeaponData(weaponDrop);
    }

    protected void FlipEnemy()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = facingRight;

        if (firePoint != null)
        {
            Vector3 pos = firePoint.localPosition;
            pos.x = -pos.x;
            firePoint.localPosition = pos;
        }
    }

    protected float GetDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    protected float GetAttackRange() => enemyData != null ? enemyData.attackRange : 5f;

    protected float GetDetectionRange()
    {
        float range = enemyData != null ? enemyData.detectionRange : 10f;
        if (activeModifier == WaveModifier.DoubleTime) range *= 1.3f;
        return range;
    }

    protected float GetMoveSpeed()
    {
        float speed = enemyData != null ? enemyData.moveSpeed : 3f;
        if (activeModifier == WaveModifier.DoubleTime) speed *= 1.5f;
        return speed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetDetectionRange());
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetAttackRange());
    }
}
