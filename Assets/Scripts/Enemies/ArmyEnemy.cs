using UnityEngine;

/// <summary>
/// Army soldier enemy - the toughest ground unit.
/// High health, high damage, uses rifles and grenades.
/// Appears in waves 5+.
/// </summary>
public class ArmyEnemy : EnemyBase
{
    [Header("Army Specific")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float grenadeChance = 0.3f;
    [SerializeField] private float grenadeThrowForce = 8f;

    protected override void PerformAttack()
    {
        // Army soldiers occasionally throw grenades
        if (Random.value < grenadeChance && grenadePrefab != null)
        {
            ThrowGrenade();
        }
        else
        {
            ShootProjectile();
        }

        // Army voice lines
        if (Random.value > 0.9f && UIManager.Instance != null)
        {
            string[] lines = {
                "THEY BROUGHT A TANK?!",
                "WHAT KIND OF DUCK IS THIS?!",
                "REQUESTING AIR SUPPORT!",
                "THIS IS NOT IN THE MANUAL!",
                "FIRE EVERYTHING!"
            };
            UIManager.Instance.ShowTextPopup(
                lines[Random.Range(0, lines.Length)],
                transform.position + Vector3.up * 1.5f
            );
        }
    }

    /// <summary>
    /// Throw a grenade toward the player's position.
    /// </summary>
    private void ThrowGrenade()
    {
        if (firePoint == null || playerTransform == null) return;

        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

        if (grenadeRb != null)
        {
            Vector2 throwDir = (playerTransform.position - firePoint.position).normalized;
            // Add some arc to the throw
            throwDir.y += 0.5f;
            throwDir.Normalize();
            grenadeRb.AddForce(throwDir * grenadeThrowForce, ForceMode2D.Impulse);
        }

        // Set grenade as enemy grenade
        EggGrenade egg = grenade.GetComponent<EggGrenade>();
        if (egg != null)
        {
            egg.SetFriendly(false);
        }
    }
}
