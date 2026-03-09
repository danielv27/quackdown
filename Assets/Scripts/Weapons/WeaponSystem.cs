using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Handles the player's weapon systems: multi-weapon support, feather shots, and egg grenades.
/// Weapons can be data-driven via WeaponData ScriptableObjects, or fall back to inspector values.
/// Uses ObjectPool&lt;GameObject&gt; for zero-alloc projectile spawning.
/// Attach to the Player GameObject alongside PlayerController.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Default Weapon (fallback if no WeaponData assigned)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 25f;

    [Header("Egg Grenade")]
    [SerializeField] private GameObject eggGrenadePrefab;
    [SerializeField] private float grenadeThrowForce = 10f;
    [SerializeField] private float grenadeCooldown = 2f;

    [Header("Weapon Inventory")]
    [SerializeField] private WeaponData[] startingWeapons;
    [Tooltip("Max number of picked-up weapons the player can carry simultaneously")]
    [SerializeField] private int maxPickupWeapons = 3;

    [Header("Muzzle Flash")]
    [Tooltip("Optional child GameObject that flashes briefly on shoot")]
    [SerializeField] private GameObject muzzleFlash;

    private float fireTimer;
    private float grenadeTimer;

    // Weapon inventory: slot 0 = default pistol (infinite), slots 1+ = pickups
    private readonly List<WeaponData> weaponInventory = new List<WeaponData>();
    private int currentWeaponIndex = 0;
    private int[] weaponAmmo;

    // Active powerup modifiers
    private float damageMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private float grenadeRadiusMultiplier = 1f;

    // Projectile pool — zero-alloc spawning
    private ObjectPool<GameObject> projectilePool;

    private void Awake()
    {
        // Populate initial inventory from starting weapons
        if (startingWeapons != null)
            weaponInventory.AddRange(startingWeapons);

        weaponAmmo = new int[maxPickupWeapons + 1];
        for (int i = 0; i < weaponAmmo.Length; i++) weaponAmmo[i] = -1; // -1 = infinite

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);

        // Create projectile pool (only if prefab is assigned)
        if (projectilePrefab != null)
        {
            projectilePool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(projectilePrefab),
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: go => Destroy(go),
                collectionCheck: false,
                defaultCapacity: 20,
                maxSize: 60
            );
        }
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;
        grenadeTimer -= Time.deltaTime;

        // Cycle weapons with scroll wheel or 1/2/3 keys
        if (weaponInventory.Count > 1)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f) CycleWeapon(1);
            else if (scroll < 0f) CycleWeapon(-1);

            for (int i = 0; i < Mathf.Min(weaponInventory.Count, 9); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) currentWeaponIndex = i;
        }
    }

    private void CycleWeapon(int direction)
    {
        currentWeaponIndex = (currentWeaponIndex + direction + weaponInventory.Count) % weaponInventory.Count;
        WeaponData w = GetCurrentWeaponData();
        if (w != null && UIManager.Instance != null)
            UIManager.Instance.ShowTextPopup(w.weaponName, transform.position + Vector3.up * 1.5f);
    }

    /// <summary>Fires a single shot (tap, called once on button press).</summary>
    public void Shoot(Vector2 direction)
    {
        WeaponData weapon = GetCurrentWeaponData();

        bool isAutomatic = weapon != null ? weapon.isAutomatic : false;
        if (isAutomatic) return; // Auto weapons handled in ShootAuto

        FireShot(direction, weapon);
    }

    /// <summary>Called every frame while fire button held — handles automatic weapons.</summary>
    public void ShootAuto(Vector2 direction)
    {
        WeaponData weapon = GetCurrentWeaponData();
        if (weapon == null || !weapon.isAutomatic) return;
        FireShot(direction, weapon);
    }

    private void FireShot(Vector2 direction, WeaponData weapon)
    {
        float currentFireRate = weapon != null ? weapon.fireRate : fireRate;
        currentFireRate /= fireRateMultiplier;

        if (fireTimer > 0f) return;
        if (projectilePrefab == null || firePoint == null) return;

        // Check grenade launcher mode
        if (weapon != null && weapon.throwsGrenade)
        {
            ThrowGrenade(direction);
            return;
        }

        // Check ammo
        if (currentWeaponIndex < weaponAmmo.Length && weaponAmmo[currentWeaponIndex] == 0)
        {
            // Out of ammo — drop this weapon
            if (weaponInventory.Count > 1)
            {
                weaponInventory.RemoveAt(currentWeaponIndex);
                currentWeaponIndex = Mathf.Clamp(currentWeaponIndex - 1, 0, weaponInventory.Count - 1);
                UIManager.Instance?.ShowTextPopup("OUT OF AMMO!", transform.position + Vector3.up);
            }
            return;
        }

        fireTimer = currentFireRate;

        // Decrement ammo
        if (currentWeaponIndex < weaponAmmo.Length && weaponAmmo[currentWeaponIndex] > 0)
            weaponAmmo[currentWeaponIndex]--;

        int count = weapon != null ? weapon.projectileCount : 1;
        float spread = weapon != null ? weapon.spreadAngle : 0f;
        float dmg = (weapon != null ? weapon.damage : projectileDamage) * damageMultiplier;
        float speed = weapon != null ? weapon.projectileSpeed : projectileSpeed;
        bool pierces = weapon != null && weapon.piercesEnemies;

        for (int i = 0; i < count; i++)
        {
            Vector2 shotDir = direction;
            if (spread > 0f && count > 1)
            {
                float angle = Mathf.Lerp(-spread / 2f, spread / 2f, count == 1 ? 0f : (float)i / (count - 1));
                shotDir = Quaternion.Euler(0, 0, angle) * direction;
            }
            else if (spread > 0f)
            {
                shotDir = Quaternion.Euler(0, 0, Random.Range(-spread / 2f, spread / 2f)) * direction;
            }

            SpawnProjectile(shotDir, dmg, speed, weapon, pierces);
        }

        // Screen shake on fire
        float shake = weapon != null ? weapon.shakeOnFire : 0.04f;
        CameraFollow.ShakeCamera(shake);

        // Muzzle flash
        if (muzzleFlash != null)
            StartCoroutine(FlashMuzzle());

        // Shell casing particle
        bool facingRight = transform.localScale.x > 0f;
        ParticleManager.SpawnShellCasing(firePoint != null ? firePoint.position : transform.position, facingRight);

        // SFX
        string sfx = weapon != null ? weapon.sfxName : "shoot";
        AudioManager.PlaySFX(sfx);
    }

    private void SpawnProjectile(Vector2 direction, float damage, float speed, WeaponData weapon, bool pierces)
    {
        GameObject proj;
        if (projectilePool != null)
            proj = projectilePool.Get();
        else
            proj = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);

        proj.transform.position = firePoint != null ? firePoint.position : transform.position;
        proj.transform.rotation = Quaternion.identity;
        proj.SetActive(true);

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, damage, true);
            projectile.SetSpeed(speed);
            projectile.SetPiercing(pierces);

            if (weapon?.projectileSprite != null)
                projectile.SetSprite(weapon.projectileSprite, weapon.projectileColor);

            // Return to pool when lifetime expires
            if (projectilePool != null)
                projectile.SetPool(projectilePool);
        }
    }

    private IEnumerator FlashMuzzle()
    {
        muzzleFlash.SetActive(true);
        yield return null; // One frame
        yield return null;
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    /// <summary>Throws an egg grenade in the given direction.</summary>
    public void ThrowGrenade(Vector2 direction)
    {
        float currentGrenadeCooldown = grenadeCooldown;
        WeaponData weapon = GetCurrentWeaponData();
        if (weapon != null && weapon.throwsGrenade)
            currentGrenadeCooldown = weapon.grenadeCooldown;

        if (grenadeTimer > 0f) return;
        if (eggGrenadePrefab == null || firePoint == null) return;

        grenadeTimer = currentGrenadeCooldown;

        GameObject egg = Instantiate(eggGrenadePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D eggRb = egg.GetComponent<Rigidbody2D>();

        if (eggRb != null)
        {
            Vector2 throwDir = direction;
            throwDir.y += 0.3f;
            throwDir.Normalize();
            float force = weapon != null && weapon.throwsGrenade ? weapon.grenadeThrowForce : grenadeThrowForce;
            eggRb.AddForce(throwDir * force, ForceMode2D.Impulse);
            eggRb.AddTorque(5f);
        }

        EggGrenade grenade = egg.GetComponent<EggGrenade>();
        if (grenade != null)
        {
            grenade.SetFriendly(true);
            grenade.SetRadiusMultiplier(grenadeRadiusMultiplier);
        }

        CameraFollow.ShakeCamera(0.05f);
        AudioManager.PlaySFX("grenade_throw");
    }

    /// <summary>Pick up a weapon dropped by an enemy.</summary>
    public void PickupWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;

        // Don't exceed pickup slots
        int pickupCount = weaponInventory.Count - (startingWeapons != null ? startingWeapons.Length : 0);
        if (pickupCount >= maxPickupWeapons)
        {
            // Replace current weapon
            int replaceIdx = currentWeaponIndex;
            if (replaceIdx < weaponInventory.Count)
                weaponInventory[replaceIdx] = weaponData;
        }
        else
        {
            weaponInventory.Add(weaponData);
            currentWeaponIndex = weaponInventory.Count - 1;
        }

        // Set ammo
        int idx = Mathf.Clamp(currentWeaponIndex, 0, weaponAmmo.Length - 1);
        weaponAmmo[idx] = weaponData.ammoCount;

        UIManager.Instance?.ShowTextPopup($"PICKED UP {weaponData.weaponName.ToUpper()}!", transform.position + Vector3.up * 1.5f);
        AudioManager.PlaySFX("pickup");
    }

    private WeaponData GetCurrentWeaponData()
    {
        if (weaponInventory.Count == 0) return null;
        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, weaponInventory.Count - 1);
        return weaponInventory[currentWeaponIndex];
    }

    // --- Powerup effect methods ---

    public void ApplyDamageBoost(float multiplier, float duration)
    {
        StartCoroutine(DamageBoostCoroutine(multiplier, duration));
    }

    private IEnumerator DamageBoostCoroutine(float multiplier, float duration)
    {
        damageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        damageMultiplier = 1f;
    }

    public void ApplyRapidFire(float duration)
    {
        StartCoroutine(RapidFireCoroutine(duration));
    }

    private IEnumerator RapidFireCoroutine(float duration)
    {
        fireRateMultiplier = 2f;
        yield return new WaitForSeconds(duration);
        fireRateMultiplier = 1f;
    }

    public void ApplyBigEgg(float duration)
    {
        StartCoroutine(BigEggCoroutine(duration));
    }

    private IEnumerator BigEggCoroutine(float duration)
    {
        grenadeRadiusMultiplier = 2f;
        yield return new WaitForSeconds(duration);
        grenadeRadiusMultiplier = 1f;
    }
}
