using UnityEngine;

public enum PowerupType { Health, Speed, Damage, Shield, RapidFire, BigEgg }

/// <summary>
/// ScriptableObject that holds configurable powerup stats.
/// Create instances via Assets > Create > DuckRevolution > Powerup Data.
/// </summary>
[CreateAssetMenu(fileName = "NewPowerupData", menuName = "DuckRevolution/Powerup Data")]
public class PowerupData : ScriptableObject
{
    [Header("Identity")]
    public string powerupName = "Health Egg";
    public PowerupType powerupType = PowerupType.Health;
    public Sprite sprite;
    public Color glowColor = Color.green;

    [Header("Effect")]
    [Tooltip("Duration in seconds. 0 = instant one-time effect.")]
    public float duration = 8f;
    [Tooltip("Heal amount for Health, multiplier for Speed/Damage, absorption for Shield")]
    public float magnitude = 25f;

    [Header("Pickup")]
    public string popupText = "+HEALTH!";
    public float floatAmplitude = 0.4f;
    public float floatSpeed = 2f;
}
