using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all HUD and UI elements:
///   - Health bar
///   - Score display
///   - Wave announcement banner
///   - Game Over / Victory screens
///
/// Attach to a dedicated "UIManager" GameObject.
/// Wire all fields in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ---- Health ----
    [Header("Health Bar")]
    public UnityEngine.UI.Slider healthSlider;
    public TextMeshProUGUI healthText;

    // ---- Score ----
    [Header("Score")]
    public TextMeshProUGUI scoreText;

    // ---- Wave Announcement ----
    [Header("Wave Announcement")]
    public GameObject      announcementPanel;
    public TextMeshProUGUI announcementText;
    [SerializeField] private float announcementDuration = 3f;

    // ---- Game Over / Victory ----
    [Header("Game Over / Victory")]
    public GameObject      gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public GameObject      victoryPanel;

    // ---- Wave Counter ----
    [Header("Wave Counter")]
    public TextMeshProUGUI waveCounterText;

    // ------------------------------------------------
    void OnEnable()
    {
        GameManager.OnScoreChanged      += UpdateScore;
        GameManager.OnGameStateChanged  += OnGameStateChanged;
        WaveManager.OnWaveStarted       += ShowWaveAnnouncement;
        WaveManager.OnWaveCleared       += UpdateWaveCounter;
    }

    void OnDisable()
    {
        GameManager.OnScoreChanged      -= UpdateScore;
        GameManager.OnGameStateChanged  -= OnGameStateChanged;
        WaveManager.OnWaveStarted       -= ShowWaveAnnouncement;
        WaveManager.OnWaveCleared       -= UpdateWaveCounter;
    }

    void Start()
    {
        // Hide panels at start
        if (announcementPanel != null) announcementPanel.SetActive(false);
        if (gameOverPanel     != null) gameOverPanel.SetActive(false);
        if (victoryPanel      != null) victoryPanel.SetActive(false);

        UpdateScore(0);

        // Hook health bar to player's HealthSystem
        // Done here (Start) so the player is guaranteed to exist
        StartCoroutine(HookPlayerHealth());
    }

    private System.Collections.IEnumerator HookPlayerHealth()
    {
        // Wait a frame to make sure player's Start() has also run
        yield return null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        HealthSystem hs = player.GetComponent<HealthSystem>();
        if (hs == null) yield break;

        // Update health bar whenever health changes
        hs.onHealthChanged.AddListener((hp) => UpdateHealth(hp, hs.MaxHealth));

        // Initialise bar to current values
        UpdateHealth(hs.CurrentHealth, hs.MaxHealth);
    }

    // ---- Health ----
    /// <summary>Called externally (e.g., HealthSystem) to update the bar.</summary>
    public void UpdateHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value    = current;
        }
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    // ---- Score ----
    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score:N0}";
    }

    // ---- Wave Announcement ----
    private void ShowWaveAnnouncement(int waveNumber, string message)
    {
        if (announcementPanel == null) return;
        StartCoroutine(ShowBanner(message));
    }

    private IEnumerator ShowBanner(string message)
    {
        if (announcementText != null)
            announcementText.text = message;
        announcementPanel.SetActive(true);

        yield return new WaitForSeconds(announcementDuration);

        announcementPanel.SetActive(false);
    }

    // ---- Wave Counter ----
    private void UpdateWaveCounter(int waveNumber)
    {
        if (waveCounterText != null)
            waveCounterText.text = $"WAVE {waveNumber} CLEARED!";
    }

    // ---- Game State ----
    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                if (gameOverText  != null) gameOverText.text = "YOU HAVE FALLEN.\nThe humans have won…for now.";
                break;

            case GameManager.GameState.Victory:
                if (victoryPanel != null) victoryPanel.SetActive(true);
                break;
        }
    }
}
