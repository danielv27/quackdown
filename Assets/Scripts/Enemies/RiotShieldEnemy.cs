using System.Collections;
using UnityEngine;

/// <summary>
/// Riot Shield cop — a tanky frontline enemy that blocks bullets from the front.
/// Must be flanked or hit with grenades/explosives to deal full damage.
/// Appears in waves 4+.
/// </summary>
public class RiotShieldEnemy : EnemyBase
{
    [Header("Riot Shield")]
    [SerializeField] [Range(0f, 1f)] private float frontDamageReduction = 0.85f;
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private bool isCharging;

    /// <summary>
    /// Frontal damage reduction: returns the multiplier to apply to incoming damage.
    /// Called by HealthSystem when damage comes from in front of the enemy.
    /// </summary>
    public float GetFrontalDamageMultiplier(Vector2 damageSourcePosition)
    {
        Vector2 toSource = (damageSourcePosition - (Vector2)transform.position).normalized;
        Vector2 facing = facingRight ? Vector2.right : Vector2.left;
        float dot = Vector2.Dot(facing, toSource);
        // If hit from the front (dot < 0 means source is in front of facing dir)
        if (dot < -0.3f)
            return 1f - frontDamageReduction;
        return 1f;
    }

    protected override void PerformAttack()
    {
        // Riot cop charges forward instead of shooting
        if (!isCharging)
            StartCoroutine(Charge());

        if (Random.value > 0.85f && UIManager.Instance != null)
        {
            string[] lines = { "SHIELDS UP!", "FORM A LINE!", "PUSH THEM BACK!", "HOLD THE LINE!" };
            UIManager.Instance.ShowTextPopup(
                lines[Random.Range(0, lines.Length)],
                transform.position + Vector3.up * 1.5f
            );
        }
    }

    private IEnumerator Charge()
    {
        isCharging = true;
        float elapsed = 0f;
        float chargeDuration = 0.5f;

        CameraFollow.ShakeCamera(0.1f);

        while (elapsed < chargeDuration)
        {
            if (currentState == EnemyState.Dead) yield break;
            float dir = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * chargeSpeed, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
    }
}
