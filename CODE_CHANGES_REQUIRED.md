# Unity 6 Code Migration - Detailed Changes Required

## CRITICAL CHANGES (Must Do)

### 1. Rigidbody2D.velocity → rb.linearVelocity

This is the most important change for Unity 6. The `velocity` property was renamed to `linearVelocity`.

#### File: Assets/Scripts/Player/PlayerController.cs

**Line 65 (Jump application):**
```csharp
// BEFORE
rb.velocity = new Vector2(rb.velocity.x, jumpForce);

// AFTER
rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
```

**Line 94 (Movement in FixedUpdate):**
```csharp
// BEFORE
rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

// AFTER
rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
```

---

#### File: Assets/Scripts/Enemies/EnemyBase.cs

**Line 126 (Patrol movement):**
```csharp
// BEFORE
rb.velocity = new Vector2(dir * speed, rb.velocity.y);

// AFTER
rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
```

**Line 155 (Chase movement):**
```csharp
// BEFORE
rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);

// AFTER
rb.linearVelocity = new Vector2(moveDir * speed, rb.linearVelocity.y);
```

**Line 164 (Attack state - stop movement):**
```csharp
// BEFORE
rb.velocity = new Vector2(0f, rb.velocity.y);

// AFTER
rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
```

**Line 218 (Stun - freeze movement):**
```csharp
// BEFORE
rb.velocity = Vector2.zero;

// AFTER
rb.linearVelocity = Vector2.zero;
```

---

### 2. Legacy UI Text → TextMeshPro

The old `UnityEngine.UI.Text` component still works but is deprecated. **Recommendation: Migrate to TextMeshPro for better performance and quality.**

#### File: Assets/Scripts/UI/UIManager.cs

**Add this using statement at the top:**
```csharp
// ADD THIS
using TMPro;

// KEEP THIS (can be removed if not needed elsewhere)
using UnityEngine.UI;
```

**Lines 13-25 (Field declarations):**
```csharp
// BEFORE
[SerializeField] private Text scoreText;
[SerializeField] private Text waveText;
[SerializeField] private Text healthText;
[SerializeField] private Image healthBar;
[SerializeField] private GameObject gameOverPanel;
[SerializeField] private Text gameOverScoreText;

// ... later ...

[SerializeField] private Text announcementText;

// AFTER
[SerializeField] private TextMeshProUGUI scoreText;
[SerializeField] private TextMeshProUGUI waveText;
[SerializeField] private TextMeshProUGUI healthText;
[SerializeField] private Image healthBar;  // Keep this - Image is still current
[SerializeField] private GameObject gameOverPanel;
[SerializeField] private TextMeshProUGUI gameOverScoreText;

// ... later ...

[SerializeField] private TextMeshProUGUI announcementText;
```

**The text assignment code (lines 61-65) needs NO changes:**
```csharp
// This works with both Text and TextMeshProUGUI
if (scoreText != null)
    scoreText.text = "SCORE: " + score;
```

**All other usages of `.text` are compatible!** TextMeshPro has the same text property.

---

#### File: Assets/Scripts/UI/TextPopup.cs

**Complete rewrite of field and initialization:**

```csharp
// BEFORE - Using deprecated TextMesh (world space text renderer)
using UnityEngine;

public class TextPopup : MonoBehaviour
{
    // ... fields ...
    private TextMesh textMesh;
    
    public void Initialize(string text)
    {
        timer = lifetime;
        
        // DEPRECATED - TextMesh is old world-space text
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = fontSize;
        // ... etc ...
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        
        // Fade out
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = Mathf.Lerp(0f, 1f, timer / lifetime);
            textMesh.color = c;
        }
        // ...
    }
}


// AFTER - Using TextMeshPro 3D (recommended)
using UnityEngine;
using TMPro;

public class TextPopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private Color textColor = Color.yellow;
    [SerializeField] private float fontSize = 36f;  // TextMeshPro uses different scale

    private float timer;
    private TextMeshPro textMeshPro;  // CHANGED

    public void Initialize(string text)
    {
        timer = lifetime;

        // Use TextMeshPro 3D instead of deprecated TextMesh
        textMeshPro = gameObject.AddComponent<TextMeshPro>();
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = textColor;

        // Get renderer and set sorting layer
        TextMeshProUGUI renderer = gameObject.GetComponent<TextMeshProUGUI>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 100;
        }

        // Random slight horizontal offset for visual variety
        transform.position += new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Rise upward
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Fade out
        if (textMeshPro != null)
        {
            Color c = textMeshPro.color;
            c.a = Mathf.Lerp(0f, 1f, timer / lifetime);
            textMeshPro.color = c;
        }

        // Destroy when done
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
```

---

#### File: Assets/Scripts/Editor/GameSetupEditor.cs

**Line 814-821 (UI Creation - Font references):**

```csharp
// BEFORE
uiSO.FindProperty("scoreText").objectReferenceValue = scoreObj.GetComponent<UnityEngine.UI.Text>();
uiSO.FindProperty("waveText").objectReferenceValue = waveObj.GetComponent<UnityEngine.UI.Text>();
uiSO.FindProperty("healthText").objectReferenceValue = healthObj.GetComponent<UnityEngine.UI.Text>();
// ... etc

// AFTER
using TMPro;

uiSO.FindProperty("scoreText").objectReferenceValue = scoreObj.GetComponent<TextMeshProUGUI>();
uiSO.FindProperty("waveText").objectReferenceValue = waveObj.GetComponent<TextMeshProUGUI>();
uiSO.FindProperty("healthText").objectReferenceValue = healthObj.GetComponent<TextMeshProUGUI>();
// ... etc
```

**Lines 839-891 (CreateUIText function - FULL REPLACEMENT):**

```csharp
// BEFORE - Creating legacy UI.Text
private static GameObject CreateUIText(string name, Transform parent, Vector2 position, 
    Vector2 size, string text, TextAnchor anchor, int fontSize, Color color)
{
    GameObject obj = new GameObject(name);
    obj.transform.SetParent(parent, false);

    RectTransform rect = obj.AddComponent<RectTransform>();
    
    // ... anchor setup ...

    UnityEngine.UI.Text uiText = obj.AddComponent<UnityEngine.UI.Text>();
    uiText.text = text;
    uiText.fontSize = fontSize;
    uiText.color = color;
    uiText.alignment = anchor;

    // Try to load built-in font
    Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    if (font == null)
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    uiText.font = font;

    uiText.horizontalOverflow = HorizontalWrapMode.Overflow;

    UnityEngine.UI.Outline outline = obj.AddComponent<UnityEngine.UI.Outline>();
    outline.effectColor = Color.black;
    outline.effectDistance = new Vector2(1, -1);

    return obj;
}


// AFTER - Creating TextMeshProUGUI
private static GameObject CreateUIText(string name, Transform parent, Vector2 position, 
    Vector2 size, string text, TextAnchor anchor, int fontSize, Color color)
{
    using TMPro;
    
    GameObject obj = new GameObject(name);
    obj.transform.SetParent(parent, false);

    RectTransform rect = obj.AddComponent<RectTransform>();
    
    // ... same anchor setup code ...

    TextMeshProUGUI tmpText = obj.AddComponent<TextMeshProUGUI>();
    tmpText.text = text;
    tmpText.fontSize = fontSize;
    tmpText.color = color;
    
    // TextAlignmentOptions has different values than TextAnchor
    // Convert TextAnchor to TextAlignmentOptions
    tmpText.alignment = ConvertTextAnchor(anchor);
    
    // TextMeshPro uses a default font automatically
    // No need to load fonts manually

    tmpText.horizontalWrapping = false;  // Renamed property

    // Outline still exists but has different class
    UnityEngine.UI.Outline outline = obj.AddComponent<UnityEngine.UI.Outline>();
    outline.effectColor = Color.black;
    outline.effectDistance = new Vector2(1, -1);

    return obj;
}

// Helper function to convert TextAnchor to TextAlignmentOptions
private static TextAlignmentOptions ConvertTextAnchor(TextAnchor anchor)
{
    return anchor switch
    {
        TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
        TextAnchor.UpperCenter => TextAlignmentOptions.Top,
        TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
        TextAnchor.MiddleLeft => TextAlignmentOptions.MidlineLeft,
        TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
        TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
        TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
        TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
        TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
        _ => TextAlignmentOptions.Center
    };
}
```

---

## OPTIONAL/LOW-PRIORITY CHANGES

### 3. Font Resource Loading (Optional - Only if Migration Issues Occur)

**File:** Assets/Scripts/Editor/GameSetupEditor.cs (lines 879-882)

```csharp
// BEFORE - May not work in Unity 6
Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
if (font == null)
    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
uiText.font = font;

// OPTION A - Just remove (TextMeshPro uses default)
// No font setting needed

// OPTION B - Load from Assets if migrating
TMP_FontAsset tmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
tmpText.font = tmpFont;
```

---

### 4. Input System (Optional Modernization)

**File:** Assets/Scripts/Player/PlayerController.cs

The current code uses legacy Input Manager. This still works in Unity 6 but project already has `com.unity.inputsystem` v1.7.0 available.

```csharp
// CURRENT (Works but old)
horizontalInput = Input.GetAxisRaw("Horizontal");
if (Input.GetButtonDown("Jump") && isGrounded) { ... }

// MODERN (Optional)
using UnityEngine.InputSystem;

// In a different method:
playerInput = GetComponent<PlayerInput>();
// ... and restructure input handling

// This is a much larger refactoring - only do if you want to modernize
```

---

## TESTING CHECKLIST AFTER MIGRATION

- [ ] Game runs without compilation errors
- [ ] Player movement works (WASD/Arrows)
- [ ] Jump applies correct force
- [ ] Camera follows player smoothly
- [ ] Enemies spawn and move correctly in patrol
- [ ] Enemies chase player
- [ ] Enemies stop moving while attacking (line 164)
- [ ] Enemies stun and freeze (line 218)
- [ ] UI displays score, health, wave announcements
- [ ] Text popups appear and fade properly
- [ ] Explosions work and apply knockback
- [ ] No velocity-related bugs (falling through floors, weird jumping, etc.)

---

## SUMMARY

**Total Changes Required:**
- 5 instances of `rb.velocity` → `rb.linearVelocity`
- 1 complete class rewrite (TextPopup.cs)
- 4 field declarations in UIManager.cs
- 1 function rewrite (CreateUIText in GameSetupEditor.cs)
- 1 helper function addition (ConvertTextAnchor)

**Estimated Time:** 30-60 minutes (mostly testing after changes)

**Breaking Changes That Will Cause Compilation Errors:**
- Using `rb.velocity` - compiler will error
- Assigning to missing `Text` properties in serialization

**Breaking Changes That Will Cause Runtime Issues:**
- None - if velocity isn't changed, game will compile but behave oddly
