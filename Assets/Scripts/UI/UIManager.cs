using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements: score, health, wave info, text popups.
/// Singleton pattern for easy access.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;

    [Header("Text Popup")]
    [SerializeField] private GameObject textPopupPrefab;
    [SerializeField] private Canvas worldCanvas; // Canvas set to World Space for popups

    [Header("Wave Announcement")]
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private float announcementDuration = 3f;

    private float announcementTimer;
    private GameObject cachedPlayer;
    private HealthSystem cachedPlayerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Fade out announcement
        if (announcementTimer > 0f)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0f && announcementText != null)
            {
                announcementText.gameObject.SetActive(false);
            }
        }

        // Update health display
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    /// <summary>
    /// Show a wave announcement across the screen.
    /// </summary>
    public void ShowWaveAnnouncement(string text)
    {
        if (announcementText != null)
        {
            announcementText.text = text;
            announcementText.gameObject.SetActive(true);
            announcementTimer = announcementDuration;
        }

        Debug.Log("[ANNOUNCEMENT] " + text);
    }

    /// <summary>
    /// Show a floating text popup in the world.
    /// </summary>
    public void ShowTextPopup(string text, Vector3 worldPosition)
    {
        if (textPopupPrefab != null && worldCanvas != null)
        {
            GameObject popup = Instantiate(textPopupPrefab, worldPosition, Quaternion.identity, worldCanvas.transform);
            TextPopup tp = popup.GetComponent<TextPopup>();
            if (tp != null)
                tp.Initialize(text);
        }
        else
        {
            // Fallback: create a simple text popup without prefab
            CreateSimplePopup(text, worldPosition);
        }

        Debug.Log("[POPUP] " + text);
    }

    /// <summary>
    /// Show the game over screen.
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverScoreText != null)
                gameOverScoreText.text = "FINAL SCORE: " + finalScore + "\n\nThe revolution will return...\n\nPress R to restart";
        }
    }

    /// <summary>
    /// Update the health bar display.
    /// </summary>
    private void UpdateHealthDisplay()
    {
        // Cache player reference to avoid FindGameObjectWithTag every frame
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (cachedPlayer != null)
                cachedPlayerHealth = cachedPlayer.GetComponent<HealthSystem>();
        }
        if (cachedPlayer == null || cachedPlayerHealth == null) return;

        if (healthBar != null)
            healthBar.fillAmount = cachedPlayerHealth.GetHealthPercent();

        if (healthText != null)
            healthText.text = "HP: " + Mathf.Ceil(cachedPlayerHealth.GetCurrentHealth());
    }

    /// <summary>
    /// Creates a simple floating text popup without needing a prefab.
    /// </summary>
    private void CreateSimplePopup(string text, Vector3 worldPosition)
    {
        // Create a world-space canvas for the popup
        GameObject popupObj = new GameObject("TextPopup");
        popupObj.transform.position = worldPosition;

        // Add TextPopup component for animation
        TextPopup popup = popupObj.AddComponent<TextPopup>();
        popup.Initialize(text);
    }
}
