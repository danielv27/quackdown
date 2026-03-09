using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemy waves in order.
/// Each wave defines which enemies to spawn, from which spawn points.
/// After all enemies in a wave die, a short delay passes and the next wave starts.
///
/// Attach to a dedicated "WaveManager" GameObject.
/// Wire spawn points as child Transforms or set them in the Inspector.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // ---- Wave Definition ----
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject  enemyPrefab;
        public int         count    = 3;
        [Tooltip("Seconds between each spawn in this entry")]
        public float       interval = 0.5f;
    }

    [System.Serializable]
    public class Wave
    {
        public string          waveName    = "Wave";
        [Tooltip("The announcement shown on screen")]
        public string          announcement = "";
        public EnemySpawnEntry[] enemies;
        [Tooltip("Delay before this wave starts (after previous wave clears)")]
        public float           predelay = 2f;
    }

    // ---- Inspector ----
    [Header("Waves")]
    public Wave[] waves;

    [Header("Spawn Points")]
    [Tooltip("Enemy spawn locations (right side, left side, etc.)")]
    public Transform[] spawnPoints;

    [Header("Settings")]
    [SerializeField] private float waveEndDelay = 3f;
    [Tooltip("If true, wave N+1 loops after all waves are done")]
    [SerializeField] private bool  loopWaves    = false;

    // ---- State ----
    private int              _currentWaveIndex;
    private List<GameObject> _liveEnemies = new List<GameObject>();
    private bool             _waveInProgress;

    // ---- Events ----
    public static event System.Action<int, string> OnWaveStarted;  // waveNumber, announcement
    public static event System.Action<int>          OnWaveCleared;  // waveNumber

    // ------------------------------------------------

    /// <summary>Called by GameManager.Start().</summary>
    public void StartWaves()
    {
        _currentWaveIndex = 0;
        StartCoroutine(RunWaves());
    }

    // ---- Main coroutine ----
    private IEnumerator RunWaves()
    {
        while (_currentWaveIndex < waves.Length)
        {
            Wave wave = waves[_currentWaveIndex];

            // Pre-delay
            yield return new WaitForSeconds(wave.predelay);

            // Announce
            string msg = string.IsNullOrEmpty(wave.announcement)
                ? $"WAVE {_currentWaveIndex + 1}: {wave.waveName}"
                : wave.announcement;
            Debug.Log($">>> {msg} <<<");
            OnWaveStarted?.Invoke(_currentWaveIndex + 1, msg);

            // Spawn all entries
            _liveEnemies.Clear();
            foreach (EnemySpawnEntry entry in wave.enemies)
                yield return StartCoroutine(SpawnEntry(entry));

            _waveInProgress = true;

            // Wait until all enemies are dead
            yield return StartCoroutine(WaitForWaveClear());

            OnWaveCleared?.Invoke(_currentWaveIndex + 1);
            Debug.Log($"Wave {_currentWaveIndex + 1} cleared!");

            yield return new WaitForSeconds(waveEndDelay);

            _currentWaveIndex++;
            if (loopWaves && _currentWaveIndex >= waves.Length)
                _currentWaveIndex = 0;
        }

        // All waves finished
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerVictory();
    }

    // ---- Spawn a single entry ----
    private IEnumerator SpawnEntry(EnemySpawnEntry entry)
    {
        if (entry.enemyPrefab == null) yield break;

        for (int i = 0; i < entry.count; i++)
        {
            Transform spawnPoint = PickSpawnPoint();
            GameObject enemy = Instantiate(entry.enemyPrefab, spawnPoint.position, Quaternion.identity);
            // Ensure the enemy is active (prefab template may be stored inactive)
            enemy.SetActive(true);
            _liveEnemies.Add(enemy);
            yield return new WaitForSeconds(entry.interval);
        }
    }

    // ---- Wait for all enemies in wave to die ----
    private IEnumerator WaitForWaveClear()
    {
        while (true)
        {
            // Clean up destroyed entries
            _liveEnemies.RemoveAll(e => e == null);
            if (_liveEnemies.Count == 0)
                break;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ---- Pick a random spawn point ----
    private Transform PickSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform; // fallback: use WaveManager's transform

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // ---- Gizmos: visualise spawn points ----
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (Transform sp in spawnPoints)
        {
            if (sp != null)
                Gizmos.DrawWireSphere(sp.position, 0.4f);
        }
    }
}
