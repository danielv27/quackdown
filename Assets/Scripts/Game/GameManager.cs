using UnityEngine;
using UnityEngine.InputSystem;

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
    private InputAction restartAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        restartAction = new InputAction("Restart", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/r");
    }

    private void OnEnable()
    {
        restartAction.Enable();
    }

    private void OnDisable()
    {
        restartAction.Disable();
    }

    private void OnDestroy()
    {
        restartAction.Dispose();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!gameActive && restartAction.WasPressedThisFrame())
            RestartGame();
    }

    /// <summary>
    /// Start a new game.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        gameActive = true;

        if (playerPrefab != null && currentPlayer == null)
        {
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.ShowTextPopup("THE DUCK REVOLUTION HAS BEGUN!", Vector3.up * 2f);
        }

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
            UIManager.Instance.ShowGameOver(score);

        if (WaveManager.Instance != null)
            WaveManager.Instance.StopWaves();
    }

    /// <summary>
    /// Restart the game.
    /// </summary>
    public void RestartGame()
    {
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

