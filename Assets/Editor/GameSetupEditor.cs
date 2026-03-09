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
        // Step 0: Register required tags and layers in TagManager before anything uses them
        EnsureLayersAndTags();

        // Step 1: Generate improved sprites
        GenerateSprites();

        // Step 2: Create ScriptableObject enemy data assets
        CreateEnemyDataAssets();

        // Step 3: Create all prefabs
        CreatePrefabs();

        // Step 4: Build the game scene
        BuildScene();

        // Step 5: Build the main-menu scene
        BuildStartMenuScene();

        // Step 6: Register scenes in build settings
        RegisterScenesInBuildSettings();

        Debug.Log("=== DUCK REVOLUTION SETUP COMPLETE ===");
        Debug.Log("Press Play to start the game!");
        EditorUtility.DisplayDialog("Duck Revolution",
            "Game setup complete!\n\nOpen the MainMenu scene and press Play!\n\nControls:\n" +
            "- A/D or Arrow Keys: Move\n" +
            "- Space: Jump\n" +
            "- Left Click: Shoot\n" +
            "- Right Click: Throw Egg Grenade\n" +
            "- Q: Quack (Stun)\n" +
            "- Shift: Wing Dash\n" +
            "- R: Restart (game over)\n" +
            "- ESC: Main Menu (game over)", "QUACK!");
    }

    [MenuItem("DuckRevolution/Setup Start Menu Only")]
    public static void SetupStartMenuOnly()
    {
        BuildStartMenuScene();
        RegisterScenesInBuildSettings();
        EditorUtility.DisplayDialog("Duck Revolution", "Main menu scene created!", "OK");
    }

    /// <summary>
    /// Ensures all required tags and layers exist in TagManager before the setup runs.
    /// Uses SerializedObject so changes take effect immediately without a domain reload.
    /// </summary>
    private static void EnsureLayersAndTags()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        string[] requiredTags = { "Enemy" }; // "Player" is a built-in Unity tag
        foreach (string tag in requiredTags)
            AddTagIfMissing(tagManager, tag);

        string[] requiredLayers = { "Ground", "Player", "Enemy", "Projectile", "Destructible" };
        foreach (string layer in requiredLayers)
            AddLayerIfMissing(tagManager, layer);

        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Tags and layers verified.");
    }

    private static void AddTagIfMissing(SerializedObject tagManager, string tag)
    {
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
    }

    private static void AddLayerIfMissing(SerializedObject tagManager, string layerName)
    {
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        for (int i = 0; i < layersProp.arraySize; i++)
        {
            if (layersProp.GetArrayElementAtIndex(i).stringValue == layerName) return;
        }
        // User layers start at index 8; find first empty slot
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(i).stringValue))
            {
                layersProp.GetArrayElementAtIndex(i).stringValue = layerName;
                return;
            }
        }
        // If array doesn't reach slot 31 yet, expand it
        if (layersProp.arraySize < 32)
        {
            layersProp.InsertArrayElementAtIndex(layersProp.arraySize);
            layersProp.GetArrayElementAtIndex(layersProp.arraySize - 1).stringValue = layerName;
        }
        else
        {
            Debug.LogWarning($"Could not add layer '{layerName}': all 32 slots are occupied.");
        }
    }

    /// <summary>
    /// Generate polished pixel-art sprites for all game objects.
    /// </summary>
    private static void GenerateSprites()
    {
        string spritePath = "Assets/Sprites";

        // ── Characters (64×64) ─────────────────────────────────────────────────
        CreateSprite(spritePath, "Duck",   64, 64, DrawDuck);
        CreateSprite(spritePath, "Police", 64, 64, DrawPolice);
        CreateSprite(spritePath, "SWAT",   64, 64, DrawSwat);
        CreateSprite(spritePath, "Army",   64, 64, DrawArmy);

        // ── Projectiles (24×10 / 16×16) ───────────────────────────────────────
        CreateSprite(spritePath, "Feather",         20, 8,  DrawFeather);
        CreateSprite(spritePath, "Bullet",          14, 6,  DrawBullet);
        CreateSprite(spritePath, "Egg",             20, 24, DrawEgg);
        CreateSprite(spritePath, "ExplosionCircle", 48, 48, DrawExplosion);

        // ── Props (32–48 px) ───────────────────────────────────────────────────
        CreateSprite(spritePath, "Crate",  40, 40, DrawCrate);
        CreateSprite(spritePath, "Barrel", 32, 48, DrawBarrel);

        // ── Environment (64×32) ───────────────────────────────────────────────
        CreateSprite(spritePath, "Ground", 64, 32, DrawGround);

        // ── UI / Misc ─────────────────────────────────────────────────────────
        CreateSprite(spritePath, "Star", 8, 8, DrawStar);

        AssetDatabase.Refresh();
        Debug.Log("Sprites generated!");
    }

    private static void CreateSprite(string folder, string name, int w, int h,
                                     System.Action<Texture2D> drawFunc)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = folder + "/" + name + ".png";

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color[] clear = new Color[w * h];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);

        drawFunc?.Invoke(tex);
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 32;
            imp.filterMode          = FilterMode.Point;
            imp.textureCompression  = TextureImporterCompression.Uncompressed;
            TextureImporterSettings s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }
    }

    // ── Helper: set a pixel only if within bounds ─────────────────────────────
    private static void SetPx(Texture2D t, int x, int y, Color c)
    {
        if (x >= 0 && x < t.width && y >= 0 && y < t.height)
            t.SetPixel(x, y, c);
    }
    private static void FillRect(Texture2D t, int x0, int y0, int x1, int y1, Color c)
    {
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                SetPx(t, x, y, c);
    }
    private static void Outline(Texture2D t, int x0, int y0, int x1, int y1, Color c)
    {
        for (int x = x0; x <= x1; x++) { SetPx(t, x, y0, c); SetPx(t, x, y1, c); }
        for (int y = y0; y <= y1; y++) { SetPx(t, x0, y, c); SetPx(t, x1, y, c); }
    }
    private static void FillEllipse(Texture2D t, int cx, int cy, int rx, int ry, Color c)
    {
        for (int x = cx - rx; x <= cx + rx; x++)
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                float dx = (x - cx) / (float)rx;
                float dy = (y - cy) / (float)ry;
                if (dx*dx + dy*dy <= 1f) SetPx(t, x, y, c);
            }
    }

    // ── DUCK (64×64) ──────────────────────────────────────────────────────────
    private static void DrawDuck(Texture2D t)
    {
        int W = t.width, H = t.height;
        // Palette
        Color yB  = new Color(1.00f, 0.85f, 0.10f);   // body yellow
        Color yL  = new Color(1.00f, 0.95f, 0.55f);   // highlight
        Color yD  = new Color(0.85f, 0.65f, 0.00f);   // shadow
        Color ora = new Color(1.00f, 0.52f, 0.05f);   // beak / feet
        Color blk = new Color(0.10f, 0.05f, 0.05f);   // eye / outline
        Color wht = Color.white;                        // eye highlight
        Color red = new Color(0.75f, 0.08f, 0.08f);   // cap
        Color redD= new Color(0.50f, 0.03f, 0.03f);   // cap shadow
        Color wng = new Color(0.95f, 0.80f, 0.20f);   // wing

        // Body (oval)
        FillEllipse(t, 32, 22, 16, 18, yB);
        FillEllipse(t, 32, 22, 14, 16, yL);      // highlight centre
        // Shadow sides
        for (int y = 10; y <= 36; y++)
        {
            SetPx(t, 16, y, yD); SetPx(t, 17, y, yD);
            SetPx(t, 47, y, yD); SetPx(t, 48, y, yD);
        }
        // Wing (left side oval, slightly different shade)
        FillEllipse(t, 28, 20, 10, 7, wng);
        Outline(t, 18, 13, 38, 27, yD);

        // Neck + Head
        FillRect(t, 28, 37, 36, 44, yB);             // neck
        FillEllipse(t, 40, 50, 14, 12, yB);           // head
        FillEllipse(t, 40, 52, 10,  8, yL);           // head highlight
        Outline(t, 26, 36, 54, 62, yD);

        // Beak (orange, pointing right)
        Color beakShade = new Color(0.9f, 0.38f, 0f);
        FillRect(t, 52, 49, 62, 52, ora);
        FillRect(t, 56, 47, 62, 48, ora);             // upper lip
        FillRect(t, 56, 53, 62, 54, ora);             // lower lip
        SetPx(t, 60, 50, beakShade); SetPx(t, 61, 50, beakShade);
        SetPx(t, 60, 51, beakShade); SetPx(t, 61, 51, beakShade);

        // Eye (black + white sparkle)
        FillRect(t, 49, 54, 52, 57, blk);
        SetPx(t, 49, 57, wht); SetPx(t, 50, 57, wht);

        // Revolutionary beret (red cap on top)
        FillEllipse(t, 40, 60, 12, 4, red);
        FillRect(t, 32, 62, 48, 63, redD);            // brim
        FillEllipse(t, 42, 63, 3, 3, red);            // pompon

        // Feet (orange)
        FillRect(t, 24, 2,  32, 5, ora);
        FillRect(t, 36, 2,  44, 5, ora);
        FillRect(t, 22, 4,  28, 5, ora);  // left toe spread
        FillRect(t, 38, 4,  46, 5, ora);  // right toe spread
        FillRect(t, 26, 5,  30, 8, yD);   // ankle left
        FillRect(t, 38, 5,  42, 8, yD);   // ankle right
    }

    // ── POLICE (64×64) ────────────────────────────────────────────────────────
    private static void DrawPolice(Texture2D t)
    {
        Color uni  = new Color(0.20f, 0.30f, 0.75f);  // uniform blue
        Color uniD = new Color(0.12f, 0.18f, 0.55f);  // shadow blue
        Color uniL = new Color(0.40f, 0.55f, 0.95f);  // highlight
        Color skin = new Color(0.95f, 0.80f, 0.65f);
        Color blk  = new Color(0.08f, 0.08f, 0.10f);
        Color hat  = new Color(0.08f, 0.10f, 0.25f);
        Color gold = new Color(1.00f, 0.82f, 0.20f);
        Color grey = new Color(0.55f, 0.55f, 0.60f);

        // Legs
        FillRect(t, 20, 2,  28, 26, uni);
        FillRect(t, 36, 2,  44, 26, uni);
        FillRect(t, 20, 2,  28,  4, blk);   // boots
        FillRect(t, 36, 2,  44,  4, blk);

        // Body / torso
        FillRect(t, 18, 24, 46, 44, uni);
        FillRect(t, 18, 24, 18, 44, uniD);  // shadow
        FillRect(t, 45, 24, 46, 44, uniD);
        FillRect(t, 28, 28, 36, 44, uniL);  // centre highlight
        // Badge
        FillRect(t, 30, 36, 34, 40, gold);
        SetPx(t, 32, 39, uniD);

        // Arms
        FillRect(t, 10, 26, 18, 44, uni);   // left arm
        FillRect(t, 46, 26, 54, 44, uni);   // right arm
        // Hands
        FillRect(t, 10, 40, 18, 44, skin);
        // Gun (right hand)
        FillRect(t, 46, 36, 58, 40, grey);
        FillRect(t, 52, 32, 56, 36, grey);

        // Neck + head
        FillRect(t, 28, 44, 36, 48, skin);
        FillEllipse(t, 32, 54, 12, 10, skin);
        // Eyes
        FillRect(t, 27, 54, 29, 56, blk);
        FillRect(t, 35, 54, 37, 56, blk);
        SetPx(t, 27, 56, Color.white);
        SetPx(t, 35, 56, Color.white);
        // Mouth
        FillRect(t, 29, 50, 35, 51, new Color(0.7f, 0.4f, 0.3f));

        // Police cap
        FillRect(t, 22, 60, 42, 63, hat);  // brim
        FillEllipse(t, 32, 63, 9, 4, hat);  // dome
        // Cap badge
        FillRect(t, 29, 62, 35, 63, gold);
    }

    // ── SWAT (64×64) ──────────────────────────────────────────────────────────
    private static void DrawSwat(Texture2D t)
    {
        Color arm  = new Color(0.18f, 0.20f, 0.22f);  // dark tactical gear
        Color armL = new Color(0.30f, 0.34f, 0.38f);  // highlight
        Color armD = new Color(0.08f, 0.09f, 0.10f);
        Color skin = new Color(0.90f, 0.76f, 0.60f);
        Color blk  = new Color(0.05f, 0.05f, 0.07f);
        Color vest = new Color(0.22f, 0.25f, 0.30f);
        Color red  = new Color(0.80f, 0.10f, 0.10f);  // insignia
        Color vis  = new Color(0.25f, 0.60f, 0.95f);  // visor

        // Legs + boots
        FillRect(t, 20, 2,  28, 26, arm);
        FillRect(t, 36, 2,  44, 26, arm);
        FillRect(t, 20, 2,  28,  6, armD);
        FillRect(t, 36, 2,  44,  6, armD);
        // Kneepads
        FillRect(t, 20, 16, 28, 22, armL);
        FillRect(t, 36, 16, 44, 22, armL);

        // Body / vest
        FillRect(t, 16, 24, 48, 46, arm);
        FillRect(t, 20, 26, 44, 44, vest);  // vest plate
        FillRect(t, 20, 26, 22, 44, armD);  // shadow
        // Vest detail stripes
        for (int y = 28; y <= 42; y += 5)
            FillRect(t, 22, y, 42, y+1, armL);
        // SWAT text (simplified)
        FillRect(t, 26, 30, 38, 33, red);

        // Arms
        FillRect(t, 8,  24, 16, 44, arm);
        FillRect(t, 48, 24, 56, 44, arm);
        // Hands
        FillRect(t, 8,  40, 16, 44, armD);
        // Assault rifle (right)
        FillRect(t, 48, 32, 62, 37, armD);
        FillRect(t, 56, 28, 60, 32, armD);  // barrel
        FillRect(t, 48, 37, 54, 40, armD);  // stock

        // Helmet
        FillEllipse(t, 32, 58, 14, 10, arm);
        FillRect(t, 18, 50, 46, 54, arm);
        // Visor
        FillRect(t, 22, 50, 42, 57, vis);
        Outline(t, 22, 50, 42, 57, armD);
        // Ear comm
        FillRect(t, 44, 55, 48, 58, armD);
    }

    // ── ARMY (64×64) ──────────────────────────────────────────────────────────
    private static void DrawArmy(Texture2D t)
    {
        Color cam  = new Color(0.36f, 0.46f, 0.24f);  // camo green
        Color camL = new Color(0.50f, 0.62f, 0.34f);
        Color camD = new Color(0.22f, 0.30f, 0.14f);
        Color cam2 = new Color(0.56f, 0.50f, 0.25f);  // tan patch
        Color skin = new Color(0.92f, 0.78f, 0.62f);
        Color blk  = new Color(0.08f, 0.08f, 0.08f);
        Color grey = new Color(0.48f, 0.48f, 0.50f);

        // Boots + legs
        FillRect(t, 20, 2,  28, 26, cam);
        FillRect(t, 36, 2,  44, 26, cam);
        FillRect(t, 20, 2,  28,  7, blk);
        FillRect(t, 36, 2,  44,  7, blk);
        // Camo patches on legs
        FillRect(t, 22, 10, 26, 14, cam2);
        FillRect(t, 38, 14, 42, 18, camD);

        // Body
        FillRect(t, 16, 24, 48, 46, cam);
        FillRect(t, 18, 26, 46, 44, camL);
        FillRect(t, 16, 26, 18, 44, camD);
        // Camo blobs on torso
        FillRect(t, 24, 28, 32, 32, camD);
        FillRect(t, 34, 34, 40, 38, cam2);
        // Belt
        FillRect(t, 16, 24, 48, 26, blk);

        // Arms
        FillRect(t, 8,  24, 16, 44, cam);
        FillRect(t, 48, 24, 56, 44, cam);
        FillRect(t, 8,  40, 16, 44, skin); // hands
        // Rifle
        FillRect(t, 48, 30, 62, 35, grey);
        FillRect(t, 58, 26, 62, 30, grey);

        // Neck + face
        FillRect(t, 28, 44, 36, 48, skin);
        FillEllipse(t, 32, 54, 12, 10, skin);
        FillRect(t, 27, 50, 37, 52, cam); // face paint
        FillRect(t, 28, 54, 30, 56, blk);  // eyes
        FillRect(t, 34, 54, 36, 56, blk);

        // Helmet
        FillEllipse(t, 32, 62, 14, 5, camD);
        FillRect(t, 18, 59, 46, 63, cam);
        FillEllipse(t, 32, 62, 13, 4, camL);
        Outline(t, 18, 59, 46, 63, camD);
        // Netting blobs on helmet
        FillRect(t, 22, 61, 24, 63, cam2);
        FillRect(t, 36, 60, 40, 62, camD);
    }

    // ── FEATHER (20×8) ────────────────────────────────────────────────────────
    private static void DrawFeather(Texture2D t)
    {
        Color wht = new Color(1f, 1f, 0.92f);
        Color grey = new Color(0.85f, 0.85f, 0.80f);
        Color tip  = new Color(0.95f, 0.92f, 0.60f);
        // Quill
        FillRect(t, 0, 3, 18, 4, grey);
        // Barbs
        for (int x = 2; x <= 16; x += 2)
        {
            SetPx(t, x, 5, wht); SetPx(t, x, 6, wht);
            SetPx(t, x, 1, wht); SetPx(t, x, 2, wht);
        }
        // Tip
        FillRect(t, 17, 3, 19, 4, tip);
    }

    // ── BULLET (14×6) ─────────────────────────────────────────────────────────
    private static void DrawBullet(Texture2D t)
    {
        Color brass = new Color(0.88f, 0.72f, 0.18f);
        Color brassD= new Color(0.62f, 0.48f, 0.08f);
        Color tip2  = new Color(0.75f, 0.60f, 0.40f);
        // Casing
        FillRect(t, 0, 1, 9, 4, brass);
        FillRect(t, 0, 1, 0, 4, brassD);
        FillRect(t, 0, 1, 9, 1, new Color(1f,0.92f,0.5f));
        // Tip
        FillRect(t, 10, 2, 13, 3, tip2);
        SetPx(t, 13, 2, new Color(0.9f, 0.7f, 0.4f));
        SetPx(t, 13, 3, new Color(0.9f, 0.7f, 0.4f));
    }

    // ── EGG (20×24) ───────────────────────────────────────────────────────────
    private static void DrawEgg(Texture2D t)
    {
        int W = t.width, H = t.height;
        Color shell = new Color(1.00f, 0.98f, 0.92f);
        Color shad  = new Color(0.80f, 0.78f, 0.72f);
        Color hiL   = Color.white;
        Color crack = new Color(0.70f, 0.65f, 0.55f);

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                float nx = (x - W*0.5f) / (W*0.45f);
                float ny = (y - H*0.42f) / (H * (y > H*0.5f ? 0.55f : 0.45f));
                if (nx*nx + ny*ny < 1f)
                {
                    Color c = shell;
                    float shad_t = (x - W*0.5f) / W;
                    if (shad_t < -0.15f) c = Color.Lerp(shell, shad, (-shad_t - 0.15f) * 3f);
                    if (x > W*0.6f && y > H*0.5f) c = Color.Lerp(c, hiL, 0.4f);
                    SetPx(t, x, y, c);
                }
            }
        // Crack decoration
        SetPx(t, 9, 10, crack); SetPx(t, 10, 11, crack);
        SetPx(t, 11, 10, crack); SetPx(t, 10, 9, crack);
    }

    // ── EXPLOSION (48×48) ─────────────────────────────────────────────────────
    private static void DrawExplosion(Texture2D t)
    {
        int W = t.width, H = t.height;
        float cx = W * 0.5f, cy = H * 0.5f;

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float r = W * 0.5f;
                if (dist > r) continue;
                float t2 = dist / r;
                Color c;
                if (t2 < 0.25f)       c = new Color(1f, 1f, 0.8f, 1f - t2 * 2f);
                else if (t2 < 0.55f)  c = new Color(1f, 0.7f - t2 * 0.5f, 0.1f, 1f - t2 * 0.6f);
                else                  c = new Color(0.7f, 0.25f, 0.05f, (1f - t2) * 1.5f);
                SetPx(t, x, y, c);
            }
    }

    // ── CRATE (40×40) ─────────────────────────────────────────────────────────
    private static void DrawCrate(Texture2D t)
    {
        Color wood  = new Color(0.75f, 0.52f, 0.28f);
        Color woodL = new Color(0.88f, 0.68f, 0.40f);
        Color woodD = new Color(0.50f, 0.33f, 0.16f);
        Color plank = new Color(0.60f, 0.40f, 0.20f);
        Color nail  = new Color(0.65f, 0.65f, 0.65f);

        FillRect(t, 0, 0, 39, 39, wood);
        // Top face (lighter)
        FillRect(t, 0, 32, 39, 39, woodL);
        // Shadow sides
        FillRect(t, 0, 0, 2, 39, woodD);
        FillRect(t, 37, 0, 39, 39, woodD);
        FillRect(t, 0, 0, 39, 2, woodD);
        // Plank lines
        FillRect(t, 0, 13, 39, 14, plank);
        FillRect(t, 0, 26, 39, 27, plank);
        FillRect(t, 13, 0, 14, 39, plank);
        FillRect(t, 26, 0, 27, 39, plank);
        // Corner nails
        int[] nc = {3, 11, 17, 23, 29, 37};
        foreach (int nx in nc)
            foreach (int ny in nc)
            {
                SetPx(t, nx,   ny, nail);
                SetPx(t, nx+1, ny, nail);
                SetPx(t, nx, ny+1, nail);
            }
        Outline(t, 0, 0, 39, 39, woodD);
    }

    // ── BARREL (32×48) ────────────────────────────────────────────────────────
    private static void DrawBarrel(Texture2D t)
    {
        Color body  = new Color(0.55f, 0.18f, 0.12f);
        Color bodyL = new Color(0.72f, 0.28f, 0.20f);
        Color bodyD = new Color(0.32f, 0.08f, 0.06f);
        Color hoop  = new Color(0.35f, 0.32f, 0.28f);
        Color lid   = new Color(0.48f, 0.48f, 0.45f);

        // Body (slight bulge via widening mid-section)
        for (int y = 0; y < 48; y++)
        {
            float t2 = (y - 24f) / 24f;
            int hw = (int)(14 + 2 * (1 - t2*t2));  // barrel bulge
            FillRect(t, 16-hw, y, 16+hw, y, body);
        }
        // Highlight stripe
        for (int y = 4; y < 44; y++) SetPx(t, 20, y, bodyL);
        // Hoops
        FillRect(t, 4, 10, 28, 12, hoop);
        FillRect(t, 4, 23, 28, 25, hoop);
        FillRect(t, 4, 36, 28, 38, hoop);
        // Lid
        FillRect(t, 6, 44, 26, 47, lid);
        FillRect(t, 6, 0,  26,  3, lid);
        Outline(t, 6, 44, 26, 47, hoop);
        Outline(t, 6, 0,  26,  3, hoop);
    }

    // ── GROUND (64×32) ────────────────────────────────────────────────────────
    private static void DrawGround(Texture2D t)
    {
        Color grass  = new Color(0.35f, 0.62f, 0.22f);
        Color grassL = new Color(0.48f, 0.76f, 0.30f);
        Color grassD = new Color(0.22f, 0.44f, 0.12f);
        Color soil   = new Color(0.50f, 0.36f, 0.20f);
        Color soilD  = new Color(0.34f, 0.22f, 0.10f);
        Color rock   = new Color(0.55f, 0.52f, 0.48f);

        // Soil fill
        FillRect(t, 0, 0, 63, 23, soil);
        // Soil variation
        for (int x = 4; x < 64; x += 8)  FillRect(t, x, 6, x+3, 9, soilD);
        for (int x = 1; x < 64; x += 10) FillRect(t, x, 14, x+2, 16, rock);

        // Grass top layer
        FillRect(t, 0, 24, 63, 31, grass);
        FillRect(t, 0, 30, 63, 31, grassL);  // bright top edge
        FillRect(t, 0, 24, 63, 24, grassD);  // under-grass line

        // Grass tufts
        int[] tufts = {3, 11, 19, 27, 35, 43, 51, 59};
        foreach (int tx in tufts)
        {
            SetPx(t, tx, 31, grassL);
            if (tx + 1 < 64) { SetPx(t, tx+1, 31, grassL); SetPx(t, tx+1, 30, grassL); }
        }
    }

    // ── STAR (8×8) - used in main menu background ─────────────────────────────
    private static void DrawStar(Texture2D t)
    {
        Color w = Color.white;
        // Simple 5-pixel cross + corners
        SetPx(t, 3, 7, w); SetPx(t, 4, 7, w);
        SetPx(t, 3, 0, w); SetPx(t, 4, 0, w);
        for (int i = 1; i < 7; i++) { SetPx(t, 3, i, w); SetPx(t, 4, i, w); }
        for (int i = 1; i < 7; i++) { SetPx(t, i, 3, w); SetPx(t, i, 4, w); }
    }

    /// <summary>
    /// Legacy wrapper kept for any external editor code that calls the old two-color signature.
    /// Delegates to the newer <see cref="CreateSprite"/> overload.
    /// </summary>
    private static void CreateColoredSprite(string folder, string name, int w, int h,
        Color color, System.Action<Texture2D, Color> drawFunc)
    {
        CreateSprite(folder, name, w, h, tex =>
        {
            if (drawFunc != null) drawFunc(tex, color);
            else FillRect(tex, 0, 0, w-1, h-1, color);
        });
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
        GameObject cratePrefab = CreateDestructiblePrefab("Crate", crateSprite, 30f, new Color(0.6f, 0.4f, 0.2f), false);
        PrefabUtility.SaveAsPrefabAsset(cratePrefab, prefabPath + "/Crate.prefab");
        Object.DestroyImmediate(cratePrefab);

        GameObject barrelPrefab = CreateDestructiblePrefab("Barrel", barrelSprite, 40f, new Color(0.6f, 0.2f, 0.15f), true);
        PrefabUtility.SaveAsPrefabAsset(barrelPrefab, prefabPath + "/Barrel.prefab");
        Object.DestroyImmediate(barrelPrefab);

        // === NEW ENEMY PREFABS ===
        GameObject riotPrefabObj = CreateEnemyPrefab("RiotShieldEnemy", policeSprite, policeData, bulletPrefabAsset, typeof(RiotShieldEnemy));
        PrefabUtility.SaveAsPrefabAsset(riotPrefabObj, prefabPath + "/RiotShieldEnemy.prefab");
        Object.DestroyImmediate(riotPrefabObj);

        GameObject sniperPrefabObj = CreateEnemyPrefab("SniperEnemy", armySprite, armyData, bulletPrefabAsset, typeof(SniperEnemy));
        PrefabUtility.SaveAsPrefabAsset(sniperPrefabObj, prefabPath + "/SniperEnemy.prefab");
        Object.DestroyImmediate(sniperPrefabObj);

        GameObject k9PrefabObj = CreateEnemyPrefab("K9Enemy", policeSprite, policeData, null, typeof(K9Enemy));
        PrefabUtility.SaveAsPrefabAsset(k9PrefabObj, prefabPath + "/K9Enemy.prefab");
        Object.DestroyImmediate(k9PrefabObj);

        GameObject dronePrefabObj = CreateEnemyPrefab("DroneEnemy", armySprite, armyData, eggPrefabAsset, typeof(DroneEnemy));
        SerializedObject droneSO2 = new SerializedObject(dronePrefabObj.GetComponent<DroneEnemy>());
        SerializedProperty bombProp = droneSO2.FindProperty("bombPrefab");
        if (bombProp != null) { bombProp.objectReferenceValue = eggPrefabAsset; droneSO2.ApplyModifiedProperties(); }
        PrefabUtility.SaveAsPrefabAsset(dronePrefabObj, prefabPath + "/DroneEnemy.prefab");
        Object.DestroyImmediate(dronePrefabObj);

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

    private static GameObject CreateDestructiblePrefab(string name, Sprite sprite, float health, Color debrisColor, bool isExplosive = false)
    {
        GameObject obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("Destructible");

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Props";

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        obj.AddComponent<BoxCollider2D>();

        DestructibleProp dp = obj.AddComponent<DestructibleProp>();
        SerializedObject so = new SerializedObject(dp);
        so.FindProperty("maxHealth").floatValue = health;
        so.FindProperty("debrisColor").colorValue = debrisColor;
        SerializedProperty explosiveProp = so.FindProperty("isExplosive");
        if (explosiveProp != null) explosiveProp.boolValue = isExplosive;
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
        GameObject riotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RiotShieldEnemy.prefab");
        GameObject sniperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SniperEnemy.prefab");
        GameObject k9Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/K9Enemy.prefab");
        GameObject dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/DroneEnemy.prefab");
        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ground.png");
        GameObject cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Crate.prefab");
        GameObject barrelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Barrel.prefab");

        // === GAME MANAGER ===
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gm = gameManagerObj.AddComponent<GameManager>();
        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
        gmSO.ApplyModifiedProperties();

        // === PARTICLE MANAGER ===
        GameObject particleObj = new GameObject("ParticleManager");
        particleObj.AddComponent<ParticleManager>();

        // === JUICE MANAGER ===
        GameObject juiceObj = new GameObject("JuiceManager");
        juiceObj.AddComponent<JuiceManager>();

        // === AUDIO MANAGER ===
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();

        // === WAVE MANAGER ===
        GameObject waveManagerObj = new GameObject("WaveManager");
        WaveManager wm = waveManagerObj.AddComponent<WaveManager>();
        SerializedObject wmSO = new SerializedObject(wm);
        wmSO.FindProperty("policePrefab").objectReferenceValue = policePrefab;
        wmSO.FindProperty("swatPrefab").objectReferenceValue = swatPrefab;
        wmSO.FindProperty("armyPrefab").objectReferenceValue = armyPrefab;
        wmSO.FindProperty("riotShieldPrefab").objectReferenceValue = riotPrefab;
        wmSO.FindProperty("sniperPrefab").objectReferenceValue = sniperPrefab;
        wmSO.FindProperty("k9Prefab").objectReferenceValue = k9Prefab;
        wmSO.FindProperty("dronePrefab").objectReferenceValue = dronePrefab;
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

        // === DECORATIVE PROPS - Richer arena layout ===
        PlaceProps(cratePrefab, barrelPrefab);

        // === PARALLAX BACKGROUND ===
        CreateBackground();

        // Save the scene (ensure directory exists first)
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/DuckRevolution.unity");

        Debug.Log("Scene built successfully!");
    }

    private static void CreateGround(Sprite groundSprite)
    {
        // Main ground
        GameObject ground = new GameObject("Ground");
        ground.layer = LayerMask.NameToLayer("Ground");
        ground.isStatic = true;

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = groundSprite;
        sr.color = new Color(0.35f, 0.45f, 0.25f);
        sr.sortingLayerName = "Background";
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(60f, 2f);

        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
        col.size = new Vector2(60f, 2f);

        ground.transform.position = new Vector3(0f, -1f, 0f);

        // Arena platforms - varied heights for interesting combat
        CreatePlatform("Platform_Left_Low",   new Vector3(-10f, 1.5f, 0f), new Vector2(5f, 0.5f), groundSprite);
        CreatePlatform("Platform_Left_High",  new Vector3(-7f,  3.5f, 0f), new Vector2(3f, 0.5f), groundSprite);
        CreatePlatform("Platform_Center",     new Vector3(0f,   4.5f, 0f), new Vector2(4f, 0.5f), groundSprite);
        CreatePlatform("Platform_Right_Low",  new Vector3(9f,   2.0f, 0f), new Vector2(5f, 0.5f), groundSprite);
        CreatePlatform("Platform_Right_High", new Vector3(6f,   4.0f, 0f), new Vector2(3f, 0.5f), groundSprite);
        CreatePlatform("Platform_Far_Left",   new Vector3(-18f, 1.0f, 0f), new Vector2(4f, 0.5f), groundSprite);
        CreatePlatform("Platform_Far_Right",  new Vector3(18f,  1.0f, 0f), new Vector2(4f, 0.5f), groundSprite);
    }

    private static void PlaceProps(GameObject cratePrefab, GameObject barrelPrefab)
    {
        // Ground level props
        Vector3[] cratePositions = {
            new Vector3(3f, 0.5f, 0f), new Vector3(-5f, 0.5f, 0f),
            new Vector3(12f, 0.5f, 0f), new Vector3(-12f, 0.5f, 0f),
            new Vector3(0f, 5.0f, 0f),  // On center platform
        };
        Vector3[] barrelPositions = {
            new Vector3(7f, 0.5f, 0f), new Vector3(-2f, 0.5f, 0f),
            new Vector3(10f, 2.7f, 0f), // On right platform
            new Vector3(-9f, 4.2f, 0f), // On left high platform
        };

        foreach (var pos in cratePositions)
        {
            if (cratePrefab == null) break;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab);
            go.transform.position = pos;
        }

        foreach (var pos in barrelPositions)
        {
            if (barrelPrefab == null) break;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(barrelPrefab);
            go.transform.position = pos;
        }
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

        // Ground spawn points off-screen on both sides, at varied heights
        Vector3[] groundSpawnPositions = {
            new Vector3( 17f, 1f,  0f),
            new Vector3(-17f, 1f,  0f),
            new Vector3( 19f, 3f,  0f),
            new Vector3(-19f, 3f,  0f),
            new Vector3( 16f, 0.5f, 0f),
            new Vector3(-16f, 0.5f, 0f),
        };

        Transform[] spawnTransforms = new Transform[groundSpawnPositions.Length];
        for (int i = 0; i < groundSpawnPositions.Length; i++)
        {
            var sp = new GameObject("SpawnPoint_" + i);
            sp.transform.SetParent(spawnPointsParent.transform);
            sp.transform.position = groundSpawnPositions[i];
            spawnTransforms[i] = sp.transform;
        }

        // Aerial spawn points for drones (high, off-screen)
        Vector3[] aerialPositions = {
            new Vector3( 18f, 8f, 0f),
            new Vector3(-18f, 8f, 0f),
            new Vector3( 14f, 7f, 0f),
            new Vector3(-14f, 7f, 0f),
        };

        Transform[] aerialTransforms = new Transform[aerialPositions.Length];
        for (int i = 0; i < aerialPositions.Length; i++)
        {
            var sp = new GameObject("AerialSpawn_" + i);
            sp.transform.SetParent(spawnPointsParent.transform);
            sp.transform.position = aerialPositions[i];
            aerialTransforms[i] = sp.transform;
        }

        // Assign to wave manager
        SerializedProperty spawnPointsProp = wmSO.FindProperty("spawnPoints");
        spawnPointsProp.arraySize = spawnTransforms.Length;
        for (int i = 0; i < spawnTransforms.Length; i++)
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnTransforms[i];

        SerializedProperty aerialProp = wmSO.FindProperty("aerialSpawnPoints");
        if (aerialProp != null)
        {
            aerialProp.arraySize = aerialTransforms.Length;
            for (int i = 0; i < aerialTransforms.Length; i++)
                aerialProp.GetArrayElementAtIndex(i).objectReferenceValue = aerialTransforms[i];
        }

        wmSO.ApplyModifiedProperties();
    }

    private static void CreateUICanvas()
    {
        // Screen Space UI Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── TOP-RIGHT: Score panel ─────────────────────────────────────────────
        GameObject scorePanelObj = new GameObject("ScorePanel");
        scorePanelObj.transform.SetParent(canvasObj.transform, false);
        var scorePanelRect = scorePanelObj.AddComponent<RectTransform>();
        scorePanelRect.anchorMin = new Vector2(1, 1);
        scorePanelRect.anchorMax = new Vector2(1, 1);
        scorePanelRect.pivot     = new Vector2(1, 1);
        scorePanelRect.anchoredPosition = new Vector2(-14, -14);
        scorePanelRect.sizeDelta = new Vector2(280, 52);
        var scorePanelImg = scorePanelObj.AddComponent<UnityEngine.UI.Image>();
        scorePanelImg.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(scorePanelObj.transform, false);
        var scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = Vector2.zero; scoreRect.anchorMax = Vector2.one;
        scoreRect.offsetMin = new Vector2(10, 4); scoreRect.offsetMax = new Vector2(-10, -4);
        TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "SCORE: 0";
        scoreTMP.fontSize = 28;
        scoreTMP.color = new Color(1f, 0.95f, 0.55f);
        scoreTMP.alignment = TextAlignmentOptions.MidlineRight;
        scoreTMP.textWrappingMode = TextWrappingModes.NoWrap;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.outlineWidth = 0.25f;
        scoreTMP.outlineColor = new Color(0.3f, 0.2f, 0f, 0.9f);

        // ── TOP-CENTER: Wave text ──────────────────────────────────────────────
        GameObject waveObj = CreateUIText("WaveText", canvasObj.transform,
            new Vector2(0, -14), new Vector2(440, 48),
            "", TextAnchor.UpperCenter, 22, new Color(0.88f, 0.88f, 0.88f));

        // ── TOP-LEFT: Health panel ─────────────────────────────────────────────
        GameObject hpPanelObj = new GameObject("HPPanel");
        hpPanelObj.transform.SetParent(canvasObj.transform, false);
        var hpPanelRect = hpPanelObj.AddComponent<RectTransform>();
        hpPanelRect.anchorMin = new Vector2(0, 1);
        hpPanelRect.anchorMax = new Vector2(0, 1);
        hpPanelRect.pivot     = new Vector2(0, 1);
        hpPanelRect.anchoredPosition = new Vector2(14, -14);
        hpPanelRect.sizeDelta = new Vector2(220, 66);
        var hpPanelImg = hpPanelObj.AddComponent<UnityEngine.UI.Image>();
        hpPanelImg.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject healthObj = new GameObject("HealthText");
        healthObj.transform.SetParent(hpPanelObj.transform, false);
        var healthRect = healthObj.AddComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0, 0.5f);
        healthRect.anchorMax = new Vector2(1, 1);
        healthRect.offsetMin = new Vector2(8, 0); healthRect.offsetMax = new Vector2(-8, -4);
        TextMeshProUGUI healthTMP = healthObj.AddComponent<TextMeshProUGUI>();
        healthTMP.text = "HP: 200";
        healthTMP.fontSize = 20;
        healthTMP.color = new Color(0.4f, 0.95f, 0.4f);
        healthTMP.fontStyle = FontStyles.Bold;
        healthTMP.outlineWidth = 0.25f;
        healthTMP.outlineColor = Color.black;

        // Health bar background
        GameObject hpBarBg = new GameObject("HPBarBG");
        hpBarBg.transform.SetParent(hpPanelObj.transform, false);
        var hpBgRect = hpBarBg.AddComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 0.48f);
        hpBgRect.offsetMin = new Vector2(8, 6); hpBgRect.offsetMax = new Vector2(-8, 0);
        var hpBgImg = hpBarBg.AddComponent<UnityEngine.UI.Image>();
        hpBgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Health bar fill
        GameObject hpBarFill = new GameObject("HPBarFill");
        hpBarFill.transform.SetParent(hpBarBg.transform, false);
        var hpFillRect = hpBarFill.AddComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero; hpFillRect.anchorMax = Vector2.one;
        hpFillRect.offsetMin = new Vector2(2, 2); hpFillRect.offsetMax = new Vector2(-2, -2);
        UnityEngine.UI.Image fillImg = hpBarFill.AddComponent<UnityEngine.UI.Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.2f);
        fillImg.type = UnityEngine.UI.Image.Type.Filled;
        fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

        // ── CENTER: Wave announcement ──────────────────────────────────────────
        GameObject announcementObj = new GameObject("AnnouncementText");
        announcementObj.transform.SetParent(canvasObj.transform, false);
        var annRect = announcementObj.AddComponent<RectTransform>();
        annRect.anchorMin = new Vector2(0.1f, 0.5f);
        annRect.anchorMax = new Vector2(0.9f, 0.66f);
        annRect.offsetMin = annRect.offsetMax = Vector2.zero;
        var annTMP = announcementObj.AddComponent<TextMeshProUGUI>();
        annTMP.text = "";
        annTMP.fontSize = 44;
        annTMP.color = new Color(1f, 0.92f, 0.20f);
        annTMP.alignment = TextAlignmentOptions.Center;
        annTMP.fontStyle = FontStyles.Bold;
        annTMP.outlineWidth = 0.35f;
        annTMP.outlineColor = new Color(0.4f, 0.2f, 0f, 0.9f);
        announcementObj.SetActive(false);

        // ── BOTTOM-CENTER: Combo display ───────────────────────────────────────
        GameObject comboObj = new GameObject("ComboText");
        comboObj.transform.SetParent(canvasObj.transform, false);
        var comboRect = comboObj.AddComponent<RectTransform>();
        comboRect.anchorMin = new Vector2(0.3f, 0.0f);
        comboRect.anchorMax = new Vector2(0.7f, 0.10f);
        comboRect.offsetMin = comboRect.offsetMax = Vector2.zero;
        var comboTMP = comboObj.AddComponent<TextMeshProUGUI>();
        comboTMP.text = "x2 COMBO";
        comboTMP.fontSize = 30;
        comboTMP.color = new Color(1f, 0.55f, 0.1f);
        comboTMP.alignment = TextAlignmentOptions.Bottom;
        comboTMP.fontStyle = FontStyles.Bold;
        comboTMP.outlineWidth = 0.3f;
        comboTMP.outlineColor = new Color(0.35f, 0.1f, 0f, 0.9f);
        comboObj.SetActive(false);

        // ── BOTTOM-LEFT: Quick-ref controls hint ──────────────────────────────
        CreateUIText("ControlsHint", canvasObj.transform,
            new Vector2(14, 12), new Vector2(480, 72),
            "WASD/Arrows: Move  |  Space: Jump  |  Click: Shoot\n" +
            "Right Click: Grenade  |  Q: Quack  |  Shift: Dash  |  R: Restart",
            TextAnchor.LowerLeft, 14, new Color(0.78f, 0.78f, 0.78f, 0.55f));

        // ── GAME OVER PANEL ────────────────────────────────────────────────────
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        var goRect = gameOverPanel.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero; goRect.anchorMax = Vector2.one;
        goRect.offsetMin = goRect.offsetMax = Vector2.zero;
        var goImg = gameOverPanel.AddComponent<UnityEngine.UI.Image>();
        goImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Inner card
        GameObject goCard = new GameObject("GameOverCard");
        goCard.transform.SetParent(gameOverPanel.transform, false);
        var goCardRect = goCard.AddComponent<RectTransform>();
        goCardRect.anchorMin = new Vector2(0.25f, 0.20f);
        goCardRect.anchorMax = new Vector2(0.75f, 0.82f);
        goCardRect.offsetMin = goCardRect.offsetMax = Vector2.zero;
        var goCardImg = goCard.AddComponent<UnityEngine.UI.Image>();
        goCardImg.color = new Color(0.06f, 0.06f, 0.10f, 0.92f);
        var goOutline = goCard.AddComponent<UnityEngine.UI.Outline>();
        goOutline.effectColor = new Color(0.85f, 0.70f, 0.10f, 0.8f);
        goOutline.effectDistance = new Vector2(3, -3);

        // Game-over header
        GameObject goHeader = new GameObject("GameOverHeader");
        goHeader.transform.SetParent(goCard.transform, false);
        var goHdrRect = goHeader.AddComponent<RectTransform>();
        goHdrRect.anchorMin = new Vector2(0, 0.78f);
        goHdrRect.anchorMax = new Vector2(1, 1);
        goHdrRect.offsetMin = goHdrRect.offsetMax = Vector2.zero;
        var goHdrImg = goHeader.AddComponent<UnityEngine.UI.Image>();
        goHdrImg.color = new Color(0.55f, 0.08f, 0.08f, 0.9f);

        GameObject goHeaderText = new GameObject("GameOverHeaderText");
        goHeaderText.transform.SetParent(goHeader.transform, false);
        var goHtRect = goHeaderText.AddComponent<RectTransform>();
        goHtRect.anchorMin = Vector2.zero; goHtRect.anchorMax = Vector2.one;
        goHtRect.offsetMin = goHtRect.offsetMax = Vector2.zero;
        var goHtTMP = goHeaderText.AddComponent<TextMeshProUGUI>();
        goHtTMP.text = "GAME OVER";
        goHtTMP.fontSize = 52;
        goHtTMP.color = Color.white;
        goHtTMP.alignment = TextAlignmentOptions.Center;
        goHtTMP.fontStyle = FontStyles.Bold;
        goHtTMP.outlineWidth = 0.2f;
        goHtTMP.outlineColor = new Color(0.3f, 0f, 0f, 0.8f);

        // Score / stats body
        GameObject gameOverText = new GameObject("GameOverScoreText");
        gameOverText.transform.SetParent(goCard.transform, false);
        var goTextRect = gameOverText.AddComponent<RectTransform>();
        goTextRect.anchorMin = new Vector2(0, 0);
        goTextRect.anchorMax = new Vector2(1, 0.78f);
        goTextRect.offsetMin = new Vector2(20, 12); goTextRect.offsetMax = new Vector2(-20, -12);
        var goTMP = gameOverText.AddComponent<TextMeshProUGUI>();
        goTMP.text = "FINAL SCORE: 0\n\nWave Reached: 1\nEnemies Defeated: 0\n\nPress R to play again  |  ESC for main menu";
        goTMP.fontSize = 28;
        goTMP.color = new Color(0.90f, 0.90f, 0.88f);
        goTMP.alignment = TextAlignmentOptions.Center;
        goTMP.textWrappingMode = TextWrappingModes.Normal;

        gameOverPanel.SetActive(false);

        // ── CROSSHAIR cursor ──────────────────────────────────────────────────
        Sprite crosshairSprite = CreateCrosshairSprite();
        GameObject cursorObj = new GameObject("CursorCrosshair");
        cursorObj.transform.SetParent(canvasObj.transform, false);
        RectTransform cursorRect = cursorObj.AddComponent<RectTransform>();
        cursorRect.sizeDelta = new Vector2(32, 32);
        cursorRect.pivot = new Vector2(0.5f, 0.5f);
        cursorRect.anchorMin = new Vector2(0, 0);
        cursorRect.anchorMax = new Vector2(0, 0);
        UnityEngine.UI.Image cursorImg = cursorObj.AddComponent<UnityEngine.UI.Image>();
        if (crosshairSprite != null) cursorImg.sprite = crosshairSprite;
        else cursorImg.color = new Color(1f, 0.9f, 0f, 0.9f);
        cursorObj.transform.SetAsLastSibling();

        // ── UIManager references ──────────────────────────────────────────────
        UIManager uiManager = canvasObj.AddComponent<UIManager>();
        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("scoreText").objectReferenceValue        = scoreTMP;
        uiSO.FindProperty("waveText").objectReferenceValue         = waveObj.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("healthText").objectReferenceValue       = healthTMP;
        uiSO.FindProperty("healthBar").objectReferenceValue        = fillImg;
        uiSO.FindProperty("announcementText").objectReferenceValue = annTMP;
        uiSO.FindProperty("gameOverPanel").objectReferenceValue    = gameOverPanel;
        uiSO.FindProperty("gameOverScoreText").objectReferenceValue= goTMP;
        uiSO.FindProperty("comboText").objectReferenceValue        = comboTMP;
        uiSO.FindProperty("cursorImage").objectReferenceValue      = cursorRect;
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

        // Derive anchor and pivot directly from the TextAnchor enum
        Vector2 anchorVec = GetAnchorFromTextAnchor(anchor);
        rect.anchorMin = anchorVec;
        rect.anchorMax = anchorVec;
        rect.pivot = anchorVec;
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

    private static Vector2 GetAnchorFromTextAnchor(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:    return new Vector2(0,    1);
            case TextAnchor.UpperCenter:  return new Vector2(0.5f, 1);
            case TextAnchor.UpperRight:   return new Vector2(1,    1);
            case TextAnchor.MiddleLeft:   return new Vector2(0,    0.5f);
            case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
            case TextAnchor.MiddleRight:  return new Vector2(1,    0.5f);
            case TextAnchor.LowerLeft:    return new Vector2(0,    0);
            case TextAnchor.LowerCenter:  return new Vector2(0.5f, 0);
            case TextAnchor.LowerRight:   return new Vector2(1,    0);
            default:                      return new Vector2(0.5f, 0.5f);
        }
    }

    private static Sprite CreateCrosshairSprite()
    {
        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        Color c = new Color(1f, 0.9f, 0f, 0.95f); // bright yellow
        int cx = size / 2, cy = size / 2;

        // Circle ring (radius 11–13)
        for (int a = 0; a < 360; a++)
        {
            float rad = a * Mathf.Deg2Rad;
            for (int r = 11; r <= 13; r++)
            {
                int px = cx + Mathf.RoundToInt(r * Mathf.Cos(rad));
                int py = cy + Mathf.RoundToInt(r * Mathf.Sin(rad));
                if (px >= 0 && px < size && py >= 0 && py < size)
                    pixels[py * size + px] = c;
            }
        }

        // Cross lines with center gap (gap 3–9, total arm length to ring)
        for (int t = -1; t <= 1; t++)
        {
            for (int d = 3; d <= 9; d++)
            {
                if (cx + d < size) pixels[(cy + t) * size + (cx + d)] = c;
                if (cx - d >= 0)   pixels[(cy + t) * size + (cx - d)] = c;
                if (cy + d < size) pixels[(cy + d) * size + (cx + t)] = c;
                if (cy - d >= 0)   pixels[(cy - d) * size + (cx + t)] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        if (!System.IO.Directory.Exists("Assets/Sprites"))
            System.IO.Directory.CreateDirectory("Assets/Sprites");

        string path = "Assets/Sprites/Crosshair.png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void CreateBackground()
    {
        // Parallax container
        GameObject bgContainer = new GameObject("Background_Parallax");
        ParallaxBackground parallax = bgContainer.AddComponent<ParallaxBackground>();

        // Layer 0: Gradient sky (deep blue at top -> warm blue at horizon)
        GameObject sky = CreateBgLayer("BG_Sky", new Color(0.38f, 0.60f, 0.92f), -12, 0f, -2f);
        sky.transform.SetParent(bgContainer.transform);
        sky.transform.localScale = new Vector3(120f, 60f, 1f);

        // Layer 1: Distant city silhouette (dark blue-grey)
        GameObject buildings = CreateBgLayer("BG_Buildings", new Color(0.22f, 0.24f, 0.35f), -8, 0.08f, 0f);
        buildings.transform.SetParent(bgContainer.transform);
        buildings.transform.localScale = new Vector3(70f, 18f, 1f);
        buildings.transform.position = new Vector3(0f, 3.5f, 0f);

        // Layer 2: Mid-ground hills (soft green)
        GameObject mid = CreateBgLayer("BG_Mid", new Color(0.28f, 0.44f, 0.22f), -5, 0.35f, 0f);
        mid.transform.SetParent(bgContainer.transform);
        mid.transform.localScale = new Vector3(50f, 6f, 1f);
        mid.transform.position = new Vector3(0f, -0.5f, 0f);

        // Layer 3: Near bushes (darker green, fast parallax)
        GameObject near = CreateBgLayer("BG_Near", new Color(0.18f, 0.32f, 0.12f), -3, 0.65f, 0f);
        near.transform.SetParent(bgContainer.transform);
        near.transform.localScale = new Vector3(40f, 2f, 1f);
        near.transform.position = new Vector3(0f, -2f, 0f);

        // Wire layers into ParallaxBackground
        SerializedObject pso = new SerializedObject(parallax);
        SerializedProperty layersProp = pso.FindProperty("layers");
        layersProp.arraySize = 4;

        SetParallaxLayer(layersProp.GetArrayElementAtIndex(0), sky.transform,       0f,    false);
        SetParallaxLayer(layersProp.GetArrayElementAtIndex(1), buildings.transform, 0.08f, true);
        SetParallaxLayer(layersProp.GetArrayElementAtIndex(2), mid.transform,       0.35f, true);
        SetParallaxLayer(layersProp.GetArrayElementAtIndex(3), near.transform,      0.65f, true);

        pso.ApplyModifiedProperties();
    }

    private static GameObject CreateBgLayer(string name, Color color, int sortOrder, float parallaxFactor, float offsetZ)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingLayerName = "Background";
        sr.sortingOrder = sortOrder;
        go.transform.position = new Vector3(0f, 0f, offsetZ);
        return go;
    }

    private static void SetParallaxLayer(SerializedProperty layerProp, Transform t, float factor, bool loop)
    {
        layerProp.FindPropertyRelative("layerTransform").objectReferenceValue = t;
        layerProp.FindPropertyRelative("parallaxFactor").floatValue = factor;
        layerProp.FindPropertyRelative("loopHorizontally").boolValue = loop;
    }

    // ── START MENU SCENE ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds Assets/Scenes/MainMenu.unity - a stylish start-menu scene.
    /// </summary>
    private static void BuildStartMenuScene()
    {
        // Create a new empty scene
        var menuScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
            UnityEditor.SceneManagement.NewSceneMode.Single);

        // ── Camera ──
        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.20f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camObj.tag = "MainCamera";

        // ── Gradient sky backdrop via camera background color ──
        cam.backgroundColor = new Color(0.12f, 0.16f, 0.28f);

        // ── Stars (simple dots) ──
        CreateMenuStars();

        // ── Canvas ──
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── Semi-transparent dark overlay panel (fills screen) ──
        CreateMenuPanel("Overlay", canvasObj.transform,
            Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.45f));

        // ── Title background banner ──
        GameObject titleBanner = CreateMenuPanel("TitleBanner", canvasObj.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0.12f, 0.10f, 0.06f, 0.80f));
        RectTransform tbRect = titleBanner.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0.1f, 0.60f);
        tbRect.anchorMax = new Vector2(0.9f, 0.95f);
        tbRect.offsetMin = tbRect.offsetMax = Vector2.zero;
        // Gold border
        AddMenuOutline(titleBanner, new Color(0.90f, 0.72f, 0.10f, 0.9f));

        // ── Title text ──
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.68f);
        titleRect.anchorMax = new Vector2(0.9f, 0.94f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
        var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "<b>DUCK\nREVOLUTION</b>";
        titleTMP.fontSize = 96;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.88f, 0.12f);
        titleTMP.outlineWidth = 0.4f;
        titleTMP.outlineColor = new Color(0.55f, 0.35f, 0f, 1f);

        // ── Subtitle ──
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(canvasObj.transform, false);
        var subRect = subObj.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.2f, 0.60f);
        subRect.anchorMax = new Vector2(0.8f, 0.68f);
        subRect.offsetMin = subRect.offsetMax = Vector2.zero;
        var subTMP = subObj.AddComponent<TextMeshProUGUI>();
        subTMP.text = "The city will fear the waddle.";
        subTMP.fontSize = 28;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.90f, 0.88f, 0.75f, 0.85f);
        subTMP.fontStyle = FontStyles.Italic;

        // ── High score ──
        GameObject hsObj = new GameObject("HighScoreText");
        hsObj.transform.SetParent(canvasObj.transform, false);
        var hsRect = hsObj.AddComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0.25f, 0.54f);
        hsRect.anchorMax = new Vector2(0.75f, 0.60f);
        hsRect.offsetMin = hsRect.offsetMax = Vector2.zero;
        var hsTMP = hsObj.AddComponent<TextMeshProUGUI>();
        hsTMP.text = "<color=#FFD700>* Best Score: 0</color>";
        hsTMP.fontSize = 26;
        hsTMP.alignment = TextAlignmentOptions.Center;

        // ── PLAY button ──
        GameObject playBtn = CreateMenuButton("PlayButton", canvasObj.transform,
            new Vector2(0.35f, 0.36f), new Vector2(0.65f, 0.50f),
            "PLAY",
            new Color(0.18f, 0.62f, 0.18f), new Color(0.24f, 0.82f, 0.24f),
            new Color(1f, 1f, 1f), 42);

        // ── Controls panel ──
        GameObject ctrlPanel = CreateMenuPanel("ControlsPanel", canvasObj.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Color(0.05f, 0.06f, 0.10f, 0.75f));
        RectTransform ctrlRect = ctrlPanel.GetComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(0.05f, 0.04f);
        ctrlRect.anchorMax = new Vector2(0.50f, 0.34f);
        ctrlRect.offsetMin = ctrlRect.offsetMax = Vector2.zero;
        AddMenuOutline(ctrlPanel, new Color(0.40f, 0.45f, 0.60f, 0.6f));

        GameObject ctrlTextObj = new GameObject("ControlsText");
        ctrlTextObj.transform.SetParent(ctrlPanel.transform, false);
        var ctrlTextRect = ctrlTextObj.AddComponent<RectTransform>();
        ctrlTextRect.anchorMin = Vector2.zero;
        ctrlTextRect.anchorMax = Vector2.one;
        ctrlTextRect.offsetMin = new Vector2(12, 8);
        ctrlTextRect.offsetMax = new Vector2(-12, -8);
        var ctrlTMP = ctrlTextObj.AddComponent<TextMeshProUGUI>();
        ctrlTMP.text =
            "<size=80%><color=#BBBBDD><b>Controls</b></color>\n</size>" +
            "<size=70%>A / D  |  <- ->  - Move\n" +
            "Space  |  W  - Jump\n" +
            "Left Click  - Shoot\n" +
            "Right Click  - Egg Grenade\n" +
            "Q  - Quack (Stun)\n" +
            "Shift  - Wing Dash\n" +
            "S (air)  - Ground Pound\n" +
            "R  - Restart  |  ESC  - Menu</size>";
        ctrlTMP.fontSize = 22;
        ctrlTMP.color = new Color(0.88f, 0.88f, 0.88f);
        ctrlTMP.textWrappingMode = TextWrappingModes.Normal;

        // ── Version / credit ──
        GameObject verObj = new GameObject("VersionText");
        verObj.transform.SetParent(canvasObj.transform, false);
        var verRect = verObj.AddComponent<RectTransform>();
        verRect.anchorMin = new Vector2(0.7f, 0.04f);
        verRect.anchorMax = new Vector2(1f, 0.08f);
        verRect.offsetMin = verRect.offsetMax = Vector2.zero;
        var verTMP = verObj.AddComponent<TextMeshProUGUI>();
        verTMP.text = "v1.0  |  Duck Revolution";
        verTMP.fontSize = 16;
        verTMP.color = new Color(0.55f, 0.55f, 0.55f, 0.7f);
        verTMP.alignment = TextAlignmentOptions.BottomRight;

        // ── StartMenuManager on the canvas ──
        StartMenuManager smMgr = canvasObj.AddComponent<StartMenuManager>();
        SerializedObject smSO = new SerializedObject(smMgr);
        smSO.FindProperty("titleText").objectReferenceValue       = titleTMP;
        smSO.FindProperty("subtitleText").objectReferenceValue    = subTMP;
        smSO.FindProperty("highScoreText").objectReferenceValue   = hsTMP;
        smSO.FindProperty("controlsText").objectReferenceValue    = ctrlTMP;
        smSO.FindProperty("playButton").objectReferenceValue      = playBtn.GetComponent<UnityEngine.UI.Button>();
        smSO.FindProperty("playButtonLabel").objectReferenceValue = playBtn.GetComponentInChildren<TextMeshProUGUI>();
        smSO.ApplyModifiedProperties();

        // ── Wire play button click ──
        UnityEngine.UI.Button btnComp = playBtn.GetComponent<UnityEngine.UI.Button>();
        if (btnComp != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btnComp.onClick,
                smMgr.PlayGame);
        }

        // Save
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(menuScene, "Assets/Scenes/MainMenu.unity");
        Debug.Log("Main menu scene created: Assets/Scenes/MainMenu.unity");
    }

    private static void CreateMenuStars()
    {
        // Load the pre-generated star sprite
        Sprite starSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Star.png");

        // Scatter small star sprites across the background
        System.Random rng = new System.Random(42);
        GameObject starsParent = new GameObject("Stars");
        for (int i = 0; i < 40; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(starsParent.transform);
            float x = (float)(rng.NextDouble() * 40 - 20);
            float y = (float)(rng.NextDouble() * 20 - 5);
            star.transform.position = new Vector3(x, y, 0f);
            float sz = (float)(rng.NextDouble() * 0.12f + 0.05f);
            star.transform.localScale = Vector3.one * sz;
            var sr = star.AddComponent<SpriteRenderer>();
            if (starSprite != null) sr.sprite = starSprite;
            float bright = (float)(rng.NextDouble() * 0.4f + 0.6f);
            sr.color = new Color(bright, bright, bright * 0.9f, bright * 0.9f);
            sr.sortingOrder = -9;
        }
    }

    /// <summary>Create a UI panel (Image) anchored to given min/max fractions.</summary>
    private static GameObject CreateMenuPanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        return go;
    }

    /// <summary>Add a thin UnityEngine.UI.Outline component for a border effect.</summary>
    private static void AddMenuOutline(GameObject go, Color color)
    {
        var outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(2, -2);
    }

    /// <summary>Create a styled UI Button with a label.</summary>
    private static GameObject CreateMenuButton(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        string label, Color normalColor, Color hoverColor,
        Color textColor, int fontSize)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);
        var rect = btnGo.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var img = btnGo.AddComponent<UnityEngine.UI.Image>();
        img.color = normalColor;

        var btn = btnGo.AddComponent<UnityEngine.UI.Button>();
        var colors = btn.colors;
        colors.normalColor      = normalColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor     = normalColor * 0.8f;
        colors.selectedColor    = normalColor;
        btn.colors = colors;

        // Label
        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(btnGo.transform, false);
        var lblRect = lblGo.AddComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = lblRect.offsetMax = Vector2.zero;

        var lbl = lblGo.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = fontSize;
        lbl.color = textColor;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.fontStyle = FontStyles.Bold;
        lbl.outlineWidth = 0.15f;
        lbl.outlineColor = new Color(0f, 0.3f, 0f, 0.8f);

        // Rounded look via outline
        AddMenuOutline(btnGo, new Color(1f, 1f, 1f, 0.3f));

        return btnGo;
    }

    // ── BUILD SETTINGS ────────────────────────────────────────────────────────

    private static void RegisterScenesInBuildSettings()
    {
        string menuPath  = "Assets/Scenes/MainMenu.unity";
        string gamePath  = "Assets/Scenes/DuckRevolution.unity";

        var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        bool hasMenu = false, hasGame = false;
        foreach (var s in existing)
        {
            if (s.path == menuPath) hasMenu = true;
            if (s.path == gamePath) hasGame = true;
        }

        // Refresh so newly-saved .unity files are visible to File.Exists
        AssetDatabase.Refresh();

        if (!hasMenu && System.IO.File.Exists(menuPath))
            existing.Insert(0, new EditorBuildSettingsScene(menuPath, true));
        if (!hasGame && System.IO.File.Exists(gamePath))
        {
            // Make sure game scene is index 1
            bool alreadyPresent = false;
            foreach (var s in existing) if (s.path == gamePath) alreadyPresent = true;
            if (!alreadyPresent) existing.Add(new EditorBuildSettingsScene(gamePath, true));
        }

        EditorBuildSettings.scenes = existing.ToArray();
        Debug.Log("Build settings updated with MainMenu + DuckRevolution scenes.");
    }
}
