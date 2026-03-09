using UnityEngine;

/// <summary>
/// Manages overall game state: wave progression, score, game over/win conditions.
/// Attach this to a dedicated "GameManager" GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- Game State ----
    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ---- Score ----
    public int Score { get; private set; }

    // ---- References ----
    [Header("References")]
    [Tooltip("The WaveManager that drives enemy spawning")]
    public WaveManager waveManager;

    // ---- Events ----
    public static event System.Action<int>      OnScoreChanged;
    public static event System.Action<GameState> OnGameStateChanged;

    // ------------------------------------------------
    void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Kick off the first wave
        if (waveManager != null)
            waveManager.StartWaves();
        else
            Debug.LogWarning("GameManager: No WaveManager assigned!");
    }

    // ---- Public API ----

    /// <summary>Called by enemies when they die.</summary>
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Called by HealthSystem when the player dies.</summary>
    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.GameOver);
        Debug.Log("GAME OVER! The ducks have been defeated…");
    }

    /// <summary>Called by WaveManager when all waves are cleared.</summary>
    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Victory);
        Debug.Log("VICTORY! The Duck Revolution is unstoppable!");
    }

    public void PauseGame()  => SetState(GameState.Paused);
    public void ResumeGame() => SetState(GameState.Playing);

    // ---- Helpers ----
    private void SetState(GameState newState)
    {
        CurrentState = newState;
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        OnGameStateChanged?.Invoke(newState);
    }
}
