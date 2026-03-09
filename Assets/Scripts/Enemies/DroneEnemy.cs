using System.Collections;
using UnityEngine;

/// <summary>
/// Drone enemy — flies freely ignoring terrain, drops explosive bombs on the player.
/// Uses 2D airborne movement, bypasses ground physics.
/// Appears in waves 6+.
/// </summary>
public class DroneEnemy : EnemyBase
{
    [Header("Drone")]
    [SerializeField] private float hoverHeight = 4f;
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float bombDropInterval = 2f;
    [SerializeField] private float bombDropForce = 3f;

    private float bombTimer;

    protected override void Awake()
    {
        base.Awake();

        // Drones float — disable gravity
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    protected override void Start()
    {
        base.Start();
        bombTimer = bombDropInterval;
    }

    protected override void Patrol()
    {
        // Hover at target height and drift slowly
        float speed = enemyData != null ? enemyData.patrolSpeed : 1.5f;
        float dir = facingRight ? 1f : -1f;

        float targetY = (groundCheck != null ? groundCheck.position.y : transform.position.y) + hoverHeight;
        float yDiff = targetY - transform.position.y;

        rb.linearVelocity = new Vector2(dir * speed, yDiff * 3f);

        // Flip on screen edges (simple timer-based)
        if (Random.value < 0.005f) FlipEnemy();
    }

    protected override void Chase()
    {
        if (playerTransform == null) return;

        float dir = playerTransform.position.x - transform.position.x;
        if (dir > 0 && !facingRight) FlipEnemy();
        if (dir < 0 && facingRight) FlipEnemy();

        float targetX = playerTransform.position.x + Mathf.Sign(dir) * -2f;
        float targetY = playerTransform.position.y + hoverHeight;

        Vector2 target = new Vector2(targetX, targetY);
        Vector2 current = rb.position;
        Vector2 moveDir = (target - current).normalized;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, moveDir * flySpeed, Time.deltaTime * 4f);
    }

    protected override void Attack()
    {
        if (playerTransform == null) return;

        // Hover above the player
        float targetX = playerTransform.position.x;
        float targetY = playerTransform.position.y + hoverHeight;
        Vector2 target = new Vector2(targetX, targetY);
        Vector2 moveDir = (target - rb.position).normalized;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, moveDir * flySpeed * 0.5f, Time.deltaTime * 3f);

        bombTimer -= Time.deltaTime;
        if (bombTimer <= 0f)
        {
            PerformAttack();
            bombTimer = bombDropInterval;
        }
    }

    protected override void PerformAttack()
    {
        DropBomb();

        if (Random.value > 0.7f && UIManager.Instance != null)
        {
            string[] lines = { "DEPLOYING PAYLOAD!", "BOMBS AWAY!", "TARGET ACQUIRED!", "INITIATING AIR STRIKE!" };
            UIManager.Instance.ShowTextPopup(
                lines[Random.Range(0, lines.Length)],
                transform.position + Vector3.up * 1.5f
            );
        }
    }

    private void DropBomb()
    {
        GameObject prefab = bombPrefab;
        if (prefab == null) prefab = FindEggGrenadePrefabFallback();
        if (prefab == null) return;

        GameObject bomb = Instantiate(prefab, transform.position, Quaternion.identity);
        Rigidbody2D bombRb = bomb.GetComponent<Rigidbody2D>();
        if (bombRb != null)
        {
            bombRb.gravityScale = 1f;
            Vector2 dropVel = new Vector2(Random.Range(-1f, 1f), -bombDropForce);
            bombRb.linearVelocity = dropVel;
        }

        EggGrenade grenade = bomb.GetComponent<EggGrenade>();
        grenade?.SetFriendly(false);

        AudioManager.PlaySFX("bomb_drop");
    }

    private GameObject FindEggGrenadePrefabFallback()
    {
        // Last resort: find any active grenade in the scene to clone its prefab reference
        EggGrenade existing = FindFirstObjectByType<EggGrenade>();
        return existing != null ? existing.gameObject : null;
    }

}
