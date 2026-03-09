using UnityEngine;

public enum WeaponType { Pistol, Shotgun, AutoRifle, EggLauncher, GoldenFeather }

/// <summary>
/// ScriptableObject that holds configurable weapon stats.
/// Create instances via Assets > Create > DuckRevolution > Weapon Data.
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "DuckRevolution/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Feather Pistol";
    public WeaponType weaponType = WeaponType.Pistol;
    public Sprite weaponSprite;
    public Sprite projectileSprite;
    public Color projectileColor = Color.white;

    [Header("Firing")]
    public float damage = 25f;
    public float fireRate = 0.2f;
    public float projectileSpeed = 15f;
    [Tooltip("Number of projectiles per shot (e.g. 5 for shotgun)")]
    public int projectileCount = 1;
    [Tooltip("Total spread cone in degrees (0 = no spread)")]
    public float spreadAngle = 0f;
    public bool isAutomatic = false;
    public bool piercesEnemies = false;

    [Header("Grenade Mode")]
    [Tooltip("If true, this weapon throws grenades instead of projectiles")]
    public bool throwsGrenade = false;
    public float grenadeThrowForce = 10f;
    public float grenadeCooldown = 2f;

    [Header("Ammo")]
    [Tooltip("-1 = infinite ammo. Positive = limited pickup ammo.")]
    public int ammoCount = -1;

    [Header("Game Feel")]
    public float shakeOnFire = 0.05f;
    public string sfxName = "shoot";

    [Header("Pickup")]
    [Tooltip("Chance for enemies to drop this weapon on death (0-1)")]
    public float dropChance = 0.12f;
}
