# 🦆 Duck Revolution

> *"One day, ducks gained consciousness and decided humans had oppressed them long enough. The ducks went completely berserk."*

A goofy, chaotic 2D side-scrolling action game built in Unity, inspired by the tone and pacing of **Broforce**. Play as **Battle Duck**, a revolutionary waterfowl leading the charge against escalating human forces — police, SWAT, and eventually the military.

---

## 📋 Table of Contents

1. [Requirements](#requirements)
2. [Project Setup](#project-setup)
3. [Opening the Project](#opening-the-project)
4. [Press Play — Quick Test](#press-play--quick-test)
5. [Controls](#controls)
6. [Gameplay Overview](#gameplay-overview)
7. [Project Structure](#project-structure)
8. [Script Reference](#script-reference)
9. [Customising the Game](#customising-the-game)
10. [Adding Real Art](#adding-real-art)
11. [Known Issues / Notes](#known-issues--notes)

---

## Requirements

| Tool | Version |
|------|---------|
| Unity | **2022.3 LTS** (any 2022.3.x patch) |
| OS   | Windows, macOS, or Linux |
| IDE  | Visual Studio 2022, VS Code, or JetBrains Rider |

> ⚠️ The project targets **Unity 2022.3 LTS**. Using a different version may require package migration. Open the project with Unity Hub and accept any upgrade prompts if prompted.

---

## Project Setup

### Step 1 — Install Unity Hub & Unity 2022.3 LTS

1. Download **Unity Hub** from [https://unity.com/download](https://unity.com/download).
2. In Unity Hub → **Installs** → **Install Editor**.
3. Choose **2022.3.x LTS** (any patch release is fine).
4. Enable **Windows Build Support** (or your target platform).

### Step 2 — Clone This Repository

```bash
git clone https://github.com/danielv27/claude-game.git
cd claude-game
```

---

## Opening the Project

1. Open **Unity Hub**.
2. Click **Open** → **Add project from disk**.
3. Navigate to the cloned `claude-game` folder and click **Add Project**.
4. Unity will import assets and compile scripts — this takes 1-3 minutes the first time.
5. If prompted about a version mismatch, click **Continue**.

---

## Press Play — Quick Test

The game is designed to run **without any manual scene setup**. The `SceneBootstrapper` script auto-generates everything at runtime.

1. In the **Project** panel, navigate to `Assets/Scenes/`.
2. Double-click **MainScene.unity** to open it.
3. Press the **▶ Play** button.

That's it! The game will:
- Generate placeholder pixel-art sprites.
- Build the level with ground, platforms, crates, and barrels.
- Spawn the player (Battle Duck).
- Set up the camera, HUD, and wave system.
- Begin Wave 1 with police enemies after a short delay.

### What You Should See

| HUD Element | Location |
|-------------|----------|
| Health bar  | Top-left |
| Score       | Top-right |
| Wave announcement banner | Centre screen |

---

## Controls

| Action | Key |
|--------|-----|
| Move Left / Right | `A` / `D` or Arrow Keys |
| Jump (double-jump supported) | `Space` |
| Shoot (hold) | `Left Ctrl` or `Left Mouse Button` |
| Throw Egg Grenade | `Right Mouse Button` |
| Quack Ability (stun nearby enemies) | `Q` |

---

## Gameplay Overview

### Wave Progression

| Wave | Enemies | Announcement |
|------|---------|--------------|
| 1 | Police × 4 | "THE DUCK REVOLUTION HAS BEGUN!" |
| 2 | Police × 6 | "BACKUP REQUESTED!" |
| 3 | SWAT × 4 | "SWAT DEPLOYED!" |
| 4 | SWAT × 4 + Police × 3 | "FULL SWAT MOBILIZATION!" |
| 5 | Army × 5 | "THEY BROUGHT THE ARMY?!" |
| 6 | Army × 4 + SWAT × 3 + Police × 2 | "TOTAL WAR DECLARED!" |

Defeat all enemies in a wave to progress. After Wave 6, you win!

### Enemy Types

| Enemy | Health | Speed | Notes |
|-------|--------|-------|-------|
| Police Officer | 40 | Slow | Pistol, long cooldown |
| SWAT Officer | 80 | Medium | Burst fire, flashbangs |
| Army Soldier | 130 | Fast | Heavy burst, occasional airstrike |

### Duck Abilities

| Ability | Description |
|---------|-------------|
| **Shoot** | Rapid feather bullets (hold `Ctrl` / `LMB`) |
| **Egg Grenade** | Bouncing egg with fuse — area explosion |
| **Quack** | Stuns all enemies in radius for 1.5 s (4 s cooldown) |
| **Double Jump** | Jump twice for extra mobility |

### Destructibles

- **Crates** — 30 HP, destroyed by bullets or grenades
- **Barrels** — Can be destroyed for area clearing

---

## Project Structure

```
Assets/
├── Scenes/
│   └── MainScene.unity           ← Open this and press Play
├── Scripts/
│   ├── Game/
│   │   ├── GameManager.cs        ← Singleton; game state, score, win/lose
│   │   ├── HealthSystem.cs       ← Shared health/damage for all characters
│   │   ├── CameraFollow.cs       ← Smooth camera tracking
│   │   ├── DestructibleObject.cs ← Crates, barrels, etc.
│   │   ├── SpriteGenerator.cs    ← Runtime pixel-art placeholder sprites
│   │   └── SceneBootstrapper.cs  ← Auto-assembles the entire scene at runtime
│   ├── Player/
│   │   └── PlayerController.cs   ← Movement, jump, shoot, quack
│   ├── Enemies/
│   │   ├── EnemyBase.cs          ← Abstract AI: patrol → chase → attack
│   │   ├── PoliceEnemy.cs        ← Wave 1-2
│   │   ├── SwatEnemy.cs          ← Wave 3-4 (burst fire + flashbang)
│   │   └── ArmyEnemy.cs          ← Wave 5+ (heavy burst + airstrike)
│   ├── Weapons/
│   │   ├── WeaponSystem.cs       ← Shoot & grenade throw logic
│   │   ├── Bullet.cs             ← Projectile behaviour
│   │   └── EggGrenade.cs         ← Bouncing grenade with fuse + explosion
│   ├── Waves/
│   │   └── WaveManager.cs        ← Spawns waves; fires OnWaveStarted events
│   ├── UI/
│   │   └── UIManager.cs          ← HUD: health, score, wave banners, game-over
│   └── ScriptableObjects/
│       └── EnemyStats.cs         ← SO: enemy config (health, speed, damage…)
ProjectSettings/
│   ├── ProjectVersion.txt        ← Unity 2022.3.20f1
│   ├── TagManager.asset          ← Tags: Player, Enemy, Bullet, Grenade, Ground
│   └── …
Packages/
│   └── manifest.json             ← TextMeshPro, 2D packages, etc.
```

---

## Script Reference

### `GameManager.cs`
Singleton. Manages game state (`Playing`, `Paused`, `GameOver`, `Victory`). Exposes `AddScore()`, `TriggerGameOver()`, `TriggerVictory()`. Fires static events `OnScoreChanged` and `OnGameStateChanged`.

### `HealthSystem.cs`
Attach to any character. Call `TakeDamage(float)` or `Heal(float)`. Fires `onHealthChanged` and `onDeath` UnityEvents. Set `isPlayer = true` to trigger `GameManager.TriggerGameOver()` on death.

### `PlayerController.cs`
Handles input, movement, double-jump, shooting delegation, and quack. Requires `WeaponSystem`, `HealthSystem`, `Rigidbody2D`. Set `groundCheck` to a child Transform below the player's feet.

### `WeaponSystem.cs`
Call `Shoot()` to fire a bullet, `ThrowGrenade()` to lob an egg. Respects `fireRate` and `grenadeCooldown`. Reads sprite flip direction for left/right aim.

### `Bullet.cs`
Auto-moves via `Rigidbody2D.velocity`. Destroys on collision, deals damage via `HealthSystem`. Set `isPlayerBullet` to prevent friendly-fire.

### `EggGrenade.cs`
Fuse countdown blinks red. On explosion, does `Physics2D.OverlapCircleAll` area damage and triggers a screen shake.

### `EnemyBase.cs`
Abstract AI state machine: **Patrol** (wander) → **Chase** (run at player) → **Attack** (shoot). Call `Stun(float duration)` to interrupt. Override `PerformAttack()` and `OnEnemyDeath()` in subclasses.

### `EnemyStats.cs` (ScriptableObject)
Create via `Right-click → Create → DuckRevolution → EnemyStats`. Fields: `maxHealth`, `moveSpeed`, `detectionRange`, `attackRange`, `attackDamage`, `attackCooldown`, `bulletSpeed`, `scoreValue`, `deathQuotes[]`.

### `WaveManager.cs`
Define waves as `Wave[]` arrays in the Inspector (or set via `SceneBootstrapper`). Each wave contains `EnemySpawnEntry[]` (prefab, count, interval). Fires `OnWaveStarted` and `OnWaveCleared` static events.

### `CameraFollow.cs`
Smooth lerp camera that clamps to configurable world bounds. Call `SetTarget(Transform)` at runtime.

### `UIManager.cs`
Subscribes to `GameManager` and `WaveManager` events. Wire `healthSlider`, `scoreText`, `announcementPanel`, etc. in the Inspector (or set via `SceneBootstrapper`).

### `SceneBootstrapper.cs`
**The magic script.** Runs in `Awake()`, creates the entire scene from code (platforms, player, enemies, waves, camera, UI, GameManager). Self-destructs after setup. Assign optional real sprite overrides in the Inspector.

---

## Customising the Game

### Adding a New Enemy Type

1. Create a new script in `Assets/Scripts/Enemies/` extending `EnemyBase`.
2. Override `PerformAttack()` and optionally `OnEnemyDeath()`.
3. In Unity: create an empty prefab, add `Rigidbody2D`, `CapsuleCollider2D`, `SpriteRenderer`, `HealthSystem`, and your new script.
4. Assign an `EnemyStats` ScriptableObject.
5. Add the new prefab to a wave in `WaveManager`.

### Creating an EnemyStats Asset

1. Right-click in `Assets/` → **Create → DuckRevolution → EnemyStats**.
2. Name it (e.g., `HelicopterStats`).
3. Fill in stats in the Inspector.
4. Drag it into your enemy prefab's `Stats` field.

### Adjusting Wave Difficulty

Open `WaveManager` in the Inspector and modify the `Waves` array:
- `predelay` — pause before wave starts
- `count` — number of enemies
- `interval` — seconds between spawns

---

## Adding Real Art

The game uses procedurally generated pixel sprites (see `SpriteGenerator.cs`). To replace them:

1. Import your PNG sprite sheets into `Assets/Sprites/`.
2. Set **Texture Type** → `Sprite (2D and UI)`, **Filter Mode** → `Point (no filter)`, **Pixels Per Unit** → `16`.
3. Select the `Bootstrapper` object in the **Hierarchy**.
4. In the Inspector, assign your sprites to the **Optional Sprite Overrides** fields on `SceneBootstrapper`.

---

## Known Issues / Notes

- **No audio**: Sound effects and music are not implemented. Add an `AudioSource` + `AudioClip` to each prefab and call `Play()` on key events.
- **No save system**: Score resets on play restart.
- **Sorting layers**: The project defines sorting layers (`Background`, `Midground`, `Foreground`, `Default`, `UI`) in `ProjectSettings/TagManager.asset`. If sprites appear in the wrong draw order, assign the correct sorting layer name to your sprite renderers (e.g., `"Background"` for the sky backdrop, `"Foreground"` for characters and projectiles). For parallax scrolling backgrounds, create additional `Background` sub-layers.
- **Enemy pathfinding**: Enemies use simple 1D AI (patrol ↔ chase). They do not navigate around platforms. For platformer-aware AI, implement a 2D raycast ground-check or use Unity's NavMesh with a 2D plugin.
- **Unity version**: If you open with a newer Unity version, accept any API upgrade prompts. The code uses stable APIs that haven't changed significantly.

---

## Credits

Built with ❤️ and quacking fury.

*"QUACK QUACK, HUMANS. THE REVOLUTION IS HERE."*
