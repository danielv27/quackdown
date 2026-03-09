using System.Collections;
using UnityEngine;

/// <summary>
/// SWAT enemy – wave 3-4.
/// Higher health, shoots in short bursts, occasionally throws a flashbang.
/// </summary>
public class SwatEnemy : EnemyBase
{
    [Header("SWAT Specific")]
    [SerializeField] private int   burstCount    = 3;
    [SerializeField] private float burstInterval = 0.12f;

    [Header("Flashbang")]
    [SerializeField] private GameObject flashbangPrefab;
    [SerializeField] private float      flashbangChance = 0.25f; // 25% per attack

    // ---- SWAT colour ----
    private static readonly Color s_SwatColor = new Color(0.3f, 0.3f, 0.3f);

    private static readonly string[] s_AlertLines =
    {
        "SWAT IS HERE, BIRD!",
        "Target acquired. Feathers will fly.",
        "We trained for this. Not this, but something like this.",
    };

    private bool _alerted;

    // ------------------------------------------------
    protected override void Start()
    {
        base.Start();
        if (Sr != null) Sr.color = s_SwatColor;
    }

    protected override void PerformAttack()
    {
        if (!_alerted)
        {
            _alerted = true;
            string line = s_AlertLines[Random.Range(0, s_AlertLines.Length)];
            Debug.Log($"[SWAT] \"{line}\"");
        }

        // Maybe throw flashbang instead of shooting
        if (flashbangPrefab != null && Random.value < flashbangChance)
        {
            ThrowFlashbang();
        }
        else
        {
            StartCoroutine(BurstFire());
        }
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            ShootAtPlayer();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void ThrowFlashbang()
    {
        if (PlayerTransform == null) return;

        Debug.Log("[SWAT] \"FLASHBANG OUT!\"");
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject fb = Instantiate(flashbangPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = fb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = (PlayerTransform.position.x > transform.position.x) ? 1f : -1f;
            rb.velocity = new Vector2(dir * 6f, 5f);
        }
    }

    protected override void OnEnemyDeath()
    {
        Debug.Log("[SWAT] \"This…wasn't covered in training…\"");
    }
}
