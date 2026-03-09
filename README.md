# 🦆 Duck Revolution - A Chaotic 2D Action Game

**A goofy, chaotic 2D action game built in Unity where ducks rise up against humanity!**

*Inspired by the tone, pacing, and destructibility of Broforce.*

## 🎮 Game Concept

One day, ducks gain consciousness and decide humans have oppressed them long enough. The ducks go completely berserk and start attacking humanity. The humans respond with escalating forces: police officers, SWAT teams, and eventually the military.

**THE DUCK REVOLUTION HAS BEGUN!**

## 📋 Requirements

- **Unity 2022.3 LTS** or newer (2023.x also works)
- Windows, macOS, or Linux

## 🚀 Quick Setup (5 Minutes!)

### Step 1: Install Unity
1. Download and install [Unity Hub](https://unity.com/download)
2. Install **Unity 2022.3 LTS** (or newer) via Unity Hub
3. Make sure to include the **2D** template support

### Step 2: Open the Project
1. Open Unity Hub
2. Click **"Open"** → Navigate to this repository folder
3. Select the root folder (the one containing `Assets/`, `ProjectSettings/`, `Packages/`)
4. Unity will import the project (this may take a few minutes on first open)

### Step 3: Auto-Setup the Game
1. Once Unity opens, go to the menu bar
2. Click **DuckRevolution > Setup Game (Full Auto Setup)**
3. This will automatically:
   - Generate all placeholder sprites (duck, police, SWAT, army, projectiles, etc.)
   - Create ScriptableObject enemy configurations
   - Build all prefabs (player, enemies, projectiles, destructible props)
   - Construct the entire game scene with UI, ground, platforms, and spawn points
4. A dialog box will confirm setup is complete

### Step 4: Play!
1. Press the **Play** button (▶) in Unity
2. **THE DUCK REVOLUTION BEGINS!**

## 🎯 Controls

| Key | Action |
|-----|--------|
| **A/D** or **←/→** | Move left/right |
| **Space** | Jump |
| **Left Mouse Button** | Shoot feathers |
| **Right Mouse Button** | Throw egg grenade |
| **Q** | QUACK! (Stun nearby enemies) |
| **R** | Restart (when game over) |

## 🌊 Wave System

| Wave | Enemies | Description |
|------|---------|-------------|
| 1-2 | Police Officers | Basic enemies with pistols |
| 3-4 | SWAT + Police | Tougher enemies with burst fire |
| 5+ | Army + SWAT + Police | The toughest soldiers with grenades |

Enemies get progressively harder with each wave. The number of enemies increases too!

## 🦆 Duck Abilities

- **Feather Shot**: Rapid-fire feather projectiles
- **Egg Grenade**: Bouncing explosive eggs with area damage
- **QUACK**: Stuns all nearby enemies (5-second cooldown)

## 📁 Project Structure

```
Assets/
├── Editor/
│   └── GameSetupEditor.cs          # Auto-setup tool (menu: DuckRevolution > Setup Game)
├── Scenes/
│   └── DuckRevolution.unity         # Main game scene (created by setup)
├── Scripts/
│   ├── Player/
│   │   └── PlayerController.cs      # Player movement, input, and abilities
│   ├── Enemies/
│   │   ├── EnemyBase.cs             # Base enemy class with AI state machine
│   │   ├── EnemyData.cs             # ScriptableObject for enemy stats
│   │   ├── PoliceEnemy.cs           # Police officer (waves 1-2)
│   │   ├── SwatEnemy.cs             # SWAT operator with burst fire (waves 3-4)
│   │   └── ArmyEnemy.cs            # Army soldier with grenades (waves 5+)
│   ├── Weapons/
│   │   ├── WeaponSystem.cs          # Shooting and grenade logic
│   │   ├── Projectile.cs            # Projectile movement and collision
│   │   ├── EggGrenade.cs            # Egg grenade with explosion
│   │   └── ExplosionEffect.cs       # Visual explosion effect
│   ├── Waves/
│   │   └── WaveManager.cs           # Wave spawning and progression
│   ├── Game/
│   │   ├── GameManager.cs           # Game state management
│   │   ├── CameraFollow.cs          # Smooth camera follow with screen shake
│   │   ├── HealthSystem.cs          # Shared health/damage system
│   │   └── DestructibleProp.cs      # Breakable crates and barrels
│   └── UI/
│       ├── UIManager.cs             # Score, health, wave announcements
│       └── TextPopup.cs             # Floating text popups
├── ScriptableObjects/
│   └── EnemyData/                   # Enemy configuration assets (created by setup)
├── Prefabs/                          # Game prefabs (created by setup)
├── Sprites/                          # Placeholder sprites (created by setup)
└── Materials/
```

## 🛠 Manual Setup (Alternative)

If the auto-setup doesn't work, here's how to set things up manually:

### Create Sprites
1. Create simple 32x32 pixel art sprites for: Duck (yellow), Police (blue), SWAT (dark gray), Army (green)
2. Create small sprites for: Feather (16x8, white), Bullet (12x6, yellow), Egg (16x20, white)
3. Create Ground tile (32x32, gray-green)
4. Place all sprites in `Assets/Sprites/`
5. Set all sprite import settings to: Sprite Mode: Single, Pixels Per Unit: 32, Filter Mode: Point

### Create Enemy Data
1. Right-click in `Assets/ScriptableObjects/EnemyData/`
2. Select **Create > DuckRevolution > Enemy Data**
3. Create three: PoliceData, SwatData, ArmyData
4. Configure stats (health, speed, damage, etc.)

### Create Player Prefab
1. Create empty GameObject, name it "Player"
2. Add components: `SpriteRenderer`, `Rigidbody2D`, `BoxCollider2D`, `PlayerController`, `WeaponSystem`, `HealthSystem`
3. Set tag to "Player", layer to "Player"
4. Create child "GroundCheck" at (0, -0.5, 0)
5. Create child "FirePoint" at (0.6, 0.1, 0)
6. Configure Rigidbody2D: Freeze Rotation Z, Gravity Scale 3
7. Drag to `Assets/Prefabs/`

### Create Enemy Prefabs
1. Similar to player but with `EnemyBase` (or subclass), `HealthSystem`
2. Set tag to "Enemy", layer to "Enemy"
3. Assign EnemyData ScriptableObject
4. Drag to `Assets/Prefabs/`

### Build the Scene
1. Create Ground: BoxCollider2D + SpriteRenderer, layer "Ground", position (0, -1, 0)
2. Add GameManager, WaveManager, UIManager GameObjects
3. Set up the Camera with CameraFollow
4. Create UI Canvas with score, health bar, and announcement text

## 🎨 Art Style

The game uses simple placeholder sprites that can be easily replaced:
- 32x32 pixel art characters
- Side-view perspective (like Broforce)
- Exaggerated, cartoonish style
- Everything feels chaotic and ridiculous

## 🔧 Customization

### Adding New Enemy Types
1. Create a new script inheriting from `EnemyBase`
2. Override `PerformAttack()` for custom attack behavior
3. Create an `EnemyData` ScriptableObject with stats
4. Create a prefab and add it to the `WaveManager`

### Adding Powerups
The code is structured for easy expansion:
- Add new weapon types in `WeaponSystem.cs`
- Create new projectile types inheriting from `Projectile.cs`
- Add pickup/powerup scripts that modify `PlayerController` or `WeaponSystem`

### Modifying Waves
Edit the `WaveManager` to:
- Change `baseEnemiesPerWave` and `enemiesPerWaveIncrease`
- Modify `GetEnemyPrefabForWave()` for different enemy compositions
- Add new enemy prefab slots

## 🐛 Troubleshooting

**"Setup Game" doesn't appear in the menu:**
- Make sure the `GameSetupEditor.cs` file is in `Assets/Editor/`
- Wait for Unity to finish compiling (check the bottom-right spinner)

**Player falls through the ground:**
- Make sure the Ground object has layer "Ground"
- Make sure the Player's GroundCheck has the Ground layer mask set

**Enemies don't spawn:**
- Check the WaveManager has all three enemy prefabs assigned
- Check that spawn points exist in the scene

**Nothing happens when pressing Play:**
- Make sure `GameManager` exists in the scene
- Check the Console (Window > General > Console) for errors

## 📜 License

This is a fun learning project. Use it however you like!

## 🦆 THE DUCK REVOLUTION WILL NOT BE TELEVISED 🦆