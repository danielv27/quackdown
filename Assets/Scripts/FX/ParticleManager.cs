using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Spawns and pools all particle effects in the game.
/// Hit sparks, feather/blood bursts, dust, muzzle flash, explosion rings.
/// All effects are self-cleaning (auto-destroy on Stop).
/// </summary>
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private ObjectPool<ParticleSystem> hitSparkPool;
    private ObjectPool<ParticleSystem> featherPool;
    private ObjectPool<ParticleSystem> dustPool;
    private ObjectPool<ParticleSystem> shellCasingPool;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        hitSparkPool    = new ObjectPool<ParticleSystem>(CreateHitSpark, OnGetPS, OnReleasePS, OnDestroyPS, true, 10, 30);
        featherPool     = new ObjectPool<ParticleSystem>(CreateFeatherBurst, OnGetPS, OnReleasePS, OnDestroyPS, true, 10, 30);
        dustPool        = new ObjectPool<ParticleSystem>(CreateDust, OnGetPS, OnReleasePS, OnDestroyPS, true, 6, 20);
        shellCasingPool = new ObjectPool<ParticleSystem>(CreateShellCasing, OnGetPS, OnReleasePS, OnDestroyPS, true, 8, 20);
    }

    public static ParticleManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("ParticleManager");
        return go.AddComponent<ParticleManager>();
    }

    // ─── Public Spawn API ────────────────────────────────────────────────

    public static void SpawnHitSpark(Vector3 pos, Vector2 hitDirection, Color color)
    {
        if (Instance == null) return;
        var ps = Instance.hitSparkPool.Get();
        ps.transform.position = pos;
        var main = ps.main;
        main.startColor = color;
        // Orient toward hit direction
        float angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
        ps.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        ps.Play();
        Instance.ReturnAfterPlay(ps, Instance.hitSparkPool);
    }

    public static void SpawnFeatherBurst(Vector3 pos, Color color, int count = 10)
    {
        if (Instance == null) return;
        var ps = Instance.featherPool.Get();
        ps.transform.position = pos;
        var main = ps.main;
        main.startColor = color;
        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, count));
        ps.Play();
        Instance.ReturnAfterPlay(ps, Instance.featherPool);
    }

    public static void SpawnLandingDust(Vector3 pos)
    {
        if (Instance == null) return;
        var ps = Instance.dustPool.Get();
        ps.transform.position = pos;
        ps.Play();
        Instance.ReturnAfterPlay(ps, Instance.dustPool);
    }

    public static void SpawnShellCasing(Vector3 pos, bool facingRight)
    {
        if (Instance == null) return;
        var ps = Instance.shellCasingPool.Get();
        ps.transform.position = pos;
        // Eject shells opposite to facing direction
        var vel = ps.velocityOverLifetime;
        vel.x = new ParticleSystem.MinMaxCurve(facingRight ? -2f : 2f, facingRight ? -5f : 5f);
        ps.Play();
        Instance.ReturnAfterPlay(ps, Instance.shellCasingPool);
    }

    public static void SpawnExplosionRing(Vector3 pos, float radius, Color color)
    {
        if (Instance == null) return;
        // Create a one-shot expanding ring
        var go = new GameObject("ExplosionRingFX");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ConfigureExplosionRing(ps, radius, color);
        ps.Play();
        Destroy(go, 1.5f);
    }

    // ─── Pool Callbacks ───────────────────────────────────────────────────

    private void OnGetPS(ParticleSystem ps) => ps.gameObject.SetActive(true);
    private void OnReleasePS(ParticleSystem ps) { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); ps.gameObject.SetActive(false); }
    private void OnDestroyPS(ParticleSystem ps) { if (ps != null) Destroy(ps.gameObject); }

    private System.Collections.IEnumerator ReturnCoroutine<T>(T obj, ObjectPool<T> pool, float delay) where T : class
    {
        yield return new WaitForSeconds(delay);
        pool.Release(obj);
    }

    private void ReturnAfterPlay(ParticleSystem ps, ObjectPool<ParticleSystem> pool)
    {
        StartCoroutine(ReturnCoroutine(ps, pool, ps.main.duration + ps.main.startLifetime.constantMax));
    }

    // ─── Particle Configuration ───────────────────────────────────────────

    private static ParticleSystem CreateHitSpark()
    {
        var go = new GameObject("HitSpark");
        var ps = go.AddComponent<ParticleSystem>();
        go.SetActive(false);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = Color.yellow;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, 8, 12));
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.05f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 8;

        return ps;
    }

    private static ParticleSystem CreateFeatherBurst()
    {
        var go = new GameObject("FeatherBurst");
        var ps = go.AddComponent<ParticleSystem>();
        go.SetActive(false);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor = new Color(0.9f, 0.9f, 0.6f);
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, 8, 16));
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var rotOverLife = ps.rotationOverLifetime;
        rotOverLife.enabled = true;
        rotOverLife.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 7;

        return ps;
    }

    private static ParticleSystem CreateDust()
    {
        var go = new GameObject("Dust");
        var ps = go.AddComponent<ParticleSystem>();
        go.SetActive(false);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.15f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new Color(0.7f, 0.6f, 0.5f, 0.7f);
        main.gravityModifier = -0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, 4, 7));
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 0.05f, 0f);

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        var sizeCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 0f));
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 6;

        return ps;
    }

    private static ParticleSystem CreateShellCasing()
    {
        var go = new GameObject("ShellCasing");
        var ps = go.AddComponent<ParticleSystem>();
        go.SetActive(false);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
        main.startColor = new Color(1f, 0.85f, 0.2f);
        main.gravityModifier = 3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, 1));
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.1f, 0.05f, 0f);

        var velOverLife = ps.velocityOverLifetime;
        velOverLife.enabled = true;
        velOverLife.space = ParticleSystemSimulationSpace.World;
        velOverLife.y = new ParticleSystem.MinMaxCurve(1f, 3f);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1.5f;
        renderer.sortingOrder = 5;

        return ps;
    }

    private static void ConfigureExplosionRing(ParticleSystem ps, float radius, Color color)
    {
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 2f, radius * 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = color;
        main.gravityModifier = 0.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.ClearBursts(); emission.AddBurst( new ParticleSystem.Burst(0f, 30, 50));
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        shape.radiusThickness = 0f;

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.grey, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLife.color = gradient;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 9;
    }
}
