using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages all UI elements: score, health, wave info, text popups, combo display.
/// Health bar uses green→yellow→red gradient. Score rolls up smoothly.
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
    [SerializeField] private Canvas worldCanvas;

    [Header("Wave Announcement")]
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private float announcementDuration = 3f;

    [Header("Combo Display")]
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Cursor")]
    [SerializeField] private RectTransform cursorImage;

    [Header("Score Animation")]
    [SerializeField] private float scoreRollSpeed = 5f;

    // Health bar gradient colors
    private static readonly Color HealthFull = new Color(0.2f, 0.85f, 0.2f);   // green
    private static readonly Color HealthMid  = new Color(1f, 0.85f, 0.1f);     // yellow
    private static readonly Color HealthLow  = new Color(0.9f, 0.15f, 0.1f);   // red

    private float announcementTimer;
    private GameObject cachedPlayer;
    private HealthSystem cachedPlayerHealth;

    private float displayedScore;
    private int targetScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Cursor.visible = false; // Use software crosshair instead
    }

    private void Update()
    {
        if (announcementTimer > 0f)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0f && announcementText != null)
                announcementText.gameObject.SetActive(false);
        }

        UpdateHealthDisplay();
        UpdateScoreRollup();
        UpdateComboDisplay();
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        if (cursorImage == null || Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        cursorImage.position = new Vector3(mousePos.x, mousePos.y, 0f);
    }

    public void UpdateScore(int score)
    {
        targetScore = score;
    }

    private void UpdateScoreRollup()
    {
        if (scoreText == null) return;
        displayedScore = Mathf.Lerp(displayedScore, targetScore, Time.deltaTime * scoreRollSpeed);
        scoreText.text = "SCORE: " + Mathf.RoundToInt(displayedScore);

        // Scale punch when score changes fast
        float diff = Mathf.Abs(targetScore - displayedScore);
        float punchScale = 1f + Mathf.Clamp01(diff / 500f) * 0.25f;
        scoreText.transform.localScale = Vector3.Lerp(
            scoreText.transform.localScale,
            Vector3.one * punchScale,
            Time.deltaTime * 12f
        );
    }

    public void ShowWaveAnnouncement(string text)
    {
        if (announcementText != null)
        {
            announcementText.text = text;
            announcementText.gameObject.SetActive(true);
            announcementTimer = announcementDuration;
            // Scale punch animation — snap large then recover in Update
            announcementText.transform.localScale = Vector3.one * 1.4f;
        }
        AudioManager.PlaySFX("wave_start");
    }

    public void ShowWaveClear(int wave)
    {
        string[] fanfares = { "WAVE CLEARED!", "THEY'RE DOWN!", "THE DUCKS PREVAIL!", "QUACK VICTORY!" };
        string msg = $"<color=#44FF44>{fanfares[wave % fanfares.Length]}</color>\nWave {wave} complete!";
        ShowWaveAnnouncement(msg);
        CameraFollow.ShakeCamera(0.12f);
        AudioManager.PlaySFX("wave_clear");
    }

    public void ShowTextPopup(string text, Vector3 worldPosition)
    {
        if (textPopupPrefab != null && worldCanvas != null)
        {
            GameObject popup = Instantiate(textPopupPrefab, worldPosition, Quaternion.identity, worldCanvas.transform);
            popup.GetComponent<TextPopup>()?.Initialize(text);
        }
        else
        {
            var popupObj = new GameObject("TextPopup");
            popupObj.transform.position = worldPosition;
            popupObj.AddComponent<TextPopup>().Initialize(text);
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        int highScore = GameManager.Instance != null ? GameManager.Instance.GetHighScore() : finalScore;
        int wave = WaveManager.Instance != null ? WaveManager.Instance.GetCurrentWave() : 0;
        int kills = ComboSystem.Instance != null ? ComboSystem.Instance.GetTotalKills() : 0;
        bool newRecord = finalScore >= highScore && finalScore > 0;

        string newRecordLine = newRecord ? "\n<color=#FFD700><b>* NEW HIGH SCORE! *</b></color>" : $"\nBest: {highScore}";
        string body =
            $"<size=120%><b>FINAL SCORE: {finalScore}</b></size>{newRecordLine}\n\n" +
            $"Wave Reached: {wave}\n" +
            $"Enemies Defeated: {kills}\n\n" +
            "<color=#AAAAAA>The revolution will return...\n\n" +
            "Press <b>R</b> to play again  |  <b>ESC</b> for main menu</color>";

        if (gameOverScoreText != null)
            gameOverScoreText.text = body;
    }

    private void UpdateHealthDisplay()
    {
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (cachedPlayer != null)
                cachedPlayerHealth = cachedPlayer.GetComponent<HealthSystem>();
        }
        if (cachedPlayerHealth == null) return;

        float pct = cachedPlayerHealth.GetHealthPercent();

        if (healthBar != null)
        {
            healthBar.fillAmount = pct;
            healthBar.color = pct > 0.5f
                ? Color.Lerp(HealthMid, HealthFull, (pct - 0.5f) * 2f)
                : Color.Lerp(HealthLow, HealthMid, pct * 2f);
        }

        if (healthText != null)
            healthText.text = "HP: " + Mathf.Ceil(cachedPlayerHealth.GetCurrentHealth());
    }

    private void UpdateComboDisplay()
    {
        if (comboText == null) return;
        if (ComboSystem.Instance == null) return;

        int mult = ComboSystem.Instance.GetComboMultiplier();
        int streak = ComboSystem.Instance.GetKillStreak();

        if (streak >= 2)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{mult} COMBO  ({streak} kills)";
            // Scale pulse
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
            comboText.transform.localScale = Vector3.one * pulse;
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }
}
