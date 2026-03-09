using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages overall game state: start, wave progression, score, game over, and high scores.
/// Bootstraps the JuiceManager and AudioManager singletons.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool gameActive;
    [SerializeField] private int score;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    private static readonly string HighScoreKey = "HighScore";
    private static readonly string HighScoreWaveKey = "HighScoreWave";
    private static readonly string TotalKillsKey = "TotalKills";

    private GameObject currentPlayer;
    private InputAction restartAction;
    private int highScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        restartAction = new InputAction("Restart", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/r");
        restartAction.AddBinding("<Gamepad>/start");
    }

    private void OnEnable() => restartAction.Enable();
    private void OnDisable() => restartAction.Disable();
    private void OnDestroy() => restartAction.Dispose();

    private void Start()
    {
        // Bootstrap singletons that aren't in the scene
        JuiceManager.GetOrCreate();
        AudioManager.GetOrCreate();
        ParticleManager.GetOrCreate();

        StartGame();
    }

    private void Update()
    {
        if (!gameActive && restartAction.WasPressedThisFrame())
            RestartGame();
    }

    public void StartGame()
    {
        score = 0;
        gameActive = true;

        if (playerPrefab != null && currentPlayer == null)
        {
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }

        UIManager.Instance?.UpdateScore(score);
        UIManager.Instance?.ShowTextPopup("THE DUCK REVOLUTION HAS BEGUN!", Vector3.up * 2f);

        WaveManager.Instance?.StartWaves();
    }

    public void AddScore(int points)
    {
        // Apply combo multiplier
        int comboMult = ComboSystem.Instance != null ? ComboSystem.Instance.GetComboMultiplier() : 1;
        score += points * comboMult;
        UIManager.Instance?.UpdateScore(score);
    }

    public int GetScore() => score;
    public int GetHighScore() => highScore;

    public void RegisterKill()
    {
        int kills = PlayerPrefs.GetInt(TotalKillsKey, 0) + 1;
        PlayerPrefs.SetInt(TotalKillsKey, kills);
    }

    public void GameOver()
    {
        gameActive = false;

        bool newHighScore = score > highScore;
        if (newHighScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            if (WaveManager.Instance != null)
                PlayerPrefs.SetInt(HighScoreWaveKey, WaveManager.Instance.GetCurrentWave());
            PlayerPrefs.Save();
        }

        UIManager.Instance?.ShowGameOver(score);
        WaveManager.Instance?.StopWaves();

        if (newHighScore)
            UIManager.Instance?.ShowTextPopup("NEW HIGH SCORE!", Vector3.up * 3f);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public bool IsGameActive() => gameActive;
}

