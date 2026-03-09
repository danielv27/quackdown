using System.Collections;
using UnityEngine;

/// <summary>
/// Army Soldier enemy – wave 5+.
/// High health, fires a heavy rifle in longer bursts,
/// and occasionally calls in an "airstrike" (fast downward bullets from above).
/// </summary>
public class ArmyEnemy : EnemyBase
{
    [Header("Army Specific")]
    [SerializeField] private int   burstCount    = 5;
    [SerializeField] private float burstInterval = 0.08f;

    [Header("Airstrike")]
    [SerializeField] private GameObject airstrikeProjectilePrefab;
    [SerializeField] private float      airstrikeChance  = 0.15f;
    [SerializeField] private int        airstrikeColumns = 5;

    // Army green tint
    private static readonly Color s_ArmyColor = new Color(0.2f, 0.5f, 0.2f);

    private static readonly string[] s_AlertLines =
    {
        "PRIVATE JENKINS, ENGAGE THE DUCK!",
        "Authorization to use extreme fowl force: GRANTED.",
        "They brought a WHAT?! A duck?!",
        "This is above my pay grade.",
    };

    private bool _alerted;

    // ------------------------------------------------
    protected override void Start()
    {
        base.Start();
        if (Sr != null) Sr.color = s_ArmyColor;
    }

    protected override void PerformAttack()
    {
        if (!_alerted)
        {
            _alerted = true;
            string line = s_AlertLines[Random.Range(0, s_AlertLines.Length)];
            Debug.Log($"[Army] \"{line}\"");
        }

        if (airstrikeProjectilePrefab != null && Random.value < airstrikeChance)
            StartCoroutine(Airstrike());
        else
            StartCoroutine(BurstFire());
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            ShootAtPlayer();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    /// <summary>
    /// Simulated airstrike: drop projectiles in a pattern above the player.
    /// </summary>
    private IEnumerator Airstrike()
    {
        Debug.Log("[Army] \"AIRSTRIKE INBOUND – GET SOME!\"");
        if (PlayerTransform == null) yield break;

        float spacing = 1.5f;
        float startX  = PlayerTransform.position.x - (airstrikeColumns / 2f) * spacing;

        for (int i = 0; i < airstrikeColumns; i++)
        {
            float x = startX + i * spacing;
            Vector3 spawnPos = new Vector3(x, PlayerTransform.position.y + 10f, 0f);
            GameObject proj = Instantiate(airstrikeProjectilePrefab, spawnPos, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = Vector2.down * 20f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    protected override void OnEnemyDeath()
    {
        Debug.Log("[Army] \"TELL MY MOM I DIED FIGHTING A DUCK!\"");
    }
}
