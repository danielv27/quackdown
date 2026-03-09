using UnityEngine;
using TMPro;

/// <summary>
/// Floating text popup that rises and fades out.
/// Used for damage numbers, funny quips, and announcements.
/// </summary>
public class TextPopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private Color textColor = Color.yellow;
    [SerializeField] private float fontSize = 5f;

    private float timer;
    private TextMeshPro tmpText;

    /// <summary>
    /// Initialize the popup with text content.
    /// </summary>
    public void Initialize(string text)
    {
        timer = lifetime;

        // Create TextMeshPro for world-space text
        tmpText = gameObject.AddComponent<TextMeshPro>();
        // Font must be assigned explicitly — no implicit default in Unity 6
        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null) tmpText.font = defaultFont;
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = textColor;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.sortingOrder = 100;

        // Random slight horizontal offset for visual variety
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Rise upward
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Fade out
        if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = Mathf.Lerp(0f, 1f, timer / lifetime);
            tmpText.color = c;
        }

        // Destroy when done
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
