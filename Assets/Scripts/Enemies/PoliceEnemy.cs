using UnityEngine;

/// <summary>
/// Police officer enemy - basic enemy with pistol.
/// Low health, slow movement, short range attacks.
/// Appears in waves 1-2.
/// </summary>
public class PoliceEnemy : EnemyBase
{
    protected override void PerformAttack()
    {
        // Police shoot with a pistol
        ShootProjectile();

        // Occasionally yell funny lines
        if (Random.value > 0.8f && UIManager.Instance != null)
        {
            string[] lines = {
                "STOP RIGHT THERE, DUCK!",
                "YOU HAVE THE RIGHT TO REMAIN... ROASTED!",
                "FREEZE, FOWL!",
                "CALLING FOR BACKUP!"
            };
            UIManager.Instance.ShowTextPopup(
                lines[Random.Range(0, lines.Length)],
                transform.position + Vector3.up * 1.5f
            );
        }
    }
}
