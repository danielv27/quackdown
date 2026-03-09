using System.Collections;
using UnityEngine;

/// <summary>
/// Sniper enemy — stays at maximum range, aims slowly, fires a single high-damage shot.
/// A laser sight telegraphs the shot before it fires.
/// Appears in waves 5+.
/// </summary>
public class SniperEnemy : EnemyBase
{
    [Header("Sniper")]
    [SerializeField] private float aimDuration = 1.5f;
    [SerializeField] private float sniperDamageMultiplier = 3f;
    [SerializeField] private Color laserColor = new Color(1f, 0.1f, 0.1f, 0.8f);

    private LineRenderer laserSight;
    private bool isAiming;

    protected override void Awake()
    {
        base.Awake();
        SetupLaserSight();
    }

    private void SetupLaserSight()
    {
        laserSight = gameObject.AddComponent<LineRenderer>();
        laserSight.startWidth = 0.04f;
        laserSight.endWidth = 0.04f;
        laserSight.material = new Material(Shader.Find("Sprites/Default"));
        laserSight.startColor = laserColor;
        laserSight.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0f);
        laserSight.positionCount = 2;
        laserSight.enabled = false;
        laserSight.sortingOrder = 5;
    }

    protected override void Attack()
    {
        // Sniper holds position completely
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (playerTransform != null)
        {
            float dir = playerTransform.position.x - transform.position.x;
            if (dir > 0 && !facingRight) FlipEnemy();
            if (dir < 0 && facingRight) FlipEnemy();
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            float cooldown = enemyData != null ? enemyData.attackCooldown : 2f;
            attackTimer = cooldown;
        }
    }

    protected override void PerformAttack()
    {
        if (!isAiming)
            StartCoroutine(AimAndFire());
    }

    private IEnumerator AimAndFire()
    {
        isAiming = true;
        laserSight.enabled = true;

        float elapsed = 0f;
        while (elapsed < aimDuration)
        {
            if (currentState == EnemyState.Dead) { laserSight.enabled = false; yield break; }
            if (playerTransform != null && firePoint != null)
            {
                Vector3 dir = (playerTransform.position - firePoint.position).normalized;
                laserSight.SetPosition(0, firePoint.position);
                laserSight.SetPosition(1, firePoint.position + dir * 20f);

                // Pulse intensity as we near the shot
                float pulse = Mathf.PingPong(elapsed * 6f, 1f);
                Color c = laserColor;
                c.a = 0.4f + pulse * 0.6f;
                laserSight.startColor = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        laserSight.enabled = false;

        // Fire the high-damage shot
        if (currentState != EnemyState.Dead && projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null && playerTransform != null)
            {
                Vector2 dir = (playerTransform.position - firePoint.position).normalized;
                float damage = (enemyData != null ? enemyData.damage : 10f) * sniperDamageMultiplier;
                projectile.Initialize(dir, damage, false);
            }
        }

        AudioManager.PlaySFX("sniper_fire");
        CameraFollow.ShakeCamera(0.12f);
        isAiming = false;
    }
}
