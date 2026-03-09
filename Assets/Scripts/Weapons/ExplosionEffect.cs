using UnityEngine;

/// <summary>
/// Simple explosion visual effect - expands and fades out.
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    private float maxRadius;
    private Color color;
    private float duration = 0.4f;
    private float timer;
    private SpriteRenderer sr;

    public void Initialize(float radius, Color explosionColor)
    {
        maxRadius = radius;
        color = explosionColor;
        timer = duration;
        sr = GetComponent<SpriteRenderer>();

        // Start small
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Expand and fade
        float progress = 1f - (timer / duration);
        float scale = maxRadius * 2f * progress;
        transform.localScale = new Vector3(scale, scale, 1f);

        if (sr != null)
        {
            Color c = color;
            c.a = 1f - progress;
            sr.color = c;
        }
    }
}
