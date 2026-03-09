using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Tracks kill combos and applies score multipliers.
/// Triggers slow-mo at high combo streaks.
/// Attach to the Player GameObject (added automatically by PlayerController).
/// </summary>
public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 3f;
    [SerializeField] private int slowMoComboThreshold = 5;

    private int comboCount;
    private float comboTimer;
    private int totalKills;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (comboCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }
    }

    /// <summary>Register a kill and return the score multiplier float.</summary>
    public float RegisterKill()
    {
        comboCount++;
        totalKills++;
        comboTimer = comboResetTime;

        if (comboCount >= slowMoComboThreshold)
            JuiceManager.Instance?.TriggerSlowMo();

        return GetComboMultiplierFloat();
    }

    private void ResetCombo() { comboCount = 0; comboTimer = 0f; }

    public int GetKillStreak() => comboCount;
    public int GetComboMultiplier() => Mathf.Min(1 + comboCount / 2, 4);
    public float GetComboMultiplierFloat() => Mathf.Min(1f + comboCount * 0.2f, 4f);
    public int GetTotalKills() => totalKills;
}
