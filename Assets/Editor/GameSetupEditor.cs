using UnityEngine;
using UnityEditor;
using System.IO;
using TMPro;

/// <summary>
/// Editor utility that generates placeholder sprites and sets up the entire game scene.
/// Run from Unity menu: DuckRevolution > Setup Game
/// </summary>
public class GameSetupEditor : EditorWindow
{
    [MenuItem("DuckRevolution/Setup Game (Full Auto Setup)")]
    public static void SetupGame()
    {
        // Step 1: Generate placeholder sprites
        GenerateSprites();

        // Step 2: Create ScriptableObject enemy data assets
        CreateEnemyDataAssets();

        // Step 3: Create all prefabs
        CreatePrefabs();

        // Step 4: Build the game scene
        BuildScene();

        Debug.Log("=== DUCK REVOLUTION SETUP COMPLETE ===");
        Debug.Log("Press Play to start the game!");
        EditorUtility.DisplayDialog("Duck Revolution",
            "Game setup complete!\n\nPress Play to start the Duck Revolution!\n\nControls:\n" +
            "- A/D or Arrow Keys: Move\n" +
            "- Space: Jump\n" +
            "- Left Click: Shoot\n" +
            "- Right Click: Throw Egg Grenade\n" +
            "- Q: Quack (Stun)\n" +
            "- R: Restart (when game over)", "QUACK!");
    }

    /// <summary>
    /// Generate simple colored placeholder sprites.
    /// </summary>
    private static void GenerateSprites()
    {
        string spritePath = "Assets/Sprites";

        // Duck (yellow square with orange beak shape)
        CreateColoredSprite(spritePath, "Duck", 32, 32, new Color(1f, 0.9f, 0.2f), DrawDuck);

        // Police (blue)
        CreateColoredSprite(spritePath, "Police", 32, 32, new Color(0.2f, 0.3f, 0.8f), DrawHuman);

        // SWAT (dark gray)
        CreateColoredSprite(spritePath, "SWAT", 32, 32, new Color(0.3f, 0.3f, 0.35f), DrawHuman);

        // Army (green)
        CreateColoredSprite(spritePath, "Army", 32, 32, new Color(0.3f, 0.5f, 0.2f), DrawHuman);

        // Feather projectile (white)
        CreateColoredSprite(spritePath, "Feather", 16, 8, new Color(1f, 1f, 0.9f), null);

        // Bullet projectile (gray)
        CreateColoredSprite(spritePath, "Bullet", 12, 6, new Color(0.8f, 0.7f, 0.2f), null);

        // Egg grenade (white oval)
        CreateColoredSprite(spritePath, "Egg", 16, 20, new Color(1f, 1f, 0.95f), DrawEgg);

        // Crate (brown)
        CreateColoredSprite(spritePath, "Crate", 32, 32, new Color(0.6f, 0.4f, 0.2f), null);

        // Barrel (dark red)
        CreateColoredSprite(spritePath, "Barrel", 24, 32, new Color(0.6f, 0.2f, 0.15f), null);

        // Ground tile (gray-green)
        CreateColoredSprite(spritePath, "Ground", 32, 32, new Color(0.4f, 0.5f, 0.3f), null);

        // Explosion circle
        CreateColoredSprite(spritePath, "ExplosionCircle", 32, 32, new Color(1f, 0.6f, 0f), DrawCircle);

        AssetDatabase.Refresh();
        Debug.Log("Sprites generated!");
    }

    private static void CreateColoredSprite(string folder, string name, int width, int height, Color color, System.Action<Texture2D, Color> drawFunc)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = folder + "/" + name + ".png";

        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point; // Pixel art look

        // Fill with transparent
        Color[] clear = new Color[width * height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;
        tex.SetPixels(clear);

        if (drawFunc != null)
        {
            drawFunc(tex, color);
        }
        else
        {
            // Default: filled rectangle
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tex.SetPixel(x, y, color);
        }

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Import settings
        AssetDatabase.Refresh();
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    // Draw a simple duck shape
    private static void DrawDuck(Texture2D tex, Color color)
    {
        int w = tex.width, h = tex.height;

        // Body (yellow rectangle)
        for (int x = 4; x < w - 4; x++)
            for (int y = 2; y < h - 8; y++)
                tex.SetPixel(x, y, color);

        // Head (wider on top)
        for (int x = 2; x < w - 2; x++)
            for (int y = h - 12; y < h - 2; y++)
                tex.SetPixel(x, y, color);

        // Beak (orange, right side)
        Color beak = new Color(1f, 0.6f, 0.1f);
        for (int x = w - 6; x < w; x++)
            for (int y = h - 10; y < h - 6; y++)
                tex.SetPixel(x, y, beak);

        // Eye (black dot)
        tex.SetPixel(w - 8, h - 5, Color.black);
        tex.SetPixel(w - 7, h - 5, Color.black);
        tex.SetPixel(w - 8, h - 6, Color.black);
        tex.SetPixel(w - 7, h - 6, Color.black);

        // Feet (orange)
        for (int x = 6; x < 10; x++)
            for (int y = 0; y < 3; y++)
                tex.SetPixel(x, y, beak);
        for (int x = w - 10; x < w - 6; x++)
            for (int y = 0; y < 3; y++)
                tex.SetPixel(x, y, beak);
    }

    // Draw a simple human silhouette
    private static void DrawHuman(Texture2D tex, Color color)
    {
        int w = tex.width, h = tex.height;

        // Legs
        for (int x = 8; x < 12; x++)
            for (int y = 0; y < 12; y++)
                tex.SetPixel(x, y, color);
        for (int x = w - 12; x < w - 8; x++)
            for (int y = 0; y < 12; y++)
                tex.SetPixel(x, y, color);

        // Body
        for (int x = 8; x < w - 8; x++)
            for (int y = 10; y < h - 10; y++)
                tex.SetPixel(x, y, color);

        // Head (skin color)
        Color skin = new Color(0.9f, 0.75f, 0.6f);
        for (int x = 10; x < w - 10; x++)
            for (int y = h - 10; y < h - 2; y++)
                tex.SetPixel(x, y, skin);

        // Eyes
        tex.SetPixel(12, h - 5, Color.black);
        tex.SetPixel(w - 13, h - 5, Color.black);
    }

    // Draw egg shape
    private static void DrawEgg(Texture2D tex, Color color)
    {
        int w = tex.width, h = tex.height;
        float cx = w / 2f, cy = h / 2f;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float dx = (x - cx) / (w / 2f);
                float dy = (y - cy) / (h / 2f);
                // Egg shape: ellipse with narrower top
                float topFactor = y > cy ? 0.7f : 1f;
                if (dx * dx / (topFactor * topFactor) + dy * dy < 1f)
                    tex.SetPixel(x, y, color);
            }
        }
    }

    // Draw circle
    private static void DrawCircle(Texture2D tex, Color color)
    {
        int w = tex.width, h = tex.height;
        float cx = w / 2f, cy = h / 2f;
        float radius = Mathf.Min(w, h) / 2f;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    Color c = color;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            }
        }
    }

    /// <summary>
    /// Create ScriptableObject assets for enemy configurations.
    /// </summary>
    private static void CreateEnemyDataAssets()
    {
        string dataPath = "Assets/ScriptableObjects/EnemyData";
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);

        // Police data
        EnemyData police = ScriptableObject.CreateInstance<EnemyData>();
        police.enemyName = "Police Officer";
        police.spriteColor = new Color(0.2f, 0.3f, 0.8f);
        police.maxHealth = 50f;
        police.moveSpeed = 3f;
        police.damage = 8f;
        police.attackRange = 6f;
        police.attackCooldown = 1.5f;
        police.detectionRange = 12f;
        police.patrolSpeed = 1.5f;
        police.scoreValue = 100;
        police.spawnAnnouncement = "The police have arrived!";
        AssetDatabase.CreateAsset(police, dataPath + "/PoliceData.asset");

        // SWAT data
        EnemyData swat = ScriptableObject.CreateInstance<EnemyData>();
        swat.enemyName = "SWAT Operator";
        swat.spriteColor = new Color(0.3f, 0.3f, 0.35f);
        swat.maxHealth = 100f;
        swat.moveSpeed = 4f;
        swat.damage = 12f;
        swat.attackRange = 8f;
        swat.attackCooldown = 0.8f;
        swat.detectionRange = 15f;
        swat.patrolSpeed = 2f;
        swat.scoreValue = 200;
        swat.spawnAnnouncement = "SWAT DEPLOYED!";
        AssetDatabase.CreateAsset(swat, dataPath + "/SwatData.asset");

        // Army data
        EnemyData army = ScriptableObject.CreateInstance<EnemyData>();
        army.enemyName = "Army Soldier";
        army.spriteColor = new Color(0.3f, 0.5f, 0.2f);
        army.maxHealth = 150f;
        army.moveSpeed = 3.5f;
        army.damage = 15f;
        army.attackRange = 10f;
        army.attackCooldown = 0.6f;
        army.detectionRange = 18f;
        army.patrolSpeed = 2f;
        army.scoreValue = 300;
        army.spawnAnnouncement = "THE ARMY HAS BEEN DEPLOYED!";
        AssetDatabase.CreateAsset(army, dataPath + "/ArmyData.asset");

        AssetDatabase.SaveAssets();
        Debug.Log("Enemy data assets created!");
    }

    /// <summary>
    /// Create all game prefabs.
    /// </summary>
    private static void CreatePrefabs()
    {
        string prefabPath = "Assets/Prefabs";
        if (!Directory.Exists(prefabPath))
            Directory.CreateDirectory(prefabPath);

        // Load sprites
        Sprite duckSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Duck.png");
        Sprite policeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Police.png");
        Sprite swatSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SWAT.png");
        Sprite armySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Army.png");
        Sprite featherSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Feather.png");
        Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bullet.png");
        Sprite eggSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Egg.png");
        Sprite crateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Crate.png");
        Sprite barrelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Barrel.png");
        Sprite explosionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ExplosionCircle.png");

        // Load enemy data
        EnemyData policeData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/EnemyData/PoliceData.asset");
        EnemyData swatData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/EnemyData/SwatData.asset");
        EnemyData armyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/ScriptableObjects/EnemyData/ArmyData.asset");

        // === FEATHER PROJECTILE PREFAB ===
        GameObject featherPrefab = CreateProjectilePrefab("FeatherProjectile", featherSprite, true);
        PrefabUtility.SaveAsPrefabAsset(featherPrefab, prefabPath + "/FeatherProjectile.prefab");
        Object.DestroyImmediate(featherPrefab);

        // === BULLET PROJECTILE PREFAB ===
        GameObject bulletPrefab = CreateProjectilePrefab("BulletProjectile", bulletSprite, false);
        PrefabUtility.SaveAsPrefabAsset(bulletPrefab, prefabPath + "/BulletProjectile.prefab");
        Object.DestroyImmediate(bulletPrefab);

        // Reload prefab references
        GameObject featherPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/FeatherProjectile.prefab");
        GameObject bulletPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/BulletProjectile.prefab");

        // === EGG GRENADE PREFAB ===
        GameObject eggPrefab = CreateEggGrenadePrefab("EggGrenade", eggSprite);
        PrefabUtility.SaveAsPrefabAsset(eggPrefab, prefabPath + "/EggGrenade.prefab");
        Object.DestroyImmediate(eggPrefab);

        GameObject eggPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/EggGrenade.prefab");

        // === PLAYER PREFAB ===
        GameObject playerPrefab = CreatePlayerPrefab(duckSprite, featherPrefabAsset, eggPrefabAsset);
        PrefabUtility.SaveAsPrefabAsset(playerPrefab, prefabPath + "/Player.prefab");
        Object.DestroyImmediate(playerPrefab);

        // === ENEMY PREFABS ===
        GameObject policePrefabObj = CreateEnemyPrefab("PoliceEnemy", policeSprite, policeData, bulletPrefabAsset, typeof(PoliceEnemy));
        PrefabUtility.SaveAsPrefabAsset(policePrefabObj, prefabPath + "/PoliceEnemy.prefab");
        Object.DestroyImmediate(policePrefabObj);

        GameObject swatPrefabObj = CreateEnemyPrefab("SwatEnemy", swatSprite, swatData, bulletPrefabAsset, typeof(SwatEnemy));
        PrefabUtility.SaveAsPrefabAsset(swatPrefabObj, prefabPath + "/SwatEnemy.prefab");
        Object.DestroyImmediate(swatPrefabObj);

        GameObject armyPrefabObj = CreateEnemyPrefab("ArmyEnemy", armySprite, armyData, bulletPrefabAsset, typeof(ArmyEnemy));
        // Add grenade prefab reference for army
        ArmyEnemy armyComp = armyPrefabObj.GetComponent<ArmyEnemy>();
        SerializedObject so = new SerializedObject(armyComp);
        SerializedProperty grenadeProp = so.FindProperty("grenadePrefab");
        if (grenadeProp != null)
        {
            grenadeProp.objectReferenceValue = eggPrefabAsset;
            so.ApplyModifiedProperties();
        }
        PrefabUtility.SaveAsPrefabAsset(armyPrefabObj, prefabPath + "/ArmyEnemy.prefab");
        Object.DestroyImmediate(armyPrefabObj);

        // === DESTRUCTIBLE PREFABS ===
        GameObject cratePrefab = CreateDestructiblePrefab("Crate", crateSprite, 30f, new Color(0.6f, 0.4f, 0.2f));
        PrefabUtility.SaveAsPrefabAsset(cratePrefab, prefabPath + "/Crate.prefab");
        Object.DestroyImmediate(cratePrefab);

        GameObject barrelPrefab = CreateDestructiblePrefab("Barrel", barrelSprite, 40f, new Color(0.6f, 0.2f, 0.15f));
        PrefabUtility.SaveAsPrefabAsset(barrelPrefab, prefabPath + "/Barrel.prefab");
        Object.DestroyImmediate(barrelPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All prefabs created!");
    }

    private static GameObject CreateProjectilePrefab(string name, Sprite sprite, bool isPlayerProjectile)
    {
        GameObject obj = new GameObject(name);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Projectiles";

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.15f);

        Projectile proj = obj.AddComponent<Projectile>();

        obj.layer = LayerMask.NameToLayer("Projectile");

        return obj;
    }

    private static GameObject CreateEggGrenadePrefab(string name, Sprite sprite)
    {
        GameObject obj = new GameObject(name);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Projectiles";

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.2f;

        // Add a physics material for bouncing
        PhysicsMaterial2D bounceMat = new PhysicsMaterial2D("EggBounce");
        bounceMat.bounciness = 0.5f;
        bounceMat.friction = 0.3f;
        col.sharedMaterial = bounceMat;

        obj.AddComponent<EggGrenade>();

        obj.layer = LayerMask.NameToLayer("Projectile");

        return obj;
    }

    private static GameObject CreatePlayerPrefab(Sprite sprite, GameObject projectilePrefab, GameObject grenadePrefab)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Player";

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 3f;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 0.9f);
        col.offset = new Vector2(0f, 0.05f);

        // Ground check child object
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        // Fire point child object
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0.6f, 0.1f, 0f);

        // Player Controller
        PlayerController pc = player.AddComponent<PlayerController>();

        // Set serialized fields via SerializedObject
        SerializedObject pcSO = new SerializedObject(pc);
        pcSO.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        pcSO.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
        pcSO.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        pcSO.ApplyModifiedProperties();

        // Weapon System
        WeaponSystem ws = player.AddComponent<WeaponSystem>();
        SerializedObject wsSO = new SerializedObject(ws);
        wsSO.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        wsSO.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        wsSO.FindProperty("eggGrenadePrefab").objectReferenceValue = grenadePrefab;
        wsSO.ApplyModifiedProperties();

        // Link weapon system to player controller
        pcSO = new SerializedObject(pc);
        pcSO.FindProperty("weaponSystem").objectReferenceValue = ws;
        pcSO.ApplyModifiedProperties();

        // Health System
        HealthSystem hs = player.AddComponent<HealthSystem>();
        SerializedObject hsSO = new SerializedObject(hs);
        hsSO.FindProperty("maxHealth").floatValue = 200f;
        hsSO.ApplyModifiedProperties();

        return player;
    }

    private static GameObject CreateEnemyPrefab(string name, Sprite sprite, EnemyData data, GameObject projectilePrefab, System.Type enemyType)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        // Sprite
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Enemies";
        if (data != null)
            sr.color = data.spriteColor;

        // Physics
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 0.9f);
        col.offset = new Vector2(0f, 0.05f);

        // Ground check
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(enemy.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.5f, 0f);

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(enemy.transform);
        firePoint.transform.localPosition = new Vector3(-0.6f, 0.1f, 0f);

        // Health System
        enemy.AddComponent<HealthSystem>();

        // Enemy component
        EnemyBase enemyComp = (EnemyBase)enemy.AddComponent(enemyType);
        SerializedObject so = new SerializedObject(enemyComp);
        so.FindProperty("enemyData").objectReferenceValue = data;
        so.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        so.FindProperty("groundLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
        so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        so.ApplyModifiedProperties();

        return enemy;
    }

    private static GameObject CreateDestructiblePrefab(string name, Sprite sprite, float health, Color debrisColor)
    {
        GameObject obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("Destructible");

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Props";

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();

        DestructibleProp dp = obj.AddComponent<DestructibleProp>();
        SerializedObject so = new SerializedObject(dp);
        so.FindProperty("maxHealth").floatValue = health;
        so.FindProperty("debrisColor").colorValue = debrisColor;
        so.ApplyModifiedProperties();

        return obj;
    }

    /// <summary>
    /// Build the complete game scene with all managers, ground, UI, etc.
    /// </summary>
    private static void BuildScene()
    {
        // Create a new scene or use current one
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // Load prefabs
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        GameObject policePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PoliceEnemy.prefab");
        GameObject swatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SwatEnemy.prefab");
        GameObject armyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ArmyEnemy.prefab");
        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ground.png");
        GameObject cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Crate.prefab");
        GameObject barrelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Barrel.prefab");

        // === GAME MANAGER ===
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gm = gameManagerObj.AddComponent<GameManager>();
        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
        gmSO.ApplyModifiedProperties();

        // === WAVE MANAGER ===
        GameObject waveManagerObj = new GameObject("WaveManager");
        WaveManager wm = waveManagerObj.AddComponent<WaveManager>();
        SerializedObject wmSO = new SerializedObject(wm);
        wmSO.FindProperty("policePrefab").objectReferenceValue = policePrefab;
        wmSO.FindProperty("swatPrefab").objectReferenceValue = swatPrefab;
        wmSO.FindProperty("armyPrefab").objectReferenceValue = armyPrefab;
        wmSO.ApplyModifiedProperties();

        // === CAMERA SETUP ===
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.5f, 0.7f, 1f); // Sky blue
            cam.transform.position = new Vector3(0f, 2f, -10f);
            cam.gameObject.AddComponent<CameraFollow>();
        }

        // === GROUND ===
        CreateGround(groundSprite);

        // === UI CANVAS ===
        CreateUICanvas();

        // === SPAWN POINTS ===
        CreateSpawnPoints(wmSO);

        // === PLAYER SPAWN POINT ===
        GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
        playerSpawn.transform.position = new Vector3(0f, 1f, 0f);
        gmSO = new SerializedObject(gm);
        gmSO.FindProperty("playerSpawnPoint").objectReferenceValue = playerSpawn.transform;
        gmSO.ApplyModifiedProperties();

        // === DECORATIVE PROPS ===
        if (cratePrefab != null)
        {
            GameObject crate1 = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crate1.transform.position = new Vector3(5f, 0.5f, 0f);
            GameObject crate2 = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            crate2.transform.position = new Vector3(-6f, 0.5f, 0f);
        }

        if (barrelPrefab != null)
        {
            GameObject barrel1 = (GameObject)PrefabUtility.InstantiatePrefab(barrelPrefab);
            barrel1.transform.position = new Vector3(8f, 0.5f, 0f);
            GameObject barrel2 = (GameObject)PrefabUtility.InstantiatePrefab(barrelPrefab);
            barrel2.transform.position = new Vector3(-3f, 0.5f, 0f);
        }

        // === BACKGROUND ===
        CreateBackground();

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/DuckRevolution.unity");

        Debug.Log("Scene built successfully!");
    }

    private static void CreateGround(Sprite groundSprite)
    {
        // Create a long ground platform
        GameObject ground = new GameObject("Ground");
        ground.layer = LayerMask.NameToLayer("Ground");
        ground.tag = "Untagged";
        ground.isStatic = true;

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = groundSprite;
        sr.color = new Color(0.4f, 0.5f, 0.3f);
        sr.sortingLayerName = "Background";
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(50f, 2f);

        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
        col.size = new Vector2(50f, 2f);

        ground.transform.position = new Vector3(0f, -1f, 0f);

        // Add some platforms
        CreatePlatform("Platform1", new Vector3(-8f, 2f, 0f), new Vector2(4f, 0.5f), groundSprite);
        CreatePlatform("Platform2", new Vector3(6f, 3f, 0f), new Vector2(5f, 0.5f), groundSprite);
        CreatePlatform("Platform3", new Vector3(0f, 5f, 0f), new Vector2(3f, 0.5f), groundSprite);
    }

    private static void CreatePlatform(string name, Vector3 position, Vector2 size, Sprite sprite)
    {
        GameObject platform = new GameObject(name);
        platform.layer = LayerMask.NameToLayer("Ground");
        platform.isStatic = true;

        SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.35f, 0.45f, 0.25f);
        sr.sortingLayerName = "Background";
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;

        BoxCollider2D col = platform.AddComponent<BoxCollider2D>();
        col.size = size;

        platform.transform.position = position;
    }

    private static void CreateSpawnPoints(SerializedObject wmSO)
    {
        GameObject spawnPointsParent = new GameObject("SpawnPoints");

        // Create spawn points off-screen on both sides
        Transform[] spawnTransforms = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject sp = new GameObject("SpawnPoint_" + i);
            sp.transform.SetParent(spawnPointsParent.transform);

            float x = (i % 2 == 0 ? 1f : -1f) * (15f + i * 2f);
            float y = 1f;
            sp.transform.position = new Vector3(x, y, 0f);

            spawnTransforms[i] = sp.transform;
        }

        // Assign spawn points to wave manager
        SerializedProperty spawnPointsProp = wmSO.FindProperty("spawnPoints");
        spawnPointsProp.arraySize = spawnTransforms.Length;
        for (int i = 0; i < spawnTransforms.Length; i++)
        {
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnTransforms[i];
        }
        wmSO.ApplyModifiedProperties();
    }

    private static void CreateUICanvas()
    {
        // Create Screen Space UI Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Score Text (top-left)
        GameObject scoreObj = CreateUIText("ScoreText", canvasObj.transform,
            new Vector2(20, -20), new Vector2(300, 50),
            "SCORE: 0", TextAnchor.UpperLeft, 28, Color.white);

        // Wave Text (top-center)
        GameObject waveObj = CreateUIText("WaveText", canvasObj.transform,
            new Vector2(0, -20), new Vector2(400, 50),
            "", TextAnchor.UpperCenter, 24, Color.white);

        // Health Text (top-left, below score)
        GameObject healthObj = CreateUIText("HealthText", canvasObj.transform,
            new Vector2(20, -60), new Vector2(200, 40),
            "HP: 200", TextAnchor.UpperLeft, 22, Color.green);

        // Health Bar Background
        GameObject healthBarBg = new GameObject("HealthBarBG");
        healthBarBg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = healthBarBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(20, -90);
        bgRect.sizeDelta = new Vector2(200, 20);
        UnityEngine.UI.Image bgImg = healthBarBg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Health Bar Fill
        GameObject healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(healthBarBg.transform, false);
        RectTransform fillRect = healthBarFill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image fillImg = healthBarFill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = Color.green;
        fillImg.type = UnityEngine.UI.Image.Type.Filled;
        fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

        // Wave Announcement (center of screen, large)
        GameObject announcementObj = CreateUIText("AnnouncementText", canvasObj.transform,
            Vector2.zero, new Vector2(800, 100),
            "", TextAnchor.MiddleCenter, 42, Color.yellow);
        announcementObj.SetActive(false);

        // Controls Help (bottom-left)
        CreateUIText("ControlsText", canvasObj.transform,
            new Vector2(20, 20), new Vector2(500, 120),
            "WASD/Arrows: Move | Space: Jump\nLeft Click: Shoot | Right Click: Egg Grenade\nQ: Quack (Stun) | R: Restart",
            TextAnchor.LowerLeft, 16, new Color(1f, 1f, 1f, 0.6f));

        // Game Over Panel (hidden by default)
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform goRect = gameOverPanel.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.offsetMin = Vector2.zero;
        goRect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image goImg = gameOverPanel.AddComponent<UnityEngine.UI.Image>();
        goImg.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject gameOverText = CreateUIText("GameOverText", gameOverPanel.transform,
            Vector2.zero, new Vector2(600, 300),
            "GAME OVER\n\nThe revolution will return...\n\nPress R to restart",
            TextAnchor.MiddleCenter, 36, Color.red);

        gameOverPanel.SetActive(false);

        // Create UI Manager and assign references
        UIManager uiManager = canvasObj.AddComponent<UIManager>();
        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("scoreText").objectReferenceValue = scoreObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("waveText").objectReferenceValue = waveObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("healthText").objectReferenceValue = healthObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("healthBar").objectReferenceValue = fillImg;
        uiSO.FindProperty("announcementText").objectReferenceValue = announcementObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSO.FindProperty("gameOverScoreText").objectReferenceValue = gameOverText.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        // World Space Canvas for text popups
        GameObject worldCanvasObj = new GameObject("WorldCanvas");
        Canvas worldCanvas = worldCanvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingLayerName = "UI";
        worldCanvas.sortingOrder = 50;

        RectTransform wcRect = worldCanvasObj.GetComponent<RectTransform>();
        wcRect.sizeDelta = new Vector2(100, 50);
        wcRect.localScale = Vector3.one * 0.01f;

        uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("worldCanvas").objectReferenceValue = worldCanvas;
        uiSO.ApplyModifiedProperties();
    }

    private static TextAlignmentOptions ConvertAnchorToTMP(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.Center;
        }
    }

    private static GameObject CreateUIText(string name, Transform parent, Vector2 position, Vector2 size, string text, TextAnchor anchor, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();

        // Set anchors based on position
        if (position.x <= 0 && position.y >= 0)
        {
            // Top-left
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
        }
        else if (position.x > 0 && position.y >= 0)
        {
            // Top-right
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
        }
        else if (Mathf.Approximately(position.x, 0) && Mathf.Approximately(position.y, 0))
        {
            // Center
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI uiText = obj.AddComponent<TextMeshProUGUI>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.color = color;
        uiText.alignment = ConvertAnchorToTMP(anchor);
        uiText.textWrappingMode = TextWrappingModes.NoWrap;

        // Add outline for readability
        uiText.outlineWidth = 0.2f;
        uiText.outlineColor = Color.black;

        return obj;
    }

    private static void CreateBackground()
    {
        // Simple sky background
        GameObject bg = new GameObject("Background");

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.5f, 0.7f, 1f);
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -10;

        // Make it large enough to fill the view
        bg.transform.localScale = new Vector3(100f, 50f, 1f);
        bg.transform.position = new Vector3(0f, 10f, 5f);
    }
}
