using System.Collections;
using UnityEngine;

/// <summary>
/// Central game-feel system. Manages hit stop, slow-motion, and additive screen shake.
/// Call JuiceManager.GetOrCreate() from GameManager.Start() to ensure it exists.
/// </summary>
public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance { get; private set; }

    [Header("Hit Stop")]
    [SerializeField] private float lightHitStopSeconds = 0.025f;
    [SerializeField] private float heavyHitStopSeconds = 0.065f;

    [Header("Slow Motion")]
    [SerializeField] private float slowMoTimeScale = 0.2f;
    [SerializeField] private float slowMoDuration = 0.35f;

    private Coroutine activeHitStop;
    private Coroutine activeSlowMo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Ensures an instance exists, creating one if needed.</summary>
    public static JuiceManager GetOrCreate()
    {
        if (Instance == null)
        {
            var go = new GameObject("[JuiceManager]");
            Instance = go.AddComponent<JuiceManager>();
        }
        return Instance;
    }

    /// <summary>Brief freeze on projectile hit.</summary>
    public void HitStop()
    {
        if (activeHitStop != null) StopCoroutine(activeHitStop);
        activeHitStop = StartCoroutine(DoHitStop(lightHitStopSeconds));
    }

    /// <summary>Longer freeze on enemy kill or explosion.</summary>
    public void KillStop()
    {
        if (activeHitStop != null) StopCoroutine(activeHitStop);
        activeHitStop = StartCoroutine(DoHitStop(heavyHitStopSeconds));
    }

    /// <summary>Brief slow-motion effect for multi-kills / grenade explosions.</summary>
    public void TriggerSlowMo()
    {
        if (activeSlowMo != null) StopCoroutine(activeSlowMo);
        activeSlowMo = StartCoroutine(DoSlowMo());
    }

    private IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        activeHitStop = null;
    }

    private IEnumerator DoSlowMo()
    {
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = 0.02f * slowMoTimeScale;
        yield return new WaitForSecondsRealtime(slowMoDuration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        activeSlowMo = null;
    }

    private void OnDestroy()
    {
        // Restore time scale if this object is destroyed (e.g., scene reload)
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
