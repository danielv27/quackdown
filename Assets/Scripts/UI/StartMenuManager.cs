using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the main-menu screen: title, high-score display, play button, and credits.
/// Drop onto the MainMenu Canvas. Assign references from GameSetupEditor or manually.
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    public static StartMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonLabel;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "DuckRevolution";

    [Header("Title Animation")]
    [SerializeField] private float titleBobSpeed = 1.8f;
    [SerializeField] private float titleBobAmount = 6f;

    private Vector3 titleBasePos;
    private float bobTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Ensure clean time-scale if returning from gameplay
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (titleText != null)
        {
            titleBasePos = titleText.rectTransform.anchoredPosition;
            titleText.text = "<b>DUCK\nREVOLUTION</b>";
        }

        if (subtitleText != null)
            subtitleText.text = "The city will fear the waddle.";

        RefreshHighScore();

        if (controlsText != null)
            controlsText.text =
                "<b>Controls</b>\n" +
                "A / D  ·  Arrow Keys  — Move\n" +
                "Space  ·  W  — Jump\n" +
                "Left Click  — Shoot\n" +
                "Right Click  — Egg Grenade\n" +
                "Q  — Quack (Stun Enemies)\n" +
                "Shift  — Wing Dash\n" +
                "S  — Ground Pound (airborne)\n" +
                "R  — Restart  (game over)";

        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
    }

    private void Update()
    {
        // Gently bob the title
        if (titleText != null)
        {
            bobTimer += Time.deltaTime * titleBobSpeed;
            float offset = Mathf.Sin(bobTimer) * titleBobAmount;
            titleText.rectTransform.anchoredPosition = titleBasePos + Vector3.up * offset;
        }

        // Space / Enter also starts the game
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            PlayGame();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void RefreshHighScore()
    {
        int hs = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = hs > 0
                ? $"<color=#FFD700>★ Best Score: {hs}</color>"
                : "<color=#888888>No record yet — be the first!</color>";
    }
}
