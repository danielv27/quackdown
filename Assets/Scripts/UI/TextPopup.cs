using UnityEngine;
using TMPro;

/// <summary>
/// Floating speech-bubble popup that rises and fades out.
/// Renders text inside a white rounded chat-bubble background with a small tail pointer.
/// Used for damage numbers, quips, and announcements.
/// </summary>
public class TextPopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float riseSpeed = 1.4f;
    [SerializeField] private float lifetime = 1.8f;
    [SerializeField] private Color textColor = new Color(0.1f, 0.05f, 0f);
    [SerializeField] private float fontSize = 4.5f;

    [Header("Bubble Appearance")]
    [SerializeField] private Color bubbleColor = new Color(1f, 0.98f, 0.88f, 0.95f);
    [SerializeField] private float bubblePadding = 0.15f;

    private float timer;
    private TextMeshPro tmpText;
    private SpriteRenderer bubbleSr;
    private SpriteRenderer tailSr;

    // Cached sprites so they are only generated once per session
    private static Sprite s_BubbleSprite;
    private static Sprite s_TailSprite;

    /// <summary>Initialize the popup with text content.</summary>
    public void Initialize(string text)
    {
        timer = lifetime;

        EnsureSprites();

        // Random slight horizontal offset after setup for visual variety
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);

        // --- Bubble background ---
        var bubbleObj = new GameObject("Bubble");
        bubbleObj.transform.SetParent(transform, false);
        bubbleObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        bubbleSr = bubbleObj.AddComponent<SpriteRenderer>();
        bubbleSr.sprite = s_BubbleSprite;
        bubbleSr.color = bubbleColor;
        bubbleSr.sortingOrder = 99;
        bubbleSr.drawMode = SpriteDrawMode.Sliced;

        // --- Tail pointer ---
        var tailObj = new GameObject("BubbleTail");
        tailObj.transform.SetParent(transform, false);
        tailObj.transform.localPosition = new Vector3(0f, -0.18f, 0f);
        tailSr = tailObj.AddComponent<SpriteRenderer>();
        tailSr.sprite = s_TailSprite;
        tailSr.color = bubbleColor;
        tailSr.sortingOrder = 99;
        tailSr.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        // --- Text (on top of bubble) ---
        tmpText = gameObject.AddComponent<TextMeshPro>();
        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null) tmpText.font = defaultFont;
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = textColor;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.sortingOrder = 100;
        tmpText.rectTransform.localPosition = new Vector3(0f, 0.15f, 0f);
        tmpText.rectTransform.sizeDelta = new Vector2(4f, 1f);

        // Fit bubble size to text length
        float bubbleW = Mathf.Clamp(text.Length * 0.18f + bubblePadding * 2f, 1.2f, 6f);
        bubbleSr.size = new Vector2(bubbleW, 0.75f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        float alpha = Mathf.Clamp01(timer / lifetime);
        if (tmpText != null) { var c = textColor; c.a = alpha; tmpText.color = c; }
        if (bubbleSr != null) { var c = bubbleColor; c.a = alpha * 0.95f; bubbleSr.color = c; }
        if (tailSr != null)  { var c = bubbleColor; c.a = alpha * 0.95f; tailSr.color = c; }

        // Scale punch in at start
        float scaleBoost = timer > lifetime - 0.12f
            ? Mathf.Lerp(1.3f, 1f, 1f - (timer - (lifetime - 0.12f)) / 0.12f)
            : 1f;
        transform.localScale = Vector3.one * scaleBoost;

        if (timer <= 0f)
            Destroy(gameObject);
    }

    // ── Sprite builders ────────────────────────────────────────────────────────

    private static void EnsureSprites()
    {
        if (s_BubbleSprite == null) s_BubbleSprite = BuildBubbleSprite();
        if (s_TailSprite == null)   s_TailSprite   = BuildTailSprite();
    }

    /// <summary>White rounded-rectangle sprite with a 9-slice border.</summary>
    private static Sprite BuildBubbleSprite()
    {
        const int S = 32, R = 8;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color[S * S];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        // Rounded rectangle fill
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float nx = x, ny = y;
                // Corner circles
                float dist = RoundedRectSDF(nx, ny, S, S, R);
                if (dist <= 0f)
                {
                    float edge = Mathf.Clamp01(-dist);
                    px[y * S + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(edge * 4f + 0.6f));
                }
            }
        }

        // Thin outline
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dist = RoundedRectSDF(x, y, S, S, R);
                if (dist > -1.5f && dist <= 0f)
                    px[y * S + x] = new Color(0.55f, 0.45f, 0.25f, 0.9f);
            }

        tex.SetPixels(px);
        tex.Apply();
        // 9-slice borders at R pixels in
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 32f,
            0, SpriteMeshType.FullRect, new Vector4(R, R, R, R));
    }

    /// <summary>Small downward-pointing triangle sprite for the bubble tail.</summary>
    private static Sprite BuildTailSprite()
    {
        const int W = 14, H = 10;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color[W * H];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;

        for (int y = 0; y < H; y++)
        {
            int halfSpan = (H - y);
            int cx = W / 2;
            for (int x = cx - halfSpan; x <= cx + halfSpan; x++)
                if (x >= 0 && x < W)
                    px[y * W + x] = Color.white;
        }
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 1f), 14f);
    }

    /// <summary>Signed distance to a rounded rectangle (negative = inside).</summary>
    private static float RoundedRectSDF(float x, float y, int w, int h, int r)
    {
        float px = Mathf.Max(0f, Mathf.Abs(x - w * 0.5f) - (w * 0.5f - r));
        float py = Mathf.Max(0f, Mathf.Abs(y - h * 0.5f) - (h * 0.5f - r));
        return Mathf.Sqrt(px * px + py * py) - r;
    }
}
