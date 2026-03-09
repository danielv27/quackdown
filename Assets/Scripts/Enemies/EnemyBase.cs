using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base class for all enemy types.
/// Provides patrol → chase → attack AI and hooks for subclass customization.
///
/// Required on the same GameObject:
///   - Rigidbody2D
///   - HealthSystem
///   - SpriteRenderer
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public abstract class EnemyBase : MonoBehaviour
{
    // ---- Stats (assigned by subclass or Inspector) ----
    [Header("Enemy Stats")]
    public EnemyStats stats;

    // ---- Bullet ----
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform  firePoint;

    // ---- Cached Components ----
    protected Rigidbody2D    Rb     { get; private set; }
    protected HealthSystem   Health { get; private set; }
    protected SpriteRenderer Sr     { get; private set; }

    // ---- State Machine ----
    protected enum AIState { Patrol, Chase, Attack, Stunned, Dead }
    protected AIState CurrentAIState = AIState.Patrol;

    // ---- Player Reference ----
    protected Transform PlayerTransform { get; private set; }

    // ---- Patrol ----
    private Vector2 _patrolOrigin;
    private  int    _patrolDir = 1;

    // ---- Timing ----
    private float _attackTimer;
    private float _stunTimer;

    // ------------------------------------------------
    protected virtual void Awake()
    {
        Rb     = GetComponent<Rigidbody2D>();
        Health = GetComponent<HealthSystem>();
        Sr     = GetComponent<SpriteRenderer>();

        Rb.freezeRotation = true;

        // Hook health events
        Health.onDeath.AddListener(OnDeath);
    }

    protected virtual void Start()
    {
        _patrolOrigin = transform.position;

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            PlayerTransform = player.transform;
    }

    protected virtual void Update()
    {
        if (Health.IsDead) return;

        // Count down stun
        if (_stunTimer > 0f)
        {
            _stunTimer -= Time.deltaTime;
            CurrentAIState = AIState.Stunned;
            Rb.velocity    = Vector2.zero;
            return;
        }

        UpdateAI();
        _attackTimer -= Time.deltaTime;
    }

    // ---- AI ----
    private void UpdateAI()
    {
        if (PlayerTransform == null) { Patrol(); return; }

        float dist = Vector2.Distance(transform.position, PlayerTransform.position);

        if (dist <= stats.attackRange)
        {
            CurrentAIState = AIState.Attack;
            Rb.velocity    = Vector2.zero;
            if (_attackTimer <= 0f)
            {
                _attackTimer = stats.attackCooldown;
                PerformAttack();
            }
        }
        else if (dist <= stats.detectionRange)
        {
            CurrentAIState = AIState.Chase;
            ChasePlayer();
        }
        else
        {
            CurrentAIState = AIState.Patrol;
            Patrol();
        }

        // Flip sprite to face movement direction
        if (Rb.velocity.x != 0f)
            Sr.flipX = Rb.velocity.x < 0f;
    }

    // ---- Patrol ----
    private void Patrol()
    {
        float edgeDist = Mathf.Abs(transform.position.x - _patrolOrigin.x);
        if (edgeDist >= stats.patrolRange)
            _patrolDir *= -1;

        Rb.velocity = new Vector2(_patrolDir * stats.moveSpeed * 0.5f, Rb.velocity.y);
    }

    // ---- Chase ----
    private void ChasePlayer()
    {
        float dir = (PlayerTransform.position.x > transform.position.x) ? 1f : -1f;
        Rb.velocity = new Vector2(dir * stats.moveSpeed, Rb.velocity.y);
    }

    // ---- Attack (override in subclasses for custom behavior) ----
    protected virtual void PerformAttack()
    {
        ShootAtPlayer();
    }

    /// <summary>Utility: shoot a bullet toward the player.</summary>
    protected void ShootAtPlayer()
    {
        if (bulletPrefab == null || PlayerTransform == null) return;
        if (stats == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
        bullet.SetActive(true); // ensure active even if prefab template was inactive

        // Aim at player
        Vector2 direction = ((Vector2)PlayerTransform.position - (Vector2)origin).normalized;
        Rigidbody2D brb = bullet.GetComponent<Rigidbody2D>();
        if (brb != null)
            brb.velocity = direction * stats.bulletSpeed;

        // Mark bullet as enemy bullet
        bullet.tag = "Bullet";

        // Set enemy bullet flag on Bullet component
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
            b.isPlayerBullet = false;
    }

    // ---- Stun ----
    public void Stun(float duration)
    {
        _stunTimer = duration;
        StartCoroutine(StunFlash(duration));
    }

    private IEnumerator StunFlash(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (Sr != null)
                Sr.color = (Mathf.FloorToInt(elapsed * 10f) % 2 == 0) ? Color.cyan : Color.white;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (Sr != null)
            Sr.color = Color.white;
    }

    // ---- Death ----
    private void OnDeath()
    {
        CurrentAIState = AIState.Dead;

        // Award score
        if (stats != null && GameManager.Instance != null)
            GameManager.Instance.AddScore(stats.scoreValue);

        // Show funny death quote
        if (stats != null && stats.deathQuotes.Length > 0)
        {
            string quote = stats.deathQuotes[Random.Range(0, stats.deathQuotes.Length)];
            Debug.Log($"[{stats.enemyName}] dying: \"{quote}\"");
        }

        OnEnemyDeath();
    }

    /// <summary>Override in subclasses for custom death behaviour.</summary>
    protected virtual void OnEnemyDeath() { }

    // ---- Properties ----
    public EnemyStats Stats => stats;
}
