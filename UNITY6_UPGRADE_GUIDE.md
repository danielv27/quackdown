# Duck Revolution Unity Project Upgrade Analysis
## Unity 2022.3 → Unity 6 (6000.3.10f1)

---

## 1. PROJECT OVERVIEW

**Project Name:** Quackdown (Duck Revolution)
**Current Target:** Unity 2022.3 LTS
**Upgrade Target:** Unity 6 (6000.3.10f1)
**Project Type:** 2D Platformer with Wave-Based Enemy Spawning

### Packages (from Packages/manifest.json):
```json
{
  "com.unity.2d.sprite": "1.0.0",
  "com.unity.2d.tilemap": "1.0.0",
  "com.unity.ide.rider": "3.0.31",
  "com.unity.ide.visualstudio": "2.0.22",
  "com.unity.inputsystem": "1.7.0",
  "com.unity.textmeshpro": "3.0.6",
  "com.unity.timeline": "1.7.6",
  "com.unity.ugui": "1.0.0",
  "com.unity.modules.animation": "1.0.0",
  "com.unity.modules.audio": "1.0.0",
  "com.unity.modules.imgui": "1.0.0",
  "com.unity.modules.physics2d": "1.0.0",
  "com.unity.modules.ui": "1.0.0"
}
```

---

## 2. COMPLETE C# SCRIPT INVENTORY

### Total Files: 18 .cs files

#### **Gameplay Scripts (9 files)**

1. **Assets/Scripts/Player/PlayerController.cs** (167 lines)
   - Handles player movement, jumping, aiming, and quack ability
   - Uses: Rigidbody2D, Physics2D, Input system
   
2. **Assets/Scripts/Game/GameManager.cs** (130 lines)
   - Singleton pattern for game state management
   - Handles score, game over, scene restart
   
3. **Assets/Scripts/Game/HealthSystem.cs** (153 lines)
   - Universal health/damage system with UnityEvents
   - Used by player, enemies, and destructible props
   - Supports healing and event-driven architecture
   
4. **Assets/Scripts/Game/CameraFollow.cs** (104 lines)
   - Smooth camera following with boundary clamping
   - Screen shake effects for impacts/explosions
   
5. **Assets/Scripts/Weapons/WeaponSystem.cs** (86 lines)
   - Projectile shooting and egg grenade throwing
   - Fire rate and cooldown management
   
6. **Assets/Scripts/Weapons/Projectile.cs** (73 lines)
   - 2D projectile with directional movement
   - OnTriggerEnter2D collision handling
   - Damage application and team-based filtering
   
7. **Assets/Scripts/Weapons/ExplosionEffect.cs** (47 lines)
   - Visual explosion animation (expand and fade)
   
8. **Assets/Scripts/Weapons/EggGrenade.cs** (138 lines)
   - Grenade physics with bouncing and timed explosion
   - Area-of-effect damage with falloff
   - Uses Rigidbody2D, Physics2D.OverlapCircleAll
   
9. **Assets/Scripts/Game/DestructibleProp.cs** (103 lines)
   - Breakable objects (crates, barrels)
   - Debris particle generation

#### **Enemy Scripts (5 files)**

10. **Assets/Scripts/Enemies/EnemyBase.cs** (294 lines)
    - Base class for all enemies
    - State machine (Patrol, Chase, Attack, Stunned, Dead)
    - Ground detection and movement physics
    - Projectile shooting logic

11. **Assets/Scripts/Enemies/EnemyData.cs** (32 lines)
    - ScriptableObject for configurable enemy stats
    
12. **Assets/Scripts/Enemies/PoliceEnemy.cs** (30 lines)
    - Basic enemy variant with humorous dialog

13. **Assets/Scripts/Enemies/SwatEnemy.cs** (60 lines)
    - Burst-fire attack pattern variation

14. **Assets/Scripts/Enemies/ArmyEnemy.cs** (70 lines)
    - Grenade-throwing enemy variant

#### **UI Scripts (2 files)**

15. **Assets/Scripts/UI/UIManager.cs** (150 lines)
    - Singleton managing all UI elements
    - Score, health bar, wave announcements, popups
    - Uses legacy UnityEngine.UI.Text

16. **Assets/Scripts/UI/TextPopup.cs** (67 lines)
    - Floating text animation with TextMesh (legacy)

#### **Management Scripts (2 files)**

17. **Assets/Scripts/Waves/WaveManager.cs** (234 lines)
    - Enemy spawn wave system
    - Progressive difficulty scaling
    - Wave announcements and coordination

18. **Assets/Editor/GameSetupEditor.cs** (908 lines)
    - Editor utility for auto-setup
    - Sprite generation, prefab creation, scene building
    - Uses: EditorUtility, PrefabUtility, AssetDatabase, SerializedObject

### No .asmdef files found in project

---

## 3. CRITICAL API CHANGES REQUIRED FOR UNITY 6

### **HIGH PRIORITY - Breaking Changes**

#### A. Rigidbody2D Velocity Changes
**Affected Files:** PlayerController.cs, EnemyBase.cs, WeaponSystem.cs, EggGrenade.cs, Projectile.cs (5 files)

**Change Required:** `rb.velocity` → `rb.linearVelocity`

**Occurrences:**
- **PlayerController.cs**
  - Line 65: `rb.velocity = new Vector2(rb.velocity.x, jumpForce);` 
  - Line 94: `rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);`
  
- **EnemyBase.cs**
  - Line 126: `rb.velocity = new Vector2(dir * speed, rb.velocity.y);` (Patrol)
  - Line 155: `rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);` (Chase)
  - Line 164: `rb.velocity = new Vector2(0f, rb.velocity.y);` (Attack)
  - Line 218: `rb.velocity = Vector2.zero;` (Stun)

**Fix Template:**
```csharp
// BEFORE (2022.3)
rb.velocity = new Vector2(rb.velocity.x, jumpForce);

// AFTER (Unity 6)
rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
```

---

#### B. Legacy UI Text API
**Affected Files:** UIManager.cs, TextPopup.cs, GameSetupEditor.cs (3 files)

**Changes Required:**
1. `using UnityEngine.UI;` components are **NOT deprecated** but legacy UI (Text) still exists
2. TextMesh (line-renderer based) is **deprecated in favor of TextMeshPro**
3. `UnityEngine.UI.Text` is the **legacy Text** component

**Occurrences:**
- **UIManager.cs**
  - Line 2: `using UnityEngine.UI;`
  - Lines 13-25: Text fields using legacy UI.Text
  - Line 63-64: `scoreText.text = "SCORE: " + score;`

- **TextPopup.cs**
  - Lines 25-31: Uses deprecated `TextMesh` (world-space text rendering)
  - Line 16: `private TextMesh textMesh;`
  - Should use **TextMeshPro** instead

- **GameSetupEditor.cs**
  - Lines 814-821: References to UI.Text components
  - Line 879-882: Font loading with deprecated methods

**Fix Strategy:**
```csharp
// OPTION 1: Keep legacy UI (works in 6 but deprecated)
// No code changes needed, but performance warning

// OPTION 2: Migrate to TextMeshPro (RECOMMENDED)
using TMPro;

// BEFORE
private Text scoreText;
scoreText.text = "SCORE: " + score;

// AFTER
private TextMeshProUGUI scoreText;
scoreText.text = "SCORE: " + score;
```

---

#### C. TextMesh (World Space) Deprecation
**Affected File:** TextPopup.cs

**Issue:** Line 26 uses deprecated `TextMesh` component
```csharp
textMesh = gameObject.AddComponent<TextMesh>();
textMesh.text = text;
textMesh.characterSize = fontSize;
```

**Fix:** Replace with TextMeshPro 3D or UI canvas approach
```csharp
// OPTION: Use TextMeshPro 3D
using TMPro;
private TextMeshPro textMeshPro;
textMeshPro = gameObject.AddComponent<TextMeshPro>();
```

---

#### D. Physics2D/Collider2D Stability
**Affected Files:** PlayerController.cs, EnemyBase.cs, Projectile.cs, EggGrenade.cs (4 files)

**Current Usage (Safe):**
- `Physics2D.OverlapCircle()` - Line 59 (PlayerController), Line 86 (EnemyBase), etc.
- `Physics2D.OverlapCircleAll()` - Line 141 (PlayerController), Line 64 (EggGrenade)
- `OnTriggerEnter2D()` - Line 47 (Projectile)
- CollisionDetectionMode2D.Continuous - Line 415 (GameSetupEditor)

**Status:** ✅ **NO CHANGES REQUIRED** - These APIs remain compatible in Unity 6

---

### **MEDIUM PRIORITY - Deprecation Warnings**

#### A. Camera.main (Potential Performance Issue)
**Affected Files:** PlayerController.cs (line 119), CameraFollow.cs (line 34), WaveManager.cs (line 162)

**Status:** Still functional but cached lookups recommended

**Current Usage:**
```csharp
Camera mainCam = Camera.main;  // Acceptable, but consider caching
```

**Recommendation:** Already cached appropriately in CameraFollow.cs

---

#### B. Input.GetAxis/GetButton (Legacy Input System)
**Affected File:** PlayerController.cs

**Current Usage:**
```csharp
horizontalInput = Input.GetAxisRaw("Horizontal");      // Line 47
if (Input.GetButtonDown("Jump") && isGrounded)        // Line 63
if (Input.GetButtonDown("Fire1"))                     // Line 69
if (Input.GetButtonDown("Fire2"))                     // Line 76
if (Input.GetKeyDown(KeyCode.Q))                      // Line 84
if (!gameActive && Input.GetKeyDown(KeyCode.R))       // GameManager line 40
```

**Status:** ✅ Still supported but manifest includes `com.unity.inputsystem` v1.7.0

**Recommendation:** Migration to Input System is optional but future-proof

---

#### C. Resources.GetBuiltinResource (Font Loading)
**Affected File:** GameSetupEditor.cs, lines 879-882

**Current Usage:**
```csharp
Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
if (font == null)
    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
```

**Issue:** Paths may vary in Unity 6

**Fix:**
```csharp
// OPTION 1: Use TMP_FontAsset instead
using TMPro;
TMP_FontAsset tmpFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

// OPTION 2: Load from project
Font font = Resources.Load<Font>("Fonts/Arial");
```

---

### **LOW PRIORITY - Compatibility Notes**

#### A. SceneManager.LoadScene (Already Correct)
**File:** GameManager.cs, line 117-119

```csharp
UnityEngine.SceneManagement.SceneManager.LoadScene(
    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
);
```
✅ **No changes needed** - Fully qualified and compatible

---

#### B. Rigidbody2D Physics Material
**File:** GameSetupEditor.cs, line 421-424

```csharp
PhysicsMaterial2D bounceMat = new PhysicsMaterial2D("EggBounce");
bounceMat.bounciness = 0.5f;
bounceMat.friction = 0.3f;
```
✅ **Compatible** - No API changes

---

#### C. SerializedObject Usage
**File:** GameSetupEditor.cs (extensively)

Lines 357-364, 468-485, 533-539, etc.
✅ **Fully compatible** - Editor serialization stable across versions

---

## 4. DETAILED MIGRATION CHECKLIST

### Phase 1: Velocity Changes (CRITICAL)
- [ ] PlayerController.cs
  - [ ] Line 65: `rb.velocity.x` → `rb.linearVelocity.x`
  - [ ] Line 65: `rb.velocity.y` → `rb.linearVelocity.y`
  - [ ] Line 94: Both instances
  
- [ ] EnemyBase.cs (4 occurrences)
  - [ ] Line 126 (Patrol)
  - [ ] Line 155 (Chase)
  - [ ] Line 164 (Attack)
  - [ ] Line 218 (Stun)

### Phase 2: UI Migration (IMPORTANT)
- [ ] Decide: Keep legacy UI or migrate to TextMeshPro
- [ ] If TextMeshPro:
  - [ ] Add `using TMPro;` to UIManager.cs
  - [ ] Change `Text` → `TextMeshProUGUI` declarations
  - [ ] Update GameSetupEditor.cs UI creation code
  
- [ ] Replace TextMesh in TextPopup.cs with TextMeshPro 3D

### Phase 3: Font Loading (OPTIONAL)
- [ ] GameSetupEditor.cs font loading
  - [ ] Update Resources.GetBuiltinResource calls
  - [ ] Or switch to TextMeshPro font assets

### Phase 4: Input System (OPTIONAL)
- [ ] Update PlayerController.cs to use new Input System if desired
- [ ] Or keep legacy (still supported)

---

## 5. SAFE-TO-IGNORE ITEMS

✅ **These will work in Unity 6 without changes:**

- Physics2D.OverlapCircle / OverlapCircleAll
- OnTriggerEnter2D / Collision2D callbacks
- Rigidbody2D.AddForce / AddTorque
- BoxCollider2D / CircleCollider2D
- SpriteRenderer
- Instantiate / Destroy
- Vector2 / Vector3 math
- Mathf utilities
- Coroutines
- Tags and Layers
- SceneManager
- PrefabUtility (Editor)
- EditorUtility
- AssetDatabase
- Singleton pattern

---

## 6. SUMMARY OF REQUIRED CHANGES

| Category | Severity | Files | Required Changes |
|----------|----------|-------|------------------|
| Velocity API | **CRITICAL** | 5 | ~8 instances of `rb.velocity` → `rb.linearVelocity` |
| UI Text | **HIGH** | 3 | Migrate legacy UI.Text to TextMeshPro (or keep with warning) |
| TextMesh | **HIGH** | 1 | Replace TextMesh with TextMeshPro 3D |
| Font Loading | MEDIUM | 1 | Update Resources paths or use TMP fonts |
| Input System | LOW | 1 | Optional modernization |
| Physics2D | LOW | 4 | No changes (fully compatible) |

**Estimated Migration Time:** 1-2 hours for complete upgrade

---

## 7. NO ASSEMBLY DEFINITION FILES

Project does not use .asmdef files. All scripts compile into default assemblies:
- `Assembly-CSharp.csproj` (game code)
- `Assembly-CSharp-Editor.csproj` (editor code)

For future projects, consider using .asmdef for faster iteration.

