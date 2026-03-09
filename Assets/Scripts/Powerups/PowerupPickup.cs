using System.Collections;
using UnityEngine;

/// <summary>
/// Floating powerup pickup. Spawned by enemies on death or placed in the world.
/// When the player overlaps it, applies the powerup effect and destroys itself.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerupPickup : MonoBehaviour
{
    [SerializeField] private PowerupData data;

    private Vector3 startPos;
    private SpriteRenderer sr;
    private bool collected;

    private void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();

        if (data != null && sr != null && data.sprite != null)
            sr.sprite = data.sprite;

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void SetData(PowerupData powerupData)
    {
        data = powerupData;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.sprite != null) sr.sprite = data.sprite;
    }

    private void Update()
    {
        if (data == null) return;

        // Float up and down
        float y = startPos.y + Mathf.Sin(Time.time * data.floatSpeed) * data.floatAmplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        // Pulsing glow color
        if (sr != null)
        {
            float pulse = 0.65f + Mathf.Sin(Time.time * 4f) * 0.35f;
            sr.color = Color.Lerp(Color.white, data.glowColor, pulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        ApplyEffect(other.gameObject);
    }

    private void ApplyEffect(GameObject player)
    {
        if (data == null) return;

        switch (data.powerupType)
        {
            case PowerupType.Health:
                player.GetComponent<HealthSystem>()?.Heal(data.magnitude);
                break;

            case PowerupType.Speed:
                player.GetComponent<PlayerController>()?.ApplySpeedBoost(data.magnitude, data.duration);
                break;

            case PowerupType.Damage:
                player.GetComponent<WeaponSystem>()?.ApplyDamageBoost(data.magnitude, data.duration);
                break;

            case PowerupType.Shield:
                player.GetComponent<HealthSystem>()?.ApplyShield(data.magnitude);
                break;

            case PowerupType.RapidFire:
                player.GetComponent<WeaponSystem>()?.ApplyRapidFire(data.duration);
                break;

            case PowerupType.BigEgg:
                player.GetComponent<WeaponSystem>()?.ApplyBigEgg(data.duration);
                break;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextPopup(data.popupText, transform.position + Vector3.up);

        AudioManager.PlaySFX("pickup");
        Destroy(gameObject);
    }
}
