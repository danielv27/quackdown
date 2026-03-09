using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressively harder enemies.
/// Wave 1-2: Police, Wave 3-4: SWAT, Wave 5+: Army soldiers.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject policePrefab;
    [SerializeField] private GameObject swatPrefab;
    [SerializeField] private GameObject armyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float spawnInterval = 1f;

    [Header("Wave Configuration")]
    [SerializeField] private int baseEnemiesPerWave = 3;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int maxEnemiesPerWave = 20;

    [Header("State")]
    [SerializeField] private int currentWave;
    [SerializeField] private int enemiesAlive;
    [SerializeField] private int enemiesToSpawn;
    [SerializeField] private bool isSpawning;

    private float waveTimer;
    private float spawnTimer;
    private bool wavesActive;

    // Wave announcement messages
    private readonly Dictionary<int, string> waveAnnouncements = new Dictionary<int, string>
    {
        { 1, "WAVE 1 - The police have arrived!" },
        { 2, "WAVE 2 - More cops? Bring it on!" },
        { 3, "WAVE 3 - SWAT DEPLOYED!" },
        { 4, "WAVE 4 - HEAVY SWAT INCOMING!" },
        { 5, "WAVE 5 - THE ARMY?! THEY BROUGHT THE ARMY!" },
        { 6, "WAVE 6 - THEY BROUGHT A TANK?!" },
        { 7, "WAVE 7 - THIS IS GETTING RIDICULOUS!" },
        { 8, "WAVE 8 - IS THAT A GENERAL?!" },
        { 9, "WAVE 9 - THE ENTIRE MILITARY?!" },
        { 10, "WAVE 10 - DUCK APOCALYPSE!" }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Start the wave system.
    /// </summary>
    public void StartWaves()
    {
        currentWave = 0;
        enemiesAlive = 0;
        wavesActive = true;
        waveTimer = 2f; // Short delay before first wave
    }

    /// <summary>
    /// Stop spawning waves.
    /// </summary>
    public void StopWaves()
    {
        wavesActive = false;
        isSpawning = false;
    }

    private void Update()
    {
        if (!wavesActive) return;

        // Wait for current wave to be cleared before starting next
        if (!isSpawning && enemiesAlive <= 0)
        {
            waveTimer -= Time.deltaTime;

            if (waveTimer <= 0f)
            {
                StartNextWave();
            }
        }

        // Spawn enemies during wave
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

    /// <summary>
    /// Start the next wave of enemies.
    /// </summary>
    private void StartNextWave()
    {
        currentWave++;

        // Calculate enemies for this wave
        int totalEnemies = Mathf.Min(
            baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease,
            maxEnemiesPerWave
        );

        enemiesToSpawn = totalEnemies;
        isSpawning = true;
        spawnTimer = 0f; // Spawn first enemy immediately

        // Show wave announcement
        string announcement = GetWaveAnnouncement(currentWave);
        Debug.Log(announcement);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveAnnouncement(announcement);
        }
    }

    /// <summary>
    /// Spawn a single enemy at a random spawn point.
    /// </summary>
    private void SpawnEnemy()
    {
        GameObject prefab = GetEnemyPrefabForWave(currentWave);
        if (prefab == null)
        {
            Debug.LogWarning("No enemy prefab assigned for wave " + currentWave);
            return;
        }

        // Pick random spawn point
        Vector3 spawnPos;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPos = sp.position;
        }
        else
        {
            // Default spawn position off-screen to the right
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camRight = cam.transform.position.x + cam.orthographicSize * cam.aspect + 2f;
                // Randomly spawn from left or right
                float side = Random.value > 0.5f ? camRight : cam.transform.position.x - cam.orthographicSize * cam.aspect - 2f;
                spawnPos = new Vector3(side, 0f, 0f);
            }
            else
            {
                spawnPos = new Vector3(15f * (Random.value > 0.5f ? 1f : -1f), 0f, 0f);
            }
        }

        Instantiate(prefab, spawnPos, Quaternion.identity);
        enemiesAlive++;
    }

    /// <summary>
    /// Get the appropriate enemy prefab based on the current wave.
    /// </summary>
    private GameObject GetEnemyPrefabForWave(int wave)
    {
        if (wave <= 2)
            return policePrefab;
        else if (wave <= 4)
        {
            // Mix of police and SWAT
            return Random.value > 0.3f ? swatPrefab ?? policePrefab : policePrefab;
        }
        else
        {
            // Mix of all types, mostly army
            float roll = Random.value;
            if (roll < 0.5f) return armyPrefab ?? swatPrefab ?? policePrefab;
            if (roll < 0.8f) return swatPrefab ?? policePrefab;
            return policePrefab;
        }
    }

    /// <summary>
    /// Called when an enemy is killed. Decrements the alive counter.
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        // If wave is cleared, set timer for next wave
        if (enemiesAlive <= 0 && !isSpawning)
        {
            waveTimer = timeBetweenWaves;
            Debug.Log("Wave " + currentWave + " cleared! Next wave in " + timeBetweenWaves + "s");
        }
    }

    /// <summary>
    /// Get the wave announcement text.
    /// </summary>
    private string GetWaveAnnouncement(int wave)
    {
        if (waveAnnouncements.ContainsKey(wave))
            return waveAnnouncements[wave];
        return "WAVE " + wave + " - THEY JUST KEEP COMING!";
    }

    /// <summary>
    /// Get the current wave number.
    /// </summary>
    public int GetCurrentWave()
    {
        return currentWave;
    }
}
