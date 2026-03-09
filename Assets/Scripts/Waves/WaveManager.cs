using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressively harder enemies.
/// Supports wave modifiers (Double Time, Bullet Hell, Swarm, Armored, Explosive).
/// Inserts rest waves every 5 waves for pacing. Includes 4 new enemy types.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Base Enemy Prefabs")]
    [SerializeField] private GameObject policePrefab;
    [SerializeField] private GameObject swatPrefab;
    [SerializeField] private GameObject armyPrefab;

    [Header("New Enemy Prefabs")]
    [SerializeField] private GameObject riotShieldPrefab;
    [SerializeField] private GameObject sniperPrefab;
    [SerializeField] private GameObject k9Prefab;
    [SerializeField] private GameObject dronePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] aerialSpawnPoints;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float spawnInterval = 0.8f;

    [Header("Wave Configuration")]
    [SerializeField] private int baseEnemiesPerWave = 3;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int maxEnemiesPerWave = 20;

    [Header("State (Debug)")]
    [SerializeField] private int currentWave;
    [SerializeField] private int enemiesAlive;
    [SerializeField] private int enemiesToSpawn;
    [SerializeField] private bool isSpawning;

    private float waveTimer;
    private float spawnTimer;
    private bool wavesActive;
    private WaveModifier activeModifier = WaveModifier.None;
    private bool isRestWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartWaves()
    {
        currentWave = 0;
        enemiesAlive = 0;
        wavesActive = true;
        waveTimer = 2f;
    }

    public void StopWaves()
    {
        wavesActive = false;
        isSpawning = false;
    }

    public int GetCurrentWave() => currentWave;

    private void Update()
    {
        if (!wavesActive) return;

        if (!isSpawning && enemiesAlive <= 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
                StartNextWave();
        }

        if (isSpawning && enemiesToSpawn > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                enemiesToSpawn--;
                spawnTimer = spawnInterval;

                if (enemiesToSpawn <= 0)
                    isSpawning = false;
            }
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        isRestWave = currentWave > 1 && currentWave % 5 == 0;

        int totalEnemies;
        if (isRestWave)
        {
            totalEnemies = Mathf.Max(2, baseEnemiesPerWave - 1);
            activeModifier = WaveModifier.None;
        }
        else
        {
            totalEnemies = Mathf.Min(
                baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease,
                maxEnemiesPerWave
            );
            activeModifier = PickModifier(currentWave);
            // Swarm: double enemy count at half health
            if (activeModifier == WaveModifier.Swarm)
                totalEnemies = Mathf.Min(totalEnemies * 2, maxEnemiesPerWave);
        }

        enemiesToSpawn = totalEnemies;
        isSpawning = true;
        spawnTimer = 0f;

        UIManager.Instance?.ShowWaveAnnouncement(BuildAnnouncement());
    }

    private WaveModifier PickModifier(int wave)
    {
        if (wave < 3) return WaveModifier.None;
        // 40% chance of a modifier from wave 3 onwards
        if (Random.value > 0.4f) return WaveModifier.None;
        var modifiers = (WaveModifier[])System.Enum.GetValues(typeof(WaveModifier));
        // Skip index 0 (None)
        return modifiers[Random.Range(1, modifiers.Length)];
    }

    private string BuildAnnouncement()
    {
        string[] waveLines =
        {
            "THE POLICE HAVE ARRIVED!",
            "MORE COPS? BRING IT ON!",
            "SWAT DEPLOYED!",
            "HEAVY SWAT INCOMING!",
            "THEY BROUGHT THE ARMY?!",
            "DOGS?! THEY BROUGHT DOGS!",
            "SNIPERS ON THE ROOFTOPS!",
            "THIS IS GETTING RIDICULOUS!",
            "IS THAT A GENERAL?!",
            "DUCK APOCALYPSE!"
        };

        if (isRestWave)
            return $"WAVE {currentWave} — BREATHER WAVE\nFewer enemies, more drops!";

        int lineIdx = Mathf.Clamp(currentWave - 1, 0, waveLines.Length - 1);
        string line = currentWave <= waveLines.Length ? waveLines[lineIdx] : "THEY JUST KEEP COMING!";

        string modTag = activeModifier != WaveModifier.None
            ? $"\n<color=#FF4444>[!!] {activeModifier.ToString().ToUpper()}!</color>"
            : "";

        return $"WAVE {currentWave} — {line}{modTag}";
    }

    private void SpawnEnemy()
    {
        GameObject prefab = GetEnemyPrefabForWave(currentWave);
        if (prefab == null) return;

        bool isDrone = prefab == dronePrefab;
        Vector3 spawnPos = GetSpawnPosition(isDrone);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Apply wave modifier
        if (activeModifier != WaveModifier.None)
        {
            EnemyBase eb = enemy.GetComponent<EnemyBase>();
            eb?.ApplyModifier(activeModifier);
        }

        // Extra drops on rest waves
        if (isRestWave)
        {
            EnemyBase eb = enemy.GetComponent<EnemyBase>();
            if (eb != null)
            {
                // Boost drop chance on rest wave
            }
        }

        enemiesAlive++;
    }

    private Vector3 GetSpawnPosition(bool aerial)
    {
        if (aerial && aerialSpawnPoints != null && aerialSpawnPoints.Length > 0)
            return aerialSpawnPoints[Random.Range(0, aerialSpawnPoints.Length)].position;

        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfW = cam.orthographicSize * cam.aspect + 2f;
            float side = Random.value > 0.5f ? halfW : -halfW;
            float y = aerial ? cam.orthographicSize * 0.6f : 0f;
            return new Vector3(cam.transform.position.x + side, y, 0f);
        }
        return new Vector3(15f * (Random.value > 0.5f ? 1f : -1f), 0f, 0f);
    }

    private GameObject GetEnemyPrefabForWave(int wave)
    {
        if (isRestWave)
            return Fallback(policePrefab);

        float roll = Random.value;

        if (wave <= 2)
            return Fallback(policePrefab);

        if (wave <= 4)
        {
            if (roll < 0.15f) return Fallback(riotShieldPrefab, swatPrefab, policePrefab);
            if (roll < 0.20f) return Fallback(k9Prefab, policePrefab);
            return roll < 0.65f ? Fallback(swatPrefab, policePrefab) : Fallback(policePrefab);
        }

        if (wave <= 7)
        {
            if (roll < 0.12f) return Fallback(sniperPrefab, armyPrefab);
            if (roll < 0.22f) return Fallback(dronePrefab, armyPrefab);
            if (roll < 0.32f) return Fallback(k9Prefab, policePrefab);
            if (roll < 0.45f) return Fallback(riotShieldPrefab, swatPrefab);
            return roll < 0.7f ? Fallback(armyPrefab, swatPrefab) : Fallback(swatPrefab, policePrefab);
        }

        // Wave 8+: heavy mix
        if (roll < 0.18f) return Fallback(sniperPrefab, armyPrefab);
        if (roll < 0.33f) return Fallback(dronePrefab, armyPrefab);
        if (roll < 0.43f) return Fallback(k9Prefab, policePrefab);
        if (roll < 0.55f) return Fallback(riotShieldPrefab, swatPrefab);
        return roll < 0.8f ? Fallback(armyPrefab) : Fallback(swatPrefab, policePrefab);
    }

    /// Returns first non-null prefab from candidates.
    private GameObject Fallback(params GameObject[] candidates)
    {
        foreach (var c in candidates)
            if (c != null) return c;
        return null;
    }

    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        GameManager.Instance?.RegisterKill();

        if (enemiesAlive <= 0 && !isSpawning)
        {
            waveTimer = timeBetweenWaves;
            // Wave clear fanfare
            UIManager.Instance?.ShowWaveClear(currentWave);
        }
    }
}
