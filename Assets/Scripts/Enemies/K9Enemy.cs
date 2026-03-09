using UnityEngine;

/// <summary>
/// K9 dog enemy — very fast, melee-only, ignores ranged attacks.
/// Rushes the player and deals contact damage.
/// Appears in waves 4+.
/// </summary>
public class K9Enemy : EnemyBase
{
    [Header("K9")]
    [SerializeField] private float meleeDamage = 12f;
    [SerializeField] private float meleeCooldown = 0.8f;
    [SerializeField] private float lungeForce = 8f;

    private float meleeTimer;
    private bool hasContactedPlayer;

    protected override void Update()
    {
        base.Update();
        meleeTimer -= Time.deltaTime;
    }

    protected override void Chase()
    {
        base.Chase();

        // K9 is significantly faster while chasing
        if (enemyData != null)
            rb.linearVelocity = new Vector2(
                Mathf.Sign(playerTransform != null ? playerTransform.position.x - transform.position.x : 1f) * enemyData.moveSpeed * 1.8f,
                rb.linearVelocity.y
            );
    }

    protected override void Attack()
    {
        // K9 lunges at the player
        if (playerTransform == null) return;

        float dir = playerTransform.position.x - transform.position.x;
        if (dir > 0 && !facingRight) FlipEnemy();
        if (dir < 0 && facingRight) FlipEnemy();

        // Keep moving toward player
        float speed = enemyData != null ? enemyData.moveSpeed * 2f : 6f;
        rb.linearVelocity = new Vector2(Mathf.Sign(dir) * speed, rb.linearVelocity.y);

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = meleeCooldown;
        }
    }

    protected override void PerformAttack()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > 1.5f) return;

        // Deal damage on contact
        HealthSystem playerHealth = playerTransform.GetComponent<HealthSystem>();
        if (playerHealth != null)
            playerHealth.TakeDamage(meleeDamage);

        // Lunge force
        Vector2 lungeDir = (playerTransform.position - transform.position).normalized;
        rb.AddForce(lungeDir * lungeForce, ForceMode2D.Impulse);

        if (Random.value > 0.6f && UIManager.Instance != null)
        {
            string[] barks = { "WOOF!", "ARF ARF!", "BORK!", "GRRRR!" };
            UIManager.Instance.ShowTextPopup(
                barks[Random.Range(0, barks.Length)],
                transform.position + Vector3.up
            );
        }

        AudioManager.PlaySFX("dog_bark");
    }
}
