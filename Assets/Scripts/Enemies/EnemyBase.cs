using UnityEngine;

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

    // Components
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected HealthSystem healthSystem;

    // State
    protected Transform playerTransform;
    protected bool facingRight = false; // Enemies start facing left (toward player)
    protected float attackTimer;
    protected bool isStunned;
    protected float stunTimer;
    protected bool isGrounded;

    // Enemy states
    protected enum EnemyState { Patrol, Chase, Attack, Stunned, Dead }
    protected EnemyState currentState = EnemyState.Patrol;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();

        // Apply data from ScriptableObject
        if (enemyData != null)
        {
            if (enemyData.sprite != null)
                spriteRenderer.sprite = enemyData.sprite;
            spriteRenderer.color = enemyData.spriteColor;
        }
    }

    protected virtual void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Initialize health from enemy data
        if (healthSystem != null && enemyData != null)
            healthSystem.Initialize(enemyData.maxHealth);
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        // Handle stun timer
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

        // Update attack cooldown
        attackTimer -= Time.deltaTime;

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // State machine
        float distToPlayer = GetDistanceToPlayer();

        if (distToPlayer <= GetAttackRange())
        {
            currentState = EnemyState.Attack;
        }
        else if (distToPlayer <= GetDetectionRange())
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }

        // Execute current state
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    /// <summary>
    /// Simple patrol behavior - walk back and forth.
    /// </summary>
    protected virtual void Patrol()
    {
        float speed = enemyData != null ? enemyData.patrolSpeed : 1.5f;
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        // Flip at edges (if ground check exists)
        if (groundCheck != null && isGrounded)
        {
            // Check for edge ahead
            Vector2 edgeCheckPos = (Vector2)groundCheck.position + new Vector2(dir * 0.5f, 0f);
            bool groundAhead = Physics2D.OverlapCircle(edgeCheckPos, groundCheckRadius, groundLayer);
            if (!groundAhead)
                FlipEnemy();
        }
    }

    /// <summary>
    /// Chase the player.
    /// </summary>
    protected virtual void Chase()
    {
        if (playerTransform == null) return;

        float speed = enemyData != null ? enemyData.moveSpeed : 3f;
        float dirToPlayer = playerTransform.position.x - transform.position.x;

        // Face the player
        if (dirToPlayer > 0 && !facingRight) FlipEnemy();
        if (dirToPlayer < 0 && facingRight) FlipEnemy();

        // Move toward player
        float moveDir = Mathf.Sign(dirToPlayer);
        rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Attack the player. Override in subclasses for different attack patterns.
    /// </summary>
    protected virtual void Attack()
    {
        // Stop moving when attacking
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Face the player
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

    /// <summary>
    /// Execute the actual attack. Override for custom attack behavior.
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            ShootProjectile();
        }
    }

    /// <summary>
    /// Shoot a projectile toward the player.
    /// </summary>
    protected void ShootProjectile()
    {
        if (firePoint == null || projectilePrefab == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null && playerTransform != null)
        {
            Vector2 dir = (playerTransform.position - firePoint.position).normalized;
            float damage = enemyData != null ? enemyData.damage : 10f;
            projectile.Initialize(dir, damage, false); // false = enemy projectile
        }
    }

    /// <summary>
    /// Stun this enemy for a duration. QUACK!
    /// </summary>
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        currentState = EnemyState.Stunned;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Called when this enemy dies.
    /// </summary>
    public virtual void Die()
    {
        currentState = EnemyState.Dead;

        // Add score
        if (GameManager.Instance != null && enemyData != null)
            GameManager.Instance.AddScore(enemyData.scoreValue);

        // Notify wave manager
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyKilled();

        // Show funny text occasionally
        if (Random.value > 0.7f && UIManager.Instance != null)
        {
            string[] deathQuotes = {
                "DUCK JUSTICE!",
                "ONE LESS OPPRESSOR!",
                "QUACK QUACK!",
                "FREEDOM!",
                "FOR THE FLOCK!"
            };
            UIManager.Instance.ShowTextPopup(
                deathQuotes[Random.Range(0, deathQuotes.Length)],
                transform.position + Vector3.up
            );
        }

        Destroy(gameObject);
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

    protected float GetAttackRange()
    {
        return enemyData != null ? enemyData.attackRange : 5f;
    }

    protected float GetDetectionRange()
    {
        return enemyData != null ? enemyData.detectionRange : 10f;
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetDetectionRange());

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetAttackRange());
    }
}
