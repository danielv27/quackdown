using UnityEngine;

/// <summary>
/// SWAT enemy - tougher than police, with better weapons and armor.
/// More health, faster, more aggressive.
/// Appears in waves 3-4.
/// </summary>
public class SwatEnemy : EnemyBase
{
    [Header("SWAT Specific")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.15f;

    private int burstsRemaining;
    private float burstTimer;

    protected override void PerformAttack()
    {
        // SWAT fires in bursts
        burstsRemaining = burstCount;
        burstTimer = 0f;
    }

    protected override void Update()
    {
        base.Update();

        // Handle burst fire
        if (burstsRemaining > 0)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                ShootProjectile();
                burstsRemaining--;
                burstTimer = burstInterval;
            }
        }
    }

    protected override void Chase()
    {
        base.Chase();

        // SWAT occasionally yell
        if (Random.value > 0.995f && UIManager.Instance != null)
        {
            string[] lines = {
                "SWAT DEPLOYED!",
                "GO GO GO!",
                "BREACH AND CLEAR!",
                "TANGO SPOTTED - IT'S A DUCK?!"
            };
            UIManager.Instance.ShowTextPopup(
                lines[Random.Range(0, lines.Length)],
                transform.position + Vector3.up * 1.5f
            );
        }
    }
}
