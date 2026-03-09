using UnityEngine;

/// <summary>
/// A weapon pickup that enemies drop on death.
/// Player walks into it to add the weapon to their inventory.
/// Floats and bobs to be visible. Auto-despawns after 15 seconds.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float lifetime = 15f;

    private Vector3 startPos;
    private float bobTimer;
    private SpriteRenderer sr;

    private void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();

        // Blink-out warning before despawn
        Invoke(nameof(StartBlinking), lifetime - 3f);
        Destroy(gameObject, lifetime);
    }

    public void SetWeaponData(WeaponData data)
    {
        weaponData = data;

        // Tint sprite to match weapon color
        SpriteRenderer r = GetComponent<SpriteRenderer>();
        if (r != null && data != null)
            r.color = data.projectileColor;
    }

    private void Update()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        transform.position = startPos + new Vector3(0f, Mathf.Sin(bobTimer) * bobHeight, 0f);

        // Rotate slowly
        transform.Rotate(0f, 0f, 60f * Time.deltaTime);
    }

    private void StartBlinking()
    {
        if (sr != null)
            InvokeRepeating(nameof(ToggleVisible), 0f, 0.2f);
    }

    private void ToggleVisible()
    {
        if (sr != null) sr.enabled = !sr.enabled;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (weaponData == null) return;

        WeaponSystem ws = other.GetComponent<WeaponSystem>();
        if (ws != null)
        {
            ws.PickupWeapon(weaponData);
            AudioManager.PlaySFX("pickup");
            UIManager.Instance?.ShowTextPopup($"PICKED UP: {weaponData.weaponName}!", transform.position + Vector3.up);
        }

        Destroy(gameObject);
    }
}
