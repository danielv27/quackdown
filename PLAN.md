# 🦆 QUACKDOWN — Development Plan

> **From bare prototype to polished vertical slice.**
> A practical, impact-first roadmap for a solo indie developer.

---

## 1. PROJECT VISION

The finished prototype should feel like **the first level of a game you can't stop playing**. The player picks up the controller, immediately understands they're a gun-toting revolutionary duck, and within 30 seconds they're laughing — screen shaking, feathers flying, cops ragdolling, eggs exploding, and a massive "QUAAAACK" stunning everything on screen.

It should feel like a **playable trailer**: fast, funny, and full of personality. Every action should feel punchy. Every kill should feel satisfying. The chaos should build until the player is overwhelmed and thinks "one more run."

**Reference feel:** Broforce's chaos × Metal Slug's charm × Hotline Miami's snappiness — but starring a duck.

---

## 2. CORE GAME LOOP

```
SPAWN → FIGHT → SURVIVE → ESCALATE → DIE → RESTART
```

1. **Duck spawns** in an arena with light props and platforms
2. **Enemies arrive in waves** — police first, then SWAT, then military
3. **Player fights** using feather shots, egg grenades, and the QUACK stun
4. **Destruction accumulates** — props explode, debris flies, the arena gets wrecked
5. **Waves escalate** — more enemies, tougher types, new threats
6. **Player is overwhelmed** and dies in a blaze of glory
7. **Score screen** shows stats — kills, wave reached, destruction caused
8. **Restart is instant** — "just one more run"

**The loop works when** every run feels slightly different and the player always feels like they *almost* made it to the next wave.

---

## 3. HIGH IMPACT IMPROVEMENTS

These are the changes that will transform the game from "tech demo" to "fun prototype." Ranked by impact-per-effort:

### Tier 1 — Immediate Transformation (each one makes the game noticeably better)

| # | Improvement | Why It Matters |
|---|---|---|
| 1 | **Screen shake on every hit/kill/explosion** | Nothing sells impact like camera shake. The system exists but is underused. |
| 2 | **Hit stop / hit freeze (20-40ms pause on kill)** | Makes every kill feel weighty. Costs almost nothing to implement. |
| 3 | **Particle effects on hit, death, explosion** | Replace invisible damage with visible feathers, sparks, and debris flying everywhere. |
| 4 | **Muzzle flash + recoil animation** | The gun should *feel* like a gun. Even a sprite scale punch (1.1x → 1.0x) helps. |
| 5 | **Enemy knockback on hit** | Enemies should react to being shot. Pushes them back slightly on damage. |
| 6 | **Faster restart** | Death → playing again in under 1 second. Remove all friction from the retry loop. |

### Tier 2 — Identity and Depth

| # | Improvement | Why It Matters |
|---|---|---|
| 7 | **Sound effects** (shoot, hit, explode, quack, death, wave start) | Audio is 50% of game feel. Even placeholder sounds dramatically improve the experience. |
| 8 | **Multiple weapons** (shotgun spread, rapid fire, explosive eggs) | Variety is what makes each run interesting. |
| 9 | **Weapon pickups from killed enemies** | Creates moment-to-moment decisions and rewards aggression. |
| 10 | **Better death sequence** — slow-mo + explosion + stat screen | The death should feel dramatic, not abrupt. |
| 11 | **Visual enemy differentiation** — size, silhouette, color coding | Players should instantly know what they're fighting at a glance. |

### Tier 3 — Depth and Replayability

| # | Improvement | Why It Matters |
|---|---|---|
| 12 | **Powerup drops** (speed boost, damage up, health, shield) | Random powerups create emergent moments and keep runs fresh. |
| 13 | **Combo system** — kill streak counter with escalating rewards | Rewards skilled play and creates "in the zone" moments. |
| 14 | **Environmental hazards** — explosive barrels chain-react | Lets players use the environment strategically. |
| 15 | **Boss enemy at wave 10** | Gives players a clear goal to aim for. |

---

## 4. DEVELOPMENT ROADMAP

### Phase 1 — Game Feel ("Make It Feel Good")

> **Goal:** Every action should feel punchy and satisfying. The player should *feel* powerful.

#### Features to Implement

**1.1 — Juice System (Central FX Manager)**
- Create a `JuiceManager` singleton that centralizes all game feel effects
- Screen shake: parameterized by intensity (light hit → big explosion)
- Hit stop: freeze game for 1-3 frames on significant hits (use `Time.timeScale` briefly or pause physics)
- Slow-motion: brief 0.1s slow-mo on multi-kills or grenade explosions

**1.2 — Hit Feedback**
- Enemy flash white (not just red) on hit — white reads better as "damage"
- Enemy knockback: apply small force in hit direction on `TakeDamage()`
- Damage numbers that scale with damage amount (big numbers = big hit)
- Blood/feather particles burst from hit point (use Unity's built-in `ParticleSystem`)

**1.3 — Weapon Feel**
- Muzzle flash sprite (enable for 1 frame at fire point on shoot)
- Weapon recoil: brief sprite offset/rotation that snaps back
- Shell casings / feather eject particles
- Slight camera push in shoot direction

**1.4 — Movement Feel**
- Dust particles on landing
- Speed lines / motion trail when moving fast
- Squash & stretch on jump (scale Y up on launch, X on land)
- Coyote time: allow jump for ~0.1s after leaving a platform edge
- Jump buffering: register jump input ~0.1s before landing

**1.5 — Death & Destruction Feel**
- Enemies burst into pieces on death (extend existing debris system)
- Props chain-explode when near explosions
- Slow-motion on player death (0.3s at 0.2x time scale)
- Camera zoom on player death
- Screen flash on big explosions

**Why this matters:** Game feel is the single biggest differentiator between "amateur prototype" and "this feels like a real game." Players will forgive ugly art if the game *feels* incredible.

---

### Phase 2 — Gameplay Expansion ("Make It Interesting")

> **Goal:** Give the player meaningful choices and varied moment-to-moment gameplay.

#### Features to Implement

**2.1 — Modular Weapon System**
- Create a `WeaponData` ScriptableObject (mirrors the `EnemyData` pattern):
  ```
  weaponName, sprite, projectileSprite, damage, fireRate, 
  projectileSpeed, projectileCount, spreadAngle, 
  isAutomatic, ammoCount (-1 = infinite), soundEffect
  ```
- Weapon types to implement:
  - **Feather Pistol** (default) — single shot, reliable, infinite ammo
  - **Feather Shotgun** — 5 projectiles in spread, slow fire rate, high burst damage
  - **Quack-47** — full auto rapid fire, lower damage per shot
  - **Egg Launcher** — lobs explosive eggs rapidly
  - **Golden Feather** — single shot, massive damage, pierces enemies
- Weapons drop from killed enemies (random chance ~15%)
- Picked-up weapons have limited ammo, then revert to pistol
- Each weapon should *feel* different (different screen shake, different sound, different fire rate)

**2.2 — Duck Abilities**
- **Wing Dash** — short invincible dash in movement direction (double-tap or Shift)
  - Brief invincibility frames
  - Afterimage trail effect
  - Short cooldown (1.5s)
- **Ground Pound** — while airborne, slam down with area damage
  - Hold S/Down while airborne
  - Creates shockwave on landing
  - Damages and knocks back nearby enemies
  - Extra screen shake
- **QUACK improvement** — the existing stun also:
  - Pushes enemies back (knockback force)
  - Deflects nearby projectiles
  - Breaks destructible props in radius
  - Has a visible shockwave ring effect

**2.3 — Combo & Score System**
- Kill streak counter: resets after 3 seconds without a kill
- Combo multiplier increases score per kill: x2, x3, x4, etc.
- Visual feedback: combo counter displays prominently on screen
- At high combos (x5+): screen gets slight vignette, music intensifies (when audio exists)
- Bonus points for: multi-kills, environmental kills, grenade kills, melee range kills

**2.4 — Powerup System**
- Create `PowerupData` ScriptableObject:
  ```
  powerupName, sprite, duration, effect type, magnitude
  ```
- Powerups drop from random enemies or destroyed props
- Types:
  - **Health Egg** — restores 25 HP
  - **Speed Seed** — 50% move speed for 8s
  - **Damage Corn** — 2x damage for 8s
  - **Shield Feather** — absorbs next 50 damage
  - **Rapid Fire** — 2x fire rate for 6s
  - **Big Egg Mode** — grenades are 2x radius for 10s
- Visual indicator on player when active (colored glow/outline)
- Powerups float and bob with a subtle glow

**Why this matters:** Variety creates replayability. Each run should feel different because the weapons and powerups available change. Player skill expression deepens with dash and ground pound.

---

### Phase 3 — Visual Identity ("Make It Look Good")

> **Goal:** Replace placeholder art with a cohesive visual style that's achievable for a solo developer.

#### Art Direction

**Recommended style: Simple pixel art with strong silhouettes and expressive particles.**

- **Resolution:** 32x32 character sprites (current), 16x16 for small objects
- **Color palette:** Limited palette (12-16 colors). Use a published palette like [PICO-8](https://lospec.com/palette-list/pico-8) or [Endesga 32](https://lospec.com/palette-list/endesga-32) for cohesion
- **Characters:** Exaggerate proportions — big head, small body, big weapon. Ducks should be cute but dangerous
- **Enemies:** Each type needs a distinct silhouette even at small size:
  - Police: medium, blue uniform, cap
  - SWAT: bulky, dark, helmet + visor
  - Army: tall, green, helmet
- **Environment:** Simple geometric shapes with bold outlines

#### Features to Implement

**3.1 — Sprite Upgrades**
- Redraw duck sprite with animation frames:
  - Idle (2 frames, gentle bob)
  - Run (4-6 frames)
  - Jump (up + falling poses)
  - Shoot (recoil frame)
  - Death (dramatic)
- Redraw enemy sprites with idle + walk + attack + death frames
- Weapon sprites for each weapon type
- Powerup sprites with consistent visual language

**3.2 — Particle System Overhaul**
- Use Unity's `ParticleSystem` instead of spawning GameObjects for debris
- Particle presets:
  - **Hit sparks** — small yellow/white burst, 5-10 particles
  - **Blood/feather burst** — directional, 8-15 particles (enemy-colored)
  - **Explosion** — ring burst + smoke, 20-30 particles
  - **Dust** — small puff on landing/running, 3-5 particles
  - **Muzzle flash** — 1-frame bright sprite
  - **Shell casings** — tiny arcing physics particles
- Use Unity's built-in `ObjectPool<T>` for particle system pooling (see Unity 6 `UnityEngine.Pool` namespace)

**3.3 — Background & Environment**
- Parallax scrolling background (2-3 layers):
  - Far: city skyline silhouette
  - Mid: buildings with windows
  - Near: fence/railing (optional)
- Ground tiles with variation (grass tufts, cracks, debris)
- Platform visual variety (concrete, wood, metal)

**3.4 — UI Polish**
- Animated score counter (numbers roll up, not jump)
- Wave announcement with slide-in animation
- Health bar with color gradient (green → yellow → red)
- Combo counter with scale-up animation on increment
- Screen-edge damage vignette when low health

**3.5 — Post-Processing (Quick Wins with URP)**
- If not already on URP, consider switching for access to:
  - Bloom (makes explosions and muzzle flashes glow)
  - Chromatic aberration on damage
  - Vignette on low health
- These are configuration changes, not code — very high impact for minimal effort

**Why this matters:** Visuals create first impressions. The game doesn't need AAA art — it needs *consistent, readable, expressive* art. Particles and post-processing do most of the heavy lifting.

---

### Phase 4 — Content & Variety ("Make It Surprising")

> **Goal:** Add enough variety that each run feels fresh and the game doesn't become repetitive.

#### Features to Implement

**4.1 — New Enemy Types**
Each new enemy type should introduce a new threat the player must react to differently:

| Enemy | Behavior | Threat | Visual |
|---|---|---|---|
| **Riot Shield Cop** | Walks forward with shield, blocks frontal shots | Forces flanking or grenades | Blue + large shield sprite |
| **Sniper** | Stays at max range, laser sight, high damage single shot | Punishes standing still | Prone/kneeling, red laser line |
| **Drone** | Flies, drops bombs, ignores terrain | Aerial threat, forces looking up | Small helicopter sprite |
| **K9 Unit** | Fast, melee-only, runs at player | Pressure, forces spacing | Dog sprite, very fast |
| **Tank** (mini-boss) | Slow, huge health, fires explosive shells | Wave milestone threat | Large, multi-tile sprite |

- Each uses the existing `EnemyBase` + `EnemyData` pattern
- Subclass `EnemyBase` and override `PerformAttack()` as with existing enemies

**4.2 — Wave Escalation Improvements**
- **Wave events:** Random modifiers that apply to a wave:
  - "Double Time" — enemies move 50% faster
  - "Bullet Hell" — enemies fire 2x as fast
  - "Swarm" — 2x enemies but at half health
  - "Armored" — all enemies get +50% health
  - "Explosive" — all enemies explode on death
- Display wave modifier on announcement ("Wave 7 — SWARM!")
- **Rest waves** every 5 waves: fewer enemies, more powerup drops, breather

**4.3 — Arena Improvements**
- Destructible platforms (can be shot/exploded to deny enemy paths or create shortcuts)
- Spawn variation: enemies can come from left, right, or drop from above
- Environmental props that interact with combat:
  - Explosive barrels (already exist — make them chain-react)
  - Ammo crates (break for random weapon pickup)
  - Spring pads (launch characters upward)

**4.4 — Progression Between Runs (Lightweight Meta)**
- **High score board** — persistent local top 10 scores
- **Unlockable ducks** (cosmetic at first, then gameplay-altering):
  - Default Duck (balanced)
  - Mallard (faster movement, less health)
  - Muscovy (more health, slower)
  - Mandarin (double jump)
  - Rubber Duck (bouncy — takes no fall damage, higher jump)
- Store unlock state in `PlayerPrefs`
- Unlock conditions: reach wave X, get score Y, kill Z enemies total

**Why this matters:** Content variety is what keeps players coming back. But it's listed in Phase 4 because variety only matters *after* the base game feels good. Polish before content.

---

### Phase 5 — Polished Vertical Slice ("Ship It")

> **Goal:** Everything comes together into a tight, presentable experience that could be shown to players or publishers.

#### Features to Implement

**5.1 — Audio Integration**
- **Music:** One looping track that intensifies with wave progression
  - Start mellow → add drums at wave 3 → add intensity at wave 5+
  - Use layered audio approach (multiple tracks, unmute layers)
  - Free resources: [OpenGameArt.org](https://opengameart.org/), [FreeSFX](https://freesfx.co.uk/), or generate with [Suno AI](https://suno.ai/)
- **SFX priority list** (implement in this order):
  1. Shoot (player) — satisfying pop/pew
  2. Enemy hit — thwack/impact
  3. Enemy death — squish + pop
  4. Explosion — big boom
  5. QUACK — actual duck quack, loud and proud
  6. Jump — small whoosh
  7. Pickup — satisfying chime
  8. Wave start — alarm/horn
  9. Player damage — oof/grunt
  10. Player death — dramatic quack
- Create a simple `AudioManager` singleton with `PlaySFX(clipName)` and `PlayMusic()` methods
- Use `AudioSource` pooling for overlapping SFX

**5.2 — Game Flow Polish**
- **Title screen:** Game logo + "Press any key to start" + animated duck idle
- **Tutorial overlay:** First-run only, shows controls over gameplay (no separate tutorial level)
- **Pause menu:** ESC to pause, resume/restart/quit options
- **Game over screen upgrade:**
  - Show stats: kills, highest combo, waves survived, props destroyed
  - "New High Score!" celebration
  - Immediate restart with key press

**5.3 — Performance Optimization**
- **Object pooling** for projectiles, particles, and enemies using Unity 6's `ObjectPool<T>`:
  ```csharp
  // Use UnityEngine.Pool.ObjectPool<T> for zero-alloc spawning
  private ObjectPool<Projectile> _projectilePool;
  ```
  - Pool projectiles (currently Instantiate/Destroy per shot)
  - Pool particle effects
  - Pool enemy instances per type
- **Cache references** — eliminate all per-frame `FindGameObjectWithTag` calls
- **Profile with Unity Profiler** to catch any remaining hitches

**5.4 — Final Polish Pass**
- Screen transitions (brief fade on restart)
- Consistent UI font and color scheme
- Credits/about screen
- Controller support audit (ensure all inputs work with gamepad)
- Build and test standalone build

**Why this matters:** This phase turns a fun prototype into something you can show people. First impressions matter — a title screen, good audio, and smooth performance separate "hobby project" from "indie game with potential."

---

## 5. QUICK WINS

These can each be implemented in roughly 1-4 hours and have outsized visual/feel impact:

| # | Quick Win | Effort | Impact |
|---|---|---|---|
| 1 | **Increase screen shake** — call `CameraFollow.ShakeCamera()` on every hit, kill, and explosion with varying intensities | 30 min | 🔥🔥🔥🔥🔥 |
| 2 | **Hit stop on kill** — `Time.timeScale = 0` for 0.02s then restore | 1 hr | 🔥🔥🔥🔥🔥 |
| 3 | **Enemy knockback** — apply force to enemy Rigidbody2D on damage | 30 min | 🔥🔥🔥🔥 |
| 4 | **Squash & stretch on jump** — scale player sprite on launch/land | 1 hr | 🔥🔥🔥🔥 |
| 5 | **Coyote time + jump buffer** — forgiveness window for jumps | 1 hr | 🔥🔥🔥🔥 |
| 6 | **Landing dust particles** — simple ParticleSystem burst on ground detect | 1 hr | 🔥🔥🔥 |
| 7 | **Muzzle flash** — enable/disable a child sprite for 1 frame on shoot | 30 min | 🔥🔥🔥 |
| 8 | **Damage flash to white** (not red) — white reads as "hit" universally | 30 min | 🔥🔥🔥 |
| 9 | **Make explosions bigger** — increase `ExplosionEffect` radius by 50% | 15 min | 🔥🔥🔥 |
| 10 | **Instant restart** — skip any delay between death and replay | 30 min | 🔥🔥🔥 |
| 11 | **Random enemy death messages** — already exist, add more variety | 30 min | 🔥🔥 |
| 12 | **Speed up wave transitions** — reduce `timeBetweenWaves` from 5s to 3s | 5 min | 🔥🔥 |
| 13 | **Barrel chain reactions** — barrels in explosion radius also explode | 1 hr | 🔥🔥🔥 |
| 14 | **Trail renderer on projectiles** — add `TrailRenderer` component to feather/bullet prefabs | 30 min | 🔥🔥🔥 |
| 15 | **Health bar color gradient** — green → yellow → red based on HP% | 30 min | 🔥🔥 |

**Recommended first session:** Do #1, #2, #3, #7, and #10. That's ~2.5 hours of work that will make the game feel 10x better.

---

## 6. OPTIONAL ADVANCED FEATURES

These are exciting ideas that should **not** block early progress. Implement only after Phases 1-3 are solid:

### Gameplay
- **Co-op multiplayer** — split screen or online 2-player mode
- **Endless + Challenge modes** — "survive 5 minutes" or "pistol only" modifiers
- **Daily challenge** — seeded random run with online leaderboard
- **Melee attack** — close-range wing slap with high damage

### Technical
- **Replay system** — record inputs, play back best runs
- **Procedural level generation** — randomize platform layouts per run
- **Mod support** — let players create custom ducks/weapons via ScriptableObjects
- **Analytics** — track what wave players die on, what weapons they use most

### Content
- **Story mode** — scripted levels with set pieces and dialogue
- **Multiple biomes** — city, farm, laboratory, military base
- **Cutscenes** — comic-book panel style between acts
- **Lore collectibles** — find documents explaining duck uprising

### Visual
- **Dynamic lighting** — 2D lights for muzzle flashes and explosions (URP 2D Renderer)
- **Weather effects** — rain, fog, dynamic time of day
- **Destructible terrain** — Broforce-style block destruction
- **Ragdoll physics** — enemies ragdoll on death instead of disappearing

---

## 7. TECHNICAL NOTES & UNITY 6 BEST PRACTICES

### Current Architecture Assessment

The codebase is **well-structured for its scope**:
- ✅ Singleton managers (GameManager, WaveManager, UIManager)
- ✅ ScriptableObjects for data-driven enemy stats
- ✅ Component-based HealthSystem shared across entity types
- ✅ State machine for enemy AI (Patrol/Chase/Attack/Stunned/Dead)
- ✅ Inheritance hierarchy for enemy specialization
- ✅ New InputSystem with proper cleanup

### Key Technical Improvements to Make

**Object Pooling (Priority: High)**
- The game currently Instantiate/Destroy projectiles every shot — this creates GC pressure
- Use Unity 6's built-in `UnityEngine.Pool.ObjectPool<T>` (stack-based, non-allocating)
- Pool: projectiles, particle effects, text popups, debris, enemy instances
- Reference: [Unity ObjectPool docs](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Pool.ObjectPool_1.html)

**Caching (Priority: Medium)**
- `UIManager` calls `FindGameObjectWithTag("Player")` — cache this reference
- `CameraFollow` calls `FindGameObjectWithTag("Player")` — already has fallback, but should cache
- Avoid any per-frame `Find*` or `GetComponent*` calls

**Particle Systems (Priority: High)**
- Replace hand-coded debris spawning in `DestructibleProp` with `ParticleSystem`
- Replace hand-coded explosion effect in `ExplosionEffect` with `ParticleSystem`
- ParticleSystem is GPU-accelerated and handles hundreds of particles efficiently

**ScriptableObject Architecture (Priority: Medium)**
- Extend the `EnemyData` pattern to weapons (`WeaponData`) and powerups (`PowerupData`)
- This keeps all balance data in assets, editable without code changes
- Consider ScriptableObject-based event channels for decoupling (see Unity's [modular architecture e-book](https://unity.com/resources/create-modular-game-architecture-with-scriptable-objects-ebook))

**Memory & Performance (Unity 6 Best Practices)**
- Avoid LINQ in Update/FixedUpdate — use manual loops
- Cache `WaitForSeconds` instances in coroutines
- Use `Awaitable` instead of coroutines where appropriate (Unity 6 feature)
- Use `NativeArray<T>` for large temporary collections if needed
- Profile regularly with Unity Profiler

### Folder Structure (Recommended)

```
Assets/
├── Scripts/
│   ├── Player/         (existing)
│   ├── Enemies/        (existing)
│   ├── Weapons/        (existing — expand with WeaponData)
│   ├── Waves/          (existing)
│   ├── Game/           (existing)
│   ├── UI/             (existing)
│   ├── Powerups/       (NEW — PowerupData, PowerupPickup, PowerupEffect)
│   ├── FX/             (NEW — JuiceManager, ParticleSpawner, HitStop)
│   ├── Audio/          (NEW — AudioManager, MusicController)
│   └── ScriptableObjects/ (existing — move .cs definitions here)
├── ScriptableObjects/
│   ├── EnemyData/      (existing)
│   ├── WeaponData/     (NEW)
│   └── PowerupData/    (NEW)
├── Prefabs/            (existing — expand)
├── Sprites/            (existing — expand)
├── Audio/              (NEW — SFX/, Music/)
├── Particles/          (NEW — particle system prefabs)
└── Scenes/             (existing)
```

---

## 8. PRODUCTION PRIORITIES — TL;DR

**Build order matters.** Here's the priority stack:

```
1. 🎮 GAME FEEL     — If it doesn't feel good, nothing else matters
2. 🔊 AUDIO         — Sound is 50% of game feel  
3. 🔫 WEAPON SYSTEM — Variety is what makes it a game
4. 🎨 VISUALS       — Make it look as good as it feels
5. 👾 ENEMIES       — More variety = more replayability
6. ⚡ POWERUPS      — Random elements create emergent fun
7. 📊 PROGRESSION   — Give players reasons to come back
8. ✨ POLISH        — Title screen, menus, transitions
```

**What to delay:**
- ❌ Multiplayer — massive scope increase, do after vertical slice
- ❌ Story mode — write story *after* gameplay is proven
- ❌ Procedural generation — get one arena right first
- ❌ Complex meta-progression — high score is enough for now
- ❌ Mobile/console port — optimize for desktop first

**What gives the most visible improvement per hour:**
1. Screen shake + hit stop (2 hrs → game feels 10x better)
2. Sound effects (4 hrs → game feels alive)
3. Particle effects on everything (4 hrs → visual transformation)
4. One new weapon type (3 hrs → doubles gameplay variety)
5. Powerup drops (3 hrs → creates emergent moments)

---

*This plan is designed to be executed incrementally. Each phase delivers a playable, improved version. Don't wait for the "perfect" version — ship improvements continuously and playtest often.*

*The single most important rule: **If it's not fun, fix that first.***
