using UnityEngine;

/// <summary>
/// Manages the overall game state: start, wave progression, score, game over.
/// Singleton pattern for easy access from other scripts.
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

    private GameObject currentPlayer;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        // Press R to restart when game is over
        if (!gameActive && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Start a new game.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        gameActive = true;

        // Spawn player if prefab is assigned and no player exists
        if (playerPrefab != null && currentPlayer == null)
        {
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.ShowTextPopup("THE DUCK REVOLUTION HAS BEGUN!", Vector3.up * 2f);
        }

        // Start wave spawning
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartWaves();

        Debug.Log("=== DUCK REVOLUTION BEGINS ===");
    }

    /// <summary>
    /// Add score points.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(score);
    }

    /// <summary>
    /// Get the current score.
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// Called when the player dies. Game over!
    /// </summary>
    public void GameOver()
    {
        gameActive = false;
        Debug.Log("GAME OVER - The revolution has been quelled... for now.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(score);
        }

        // Stop wave spawning
        if (WaveManager.Instance != null)
            WaveManager.Instance.StopWaves();
    }

    /// <summary>
    /// Restart the game.
    /// </summary>
    public void RestartGame()
    {
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    /// <summary>
    /// Check if the game is currently active.
    /// </summary>
    public bool IsGameActive()
    {
        return gameActive;
    }
}
