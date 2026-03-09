using UnityEngine;

/// <summary>
/// Police Officer enemy – wave 1-2.
/// Slow and weak. Carries a pistol.
/// Uses default EnemyBase patrol/chase/shoot behaviour.
/// </summary>
public class PoliceEnemy : EnemyBase
{
    [Header("Police Specific")]
    // policeStats is kept for optional Inspector assignment as an alias
    // (the base class 'stats' field is what gets used)

    // ---- Funny police quotes on detection ----
    private static readonly string[] s_DetectionLines =
    {
        "FREEZE, DUCK!",
        "Put your wings where I can see 'em!",
        "This is highly irregular…",
    };

    private bool _alerted;

    // ------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // Override colour to a blueish tint for police
        if (Sr != null)
            Sr.color = new Color(0.4f, 0.6f, 1f);
    }

    protected override void PerformAttack()
    {
        // Shout the first time
        if (!_alerted)
        {
            _alerted = true;
            string line = s_DetectionLines[Random.Range(0, s_DetectionLines.Length)];
            Debug.Log($"[Police] \"{line}\"");
        }

        // Standard shoot
        ShootAtPlayer();
    }

    protected override void OnEnemyDeath()
    {
        Debug.Log("[Police] \"I should have stayed in the donut shop…\"");
    }
}
