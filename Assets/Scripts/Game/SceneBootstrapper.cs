using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SceneBootstrapper auto-builds the entire game scene at runtime.
/// Attach this to a single empty GameObject called "Bootstrapper" in the scene.
///
/// This means you can open the project, add one empty scene with one empty
/// GameObject + this script, press Play, and immediately test the game.
///
/// To replace placeholder sprites later:
///   - Assign real sprites to the prefabs in the Inspector.
///   - Or replace SpriteGenerator calls with Sprite.Create from loaded assets.
/// </summary>
public class SceneBootstrapper : MonoBehaviour
{
    // ---- Optional overrides (leave null to use generated sprites) ----
    [Header("Optional Sprite Overrides")]
    [Tooltip("Leave null to use generated placeholder sprites")]
    public Sprite playerSprite;
    public Sprite policeSprite;
    public Sprite swatSprite;
    public Sprite armySprite;
    public Sprite bulletSprite;
    public Sprite eggSprite;
    public Sprite groundSprite;
    public Sprite crateSprite;
    public Sprite barrelSprite;

    // ---- Internal prefabs (built at runtime) ----
    private GameObject _bulletPrefab;
    private GameObject _enemyBulletPrefab;
    private GameObject _grenadePrefab;
    private GameObject _policePrefab;
    private GameObject _swatPrefab;
    private GameObject _armyPrefab;

    // ---- EnemyStats ScriptableObjects ----
    private EnemyStats _policeStats;
    private EnemyStats _swatStats;
    private EnemyStats _armyStats;

    // ================================================================
    void Awake()
    {
        BuildStats();
        BuildPrefabs();
        BuildLevel();
        BuildPlayer();
        BuildEnemyPrefabs();
        WaveManager waveManager = BuildWaveManager();
        BuildCamera();
        BuildUI();
        BuildGameManager(waveManager);

        // Self-destruct so it doesn't run twice
        Destroy(gameObject);
    }

    // ================================================================
    // ---- Enemy Stats ----
    // ================================================================
    private void BuildStats()
    {
        _policeStats = ScriptableObject.CreateInstance<EnemyStats>();
        _policeStats.enemyName    = "Police Officer";
        _policeStats.maxHealth    = 40f;
        _policeStats.moveSpeed    = 2f;
        _policeStats.patrolRange  = 6f;
        _policeStats.detectionRange = 10f;
        _policeStats.attackRange  = 7f;
        _policeStats.attackDamage = 8f;
        _policeStats.attackCooldown = 1.8f;
        _policeStats.bulletSpeed  = 9f;
        _policeStats.scoreValue   = 50;
        _policeStats.deathQuotes  = new[] { "I should've stayed in the donut shop!", "T-This wasn't in my contract!", "OFFICER DOWN!" };

        _swatStats = ScriptableObject.CreateInstance<EnemyStats>();
        _swatStats.enemyName    = "SWAT Officer";
        _swatStats.maxHealth    = 80f;
        _swatStats.moveSpeed    = 3f;
        _swatStats.patrolRange  = 5f;
        _swatStats.detectionRange = 12f;
        _swatStats.attackRange  = 8f;
        _swatStats.attackDamage = 12f;
        _swatStats.attackCooldown = 1.2f;
        _swatStats.bulletSpeed  = 12f;
        _swatStats.scoreValue   = 100;
        _swatStats.deathQuotes  = new[] { "This wasn't covered in training!", "I QUIT!", "Tell my captain I tried!" };

        _armyStats = ScriptableObject.CreateInstance<EnemyStats>();
        _armyStats.enemyName    = "Army Soldier";
        _armyStats.maxHealth    = 130f;
        _armyStats.moveSpeed    = 3.5f;
        _armyStats.patrolRange  = 4f;
        _armyStats.detectionRange = 15f;
        _armyStats.attackRange  = 10f;
        _armyStats.attackDamage = 18f;
        _armyStats.attackCooldown = 0.9f;
        _armyStats.bulletSpeed  = 15f;
        _armyStats.scoreValue   = 200;
        _armyStats.deathQuotes  = new[] { "TELL MY MOM I DIED FIGHTING A DUCK!", "Mission… failed…", "HOW?! IT'S A DUCK!" };
    }

    // ================================================================
    // ---- Shared Prefabs ----
    // ================================================================
    private void BuildPrefabs()
    {
        _bulletPrefab      = CreateBulletPrefab(isPlayer: true);
        _enemyBulletPrefab = CreateBulletPrefab(isPlayer: false);
        _grenadePrefab     = CreateGrenadePrefab();
    }

    private GameObject CreateBulletPrefab(bool isPlayer)
    {
        GameObject go = new GameObject(isPlayer ? "PlayerBullet" : "EnemyBullet");

        go.tag = "Bullet";

        // Sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = bulletSprite ?? SpriteGenerator.CreateBulletSprite();
        sr.sortingLayerName = "Default";

        // Physics
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.15f;

        // Bullet component – fields are public now
        Bullet b = go.AddComponent<Bullet>();
        b.damage         = isPlayer ? 25f : 10f;
        b.isPlayerBullet = isPlayer;
        b.lifeTime       = 4f;

        // Keep as persistent prefab template (disable so it's not in scene)
        go.SetActive(false);
        DontDestroyOnLoad(go);
        return go;
    }

    private GameObject CreateGrenadePrefab()
    {
        GameObject go = new GameObject("EggGrenade");
        go.tag = "Grenade";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = eggSprite ?? SpriteGenerator.CreateEggSprite();
        sr.sortingLayerName = "Default";

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        go.AddComponent<EggGrenade>();

        go.SetActive(false);
        DontDestroyOnLoad(go);
        return go;
    }

    // ================================================================
    // ---- Level Geometry ----
    // ================================================================
    private void BuildLevel()
    {
        // Main ground platform
        CreatePlatform(new Vector2(0, -3f), new Vector2(50f, 1f), "Ground");

        // Elevated platforms for variety
        CreatePlatform(new Vector2(-10f, 0f), new Vector2(8f, 0.5f));
        CreatePlatform(new Vector2(8f,   1f), new Vector2(6f, 0.5f));
        CreatePlatform(new Vector2(18f,  0f), new Vector2(8f, 0.5f));
        CreatePlatform(new Vector2(-20f, 0f), new Vector2(8f, 0.5f));

        // Destructible crates
        CreateCrate(new Vector2(-5f,  -2.2f));
        CreateCrate(new Vector2(-4f,  -2.2f));
        CreateCrate(new Vector2( 5f,  -2.2f));
        CreateCrate(new Vector2(12f,  -2.2f));

        // Explosive barrels
        CreateBarrel(new Vector2(-8f,  -2.2f));
        CreateBarrel(new Vector2( 8f,  -2.2f));
        CreateBarrel(new Vector2(15f,  -2.2f));

        // Background
        CreateBackground();

        // Invisible walls at level edges
        CreateWall(new Vector2(-26f, 0f), new Vector2(1f, 20f));
        CreateWall(new Vector2( 26f, 0f), new Vector2(1f, 20f));
    }

    private void CreatePlatform(Vector2 pos, Vector2 size, string tag = "")
    {
        GameObject go = new GameObject("Platform");
        go.transform.position = pos;
        if (!string.IsNullOrEmpty(tag)) go.tag = tag;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = groundSprite ?? SpriteGenerator.CreateGroundSprite();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size     = size;
        sr.sortingLayerName = "Default";
        sr.sortingOrder     = -1;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    private void CreateCrate(Vector2 pos)
    {
        GameObject go = new GameObject("Crate");
        go.transform.position = pos;
        go.tag = "Destructible";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = crateSprite ?? SpriteGenerator.CreateCrateSprite();

        go.AddComponent<BoxCollider2D>();
        go.AddComponent<DestructibleObject>();
    }

    private void CreateBarrel(Vector2 pos)
    {
        GameObject go = new GameObject("Barrel");
        go.transform.position = pos;
        go.tag = "Destructible";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = barrelSprite ?? SpriteGenerator.CreateBarrelSprite();

        go.AddComponent<BoxCollider2D>();
        go.AddComponent<DestructibleObject>();
    }

    private void CreateWall(Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Wall");
        go.transform.position = pos;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    private void CreateBackground()
    {
        GameObject go = new GameObject("Background");
        go.transform.position = new Vector3(0, 3, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBackgroundSprite(512, 256);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(60f, 20f);
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -10;
    }

    // ================================================================
    // ---- Player ----
    // ================================================================
    private void BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(-5f, -1f, 0f);
        player.tag = "Player";

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite ?? SpriteGenerator.CreateDuckSprite();

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 1f);

        // Ground check child
        GameObject groundCheckGo = new GameObject("GroundCheck");
        groundCheckGo.transform.SetParent(player.transform);
        groundCheckGo.transform.localPosition = new Vector3(0f, -0.55f, 0f);

        // Fire point child
        GameObject firePointGo = new GameObject("FirePoint");
        firePointGo.transform.SetParent(player.transform);
        firePointGo.transform.localPosition = new Vector3(0.6f, 0.1f, 0f);

        // Health
        HealthSystem hs = player.AddComponent<HealthSystem>();
        hs.maxHealth = 100f;
        hs.isPlayer  = true;

        // Weapon – set public fields
        WeaponSystem ws = player.AddComponent<WeaponSystem>();
        ws.bulletPrefab  = _bulletPrefab;
        ws.grenadePrefab = _grenadePrefab;
        ws.firePoint     = firePointGo.transform;

        // Controller – set public fields
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.groundCheck = groundCheckGo.transform;
        pc.groundLayer = LayerMask.GetMask("Default");
    }

    // ================================================================
    // ---- Enemy Prefabs ----
    // ================================================================
    private void BuildEnemyPrefabs()
    {
        _policePrefab = CreateEnemyPrefab("Police", _policeStats,
            policeSprite ?? SpriteGenerator.CreatePoliceSprite(),
            typeof(PoliceEnemy));

        _swatPrefab = CreateEnemyPrefab("SWAT", _swatStats,
            swatSprite ?? SpriteGenerator.CreateSwatSprite(),
            typeof(SwatEnemy));

        _armyPrefab = CreateEnemyPrefab("Army", _armyStats,
            armySprite ?? SpriteGenerator.CreateArmySprite(),
            typeof(ArmyEnemy));
    }

    private GameObject CreateEnemyPrefab(string name, EnemyStats stats, Sprite sprite, System.Type enemyType)
    {
        GameObject go = new GameObject(name + "Enemy");
        go.tag = "Enemy";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D col = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 1f);

        // Fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(go.transform);
        fp.transform.localPosition = new Vector3(0.6f, 0.1f, 0f);

        // Health
        HealthSystem hs = go.AddComponent<HealthSystem>();
        hs.maxHealth = stats.maxHealth;

        // Enemy component – public fields
        EnemyBase enemy = (EnemyBase)go.AddComponent(enemyType);
        enemy.stats       = stats;
        enemy.bulletPrefab = _enemyBulletPrefab;
        enemy.firePoint    = fp.transform;

        go.SetActive(false);
        DontDestroyOnLoad(go);
        return go;
    }

    // ================================================================
    // ---- Wave Manager ----
    // ================================================================
    private WaveManager BuildWaveManager()
    {
        GameObject wm = new GameObject("WaveManager");

        // Spawn points (off-screen left and right)
        Transform[] spawnPoints = new Transform[3];

        GameObject sp1 = new GameObject("SpawnPointRight");
        sp1.transform.SetParent(wm.transform);
        sp1.transform.position = new Vector3(22f, -1.5f, 0f);
        spawnPoints[0] = sp1.transform;

        GameObject sp2 = new GameObject("SpawnPointLeft");
        sp2.transform.SetParent(wm.transform);
        sp2.transform.position = new Vector3(-22f, -1.5f, 0f);
        spawnPoints[1] = sp2.transform;

        GameObject sp3 = new GameObject("SpawnPointRight2");
        sp3.transform.SetParent(wm.transform);
        sp3.transform.position = new Vector3(18f, -1.5f, 0f);
        spawnPoints[2] = sp3.transform;

        WaveManager waveManager = wm.AddComponent<WaveManager>();
        waveManager.spawnPoints = spawnPoints;

        // Define waves
        waveManager.waves = new WaveManager.Wave[]
        {
            // Wave 1 – Police
            new WaveManager.Wave
            {
                waveName     = "Police Response",
                announcement = "THE DUCK REVOLUTION HAS BEGUN!\nWave 1: Police Deployed",
                predelay     = 2f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _policePrefab, count = 4, interval = 1f }
                }
            },
            // Wave 2 – More Police
            new WaveManager.Wave
            {
                waveName     = "Backup Requested",
                announcement = "MORE POLICE INCOMING!\nWave 2: Reinforcements",
                predelay     = 3f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _policePrefab, count = 6, interval = 0.8f }
                }
            },
            // Wave 3 – SWAT
            new WaveManager.Wave
            {
                waveName     = "SWAT Deployed",
                announcement = "SWAT DEPLOYED!\nWave 3: They mean business",
                predelay     = 3f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _swatPrefab, count = 4, interval = 0.8f }
                }
            },
            // Wave 4 – SWAT + Police mix
            new WaveManager.Wave
            {
                waveName     = "Full SWAT Response",
                announcement = "FULL SWAT MOBILIZATION!\nWave 4: Buckle up!",
                predelay     = 3f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _swatPrefab,   count = 4, interval = 0.7f },
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _policePrefab, count = 3, interval = 0.5f }
                }
            },
            // Wave 5 – Army
            new WaveManager.Wave
            {
                waveName     = "Military Response",
                announcement = "THEY BROUGHT THE ARMY?!\nWave 5: God help us all",
                predelay     = 4f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _armyPrefab, count = 5, interval = 0.7f }
                }
            },
            // Wave 6 – All out
            new WaveManager.Wave
            {
                waveName     = "Total War",
                announcement = "TOTAL WAR DECLARED!\nWave 6: The Final Stand",
                predelay     = 4f,
                enemies      = new WaveManager.EnemySpawnEntry[]
                {
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _armyPrefab,  count = 4, interval = 0.6f },
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _swatPrefab,  count = 3, interval = 0.5f },
                    new WaveManager.EnemySpawnEntry { enemyPrefab = _policePrefab,count = 2, interval = 0.4f }
                }
            }
        };

        return waveManager;
    }

    // ================================================================
    // ---- Camera ----
    // ================================================================
    private void BuildCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.4f, 0.6f, 1f);
        cam.transform.position = new Vector3(0, 0, -10f);

        if (cam.GetComponent<CameraFollow>() == null)
            cam.gameObject.AddComponent<CameraFollow>();
    }

    // ================================================================
    // ---- UI ----
    // ================================================================
    private void BuildUI()
    {
        // Create Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ---- Health Bar background ----
        GameObject hbBg = CreateUIPanel(canvasGo.transform, "HealthBarBG",
            new Vector2(10, -10), new Vector2(200, 24),
            new Vector2(0, 1), new Vector2(0, 1),
            new Color(0, 0, 0, 0.5f));

        // Fill image (green bar)
        GameObject hbFill = CreateUIPanel(hbBg.transform, "HealthFill",
            new Vector2(0, 0), new Vector2(196, 20),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Color(0.2f, 0.8f, 0.2f));
        RectTransform fillRT = hbFill.GetComponent<RectTransform>();
        fillRT.anchoredPosition = new Vector2(2, 0);

        // Slider component on BG
        UnityEngine.UI.Slider slider = hbBg.AddComponent<UnityEngine.UI.Slider>();
        slider.fillRect  = fillRT;
        slider.minValue  = 0;
        slider.maxValue  = 100;
        slider.value     = 100;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;

        // ---- Score Text ----
        GameObject scoreGo = new GameObject("ScoreText");
        scoreGo.transform.SetParent(canvasGo.transform, false);
        RectTransform scoreRT = scoreGo.AddComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(1, 1);
        scoreRT.anchorMax = new Vector2(1, 1);
        scoreRT.anchoredPosition = new Vector2(-10, -10);
        scoreRT.sizeDelta = new Vector2(220, 30);
        TMPro.TextMeshProUGUI scoreText = scoreGo.AddComponent<TMPro.TextMeshProUGUI>();
        scoreText.text      = "SCORE: 0";
        scoreText.fontSize  = 20;
        scoreText.color     = Color.white;
        scoreText.alignment = TMPro.TextAlignmentOptions.Right;

        // ---- Wave Announcement Panel ----
        GameObject announcePanelGo = CreateUIPanel(canvasGo.transform, "AnnouncementPanel",
            new Vector2(0, 50), new Vector2(700, 90),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0, 0, 0, 0.8f));
        announcePanelGo.SetActive(false);

        TMPro.TextMeshProUGUI announceText = CreateTMPText(announcePanelGo.transform,
            "AnnouncementText", Vector2.zero, new Vector2(680, 80),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            "WAVE 1: THE DUCK REVOLUTION HAS BEGUN!", 22, Color.yellow);

        // ---- Wave Counter ----
        TMPro.TextMeshProUGUI waveCountText = CreateTMPText(canvasGo.transform,
            "WaveCounterText", new Vector2(10, -40), new Vector2(300, 28),
            new Vector2(0, 1), new Vector2(0, 1),
            "WAVE 1", 16, Color.cyan);

        // ---- Game Over Panel ----
        GameObject goPanelGo = CreateUIPanel(canvasGo.transform, "GameOverPanel",
            Vector2.zero, new Vector2(540, 220),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0.5f, 0, 0, 0.9f));
        CreateTMPText(goPanelGo.transform, "GameOverText",
            Vector2.zero, new Vector2(520, 200),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            "GAME OVER\nThe humans win…for now.", 30, Color.white);
        goPanelGo.SetActive(false);

        // ---- Victory Panel ----
        GameObject victPanelGo = CreateUIPanel(canvasGo.transform, "VictoryPanel",
            Vector2.zero, new Vector2(640, 220),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0, 0.35f, 0, 0.9f));
        CreateTMPText(victPanelGo.transform, "VictoryText",
            Vector2.zero, new Vector2(620, 200),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            "VICTORY!\nTHE DUCK REVOLUTION\nCANNOT BE STOPPED!", 28, Color.yellow);
        victPanelGo.SetActive(false);

        // ---- UIManager – set public fields ----
        UIManager uiMgr = canvasGo.AddComponent<UIManager>();
        uiMgr.healthSlider       = slider;
        uiMgr.announcementPanel  = announcePanelGo;
        uiMgr.announcementText   = announceText;
        uiMgr.scoreText          = scoreText;
        uiMgr.waveCounterText    = waveCountText;
        uiMgr.gameOverPanel      = goPanelGo;
        uiMgr.victoryPanel       = victPanelGo;
        // Health bar is hooked in UIManager.Start() via FindGameObjectWithTag("Player")
    }

    // ================================================================
    // ---- Game Manager ----
    // ================================================================
    private void BuildGameManager(WaveManager waveManager)
    {
        GameObject gmGo = new GameObject("GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        gm.waveManager = waveManager;
    }

    // ================================================================
    // ---- UI Helpers ----
    // ================================================================
    private GameObject CreateUIPanel(Transform parent, string name, Vector2 anchoredPos,
        Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;

        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        return go;
    }

    private TMPro.TextMeshProUGUI CreateTMPText(Transform parent, string name,
        Vector2 anchoredPos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax,
        string text, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;

        TMPro.TextMeshProUGUI tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        return tmp;
    }
}
