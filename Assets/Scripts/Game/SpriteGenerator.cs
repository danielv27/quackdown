using UnityEngine;

/// <summary>
/// Generates simple colored pixel-art placeholder sprites at runtime.
/// Attach to a GameObject in the scene or call statically from other scripts.
///
/// This removes the need for external art assets so you can press Play immediately.
/// Replace sprites with real pixel art later without touching any other code.
/// </summary>
public static class SpriteGenerator
{
    // ---- Duck (player) ----
    public static Sprite CreateDuckSprite()
    {
        int w = 16, h = 16;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);

        Color yellow = new Color(1f, 0.85f, 0f);
        Color orange = new Color(1f, 0.5f, 0f);
        Color white  = Color.white;
        Color black  = Color.black;

        // Body (rows 2-9, cols 3-12)
        FillRect(pixels, w, 3, 2, 10, 8, yellow);
        // Head (rows 10-14, cols 5-11)
        FillRect(pixels, w, 5, 9, 7, 5, yellow);
        // Eye
        SetPixel(pixels, w, 9, 12, black);
        SetPixel(pixels, w, 10, 12, white);
        // Bill
        FillRect(pixels, w, 11, 11, 3, 2, orange);
        // Wing hint
        FillRect(pixels, w, 4, 4, 4, 2, new Color(0.9f, 0.75f, 0f));
        // Helmet (military duck)
        FillRect(pixels, w, 5, 13, 7, 2, new Color(0.3f, 0.6f, 0.3f));
        // Outline
        OutlineSprite(pixels, w, h, black);

        return MakeSprite(pixels, w, h);
    }

    // ---- Police enemy ----
    public static Sprite CreatePoliceSprite()
    {
        int w = 16, h = 16;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);

        Color blue   = new Color(0.2f, 0.4f, 0.9f);
        Color skin   = new Color(1f, 0.85f, 0.7f);
        Color black  = Color.black;
        Color yellow = Color.yellow;

        // Body
        FillRect(pixels, w, 3, 2, 10, 8, blue);
        // Head
        FillRect(pixels, w, 5, 9, 6, 5, skin);
        // Police cap
        FillRect(pixels, w, 4, 13, 8, 3, black);
        FillRect(pixels, w, 5, 15, 6, 1, blue);
        // Badge
        SetPixel(pixels, w, 5, 7, yellow);
        SetPixel(pixels, w, 6, 7, yellow);
        // Eyes
        SetPixel(pixels, w, 6, 11, black);
        SetPixel(pixels, w, 9, 11, black);
        // Outline
        OutlineSprite(pixels, w, h, black);

        return MakeSprite(pixels, w, h);
    }

    // ---- SWAT enemy ----
    public static Sprite CreateSwatSprite()
    {
        int w = 16, h = 16;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);

        Color dark   = new Color(0.15f, 0.15f, 0.15f);
        Color black  = Color.black;
        Color white  = Color.white;

        // Body + armour
        FillRect(pixels, w, 2, 2, 12, 9, dark);
        FillRect(pixels, w, 3, 4, 10, 6, new Color(0.25f, 0.25f, 0.25f)); // chest plate
        // Helmet
        FillRect(pixels, w, 4, 10, 8, 6, black);
        // Visor
        FillRect(pixels, w, 5, 11, 6, 2, new Color(0.6f, 0.8f, 1f, 0.9f));
        // Eyes behind visor
        SetPixel(pixels, w, 6, 12, white);
        SetPixel(pixels, w, 9, 12, white);
        // Outline
        OutlineSprite(pixels, w, h, black);

        return MakeSprite(pixels, w, h);
    }

    // ---- Army soldier enemy ----
    public static Sprite CreateArmySprite()
    {
        int w = 16, h = 16;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);

        Color green = new Color(0.2f, 0.45f, 0.2f);
        Color tan   = new Color(0.7f, 0.65f, 0.4f);
        Color black = Color.black;
        Color skin  = new Color(1f, 0.85f, 0.7f);

        // Body (camouflage hint)
        FillRect(pixels, w, 3, 2, 10, 8, green);
        FillRect(pixels, w, 4, 3, 3, 2, tan);
        FillRect(pixels, w, 8, 5, 3, 2, tan);
        // Head
        FillRect(pixels, w, 5, 9, 6, 5, skin);
        // Helmet
        FillRect(pixels, w, 4, 13, 8, 3, green);
        FillRect(pixels, w, 3, 14, 10, 1, green);
        // Eyes
        SetPixel(pixels, w, 6, 11, black);
        SetPixel(pixels, w, 9, 11, black);
        // Rifle silhouette
        FillRect(pixels, w, 12, 6, 3, 1, new Color(0.3f, 0.2f, 0.1f));
        // Outline
        OutlineSprite(pixels, w, h, black);

        return MakeSprite(pixels, w, h);
    }

    // ---- Bullet ----
    public static Sprite CreateBulletSprite()
    {
        int w = 8, h = 4;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);
        Color yellow = Color.yellow;
        Color black  = Color.black;

        FillRect(pixels, w, 1, 1, 6, 2, yellow);
        SetPixel(pixels, w, 6, 1, new Color(1f, 0.5f, 0f));
        SetPixel(pixels, w, 7, 1, new Color(1f, 0.3f, 0f));
        OutlineSprite(pixels, w, h, black);
        return MakeSprite(pixels, w, h);
    }

    // ---- Egg grenade ----
    public static Sprite CreateEggSprite()
    {
        int w = 10, h = 12;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);
        Color eggWhite = new Color(1f, 0.98f, 0.9f);
        Color black    = Color.black;

        // Oval egg shape
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float cx = (x - w / 2f + 0.5f) / (w / 2f);
            float cy = (y - h * 0.45f)      / (h * 0.55f);
            if (cx * cx + cy * cy < 0.9f)
                pixels[y * w + x] = eggWhite;
        }
        OutlineSprite(pixels, w, h, black);
        return MakeSprite(pixels, w, h);
    }

    // ---- Ground tile ----
    public static Sprite CreateGroundSprite()
    {
        int w = 32, h = 16;
        Color[] pixels = new Color[w * h];
        Color top  = new Color(0.3f, 0.7f, 0.3f);
        Color dirt = new Color(0.5f, 0.35f, 0.2f);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            pixels[y * w + x] = (y >= h - 4) ? top : dirt;

        // Simple tile pattern
        for (int x = 0; x < w; x += 8)
            for (int y = 0; y < h - 4; y++)
                pixels[y * w + x] = new Color(0.4f, 0.28f, 0.15f);

        return MakeSprite(pixels, w, h);
    }

    // ---- Crate ----
    public static Sprite CreateCrateSprite()
    {
        int w = 16, h = 16;
        Color[] pixels = new Color[w * h];
        Color wood  = new Color(0.6f, 0.45f, 0.25f);
        Color plank = new Color(0.5f, 0.37f, 0.2f);
        Color black = Color.black;

        SetAll(pixels, wood);
        // Planks
        for (int x = 0; x < w; x++)
        {
            pixels[0 * w + x] = plank;
            pixels[5 * w + x] = plank;
            pixels[10 * w + x]= plank;
            pixels[15 * w + x]= plank;
        }
        for (int y = 0; y < h; y++)
        {
            pixels[y * w + 0]  = plank;
            pixels[y * w + 5]  = plank;
            pixels[y * w + 10] = plank;
            pixels[y * w + 15] = plank;
        }
        OutlineSprite(pixels, w, h, black);
        return MakeSprite(pixels, w, h);
    }

    // ---- Barrel ----
    public static Sprite CreateBarrelSprite()
    {
        int w = 12, h = 18;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);
        Color red  = new Color(0.8f, 0.15f, 0.1f);
        Color band = new Color(0.3f, 0.3f, 0.3f);
        Color black= Color.black;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float cx = (x - w / 2f + 0.5f) / (w / 2f);
            // Barrel silhouette (slightly wider in middle)
            float hw = 0.5f + 0.1f * Mathf.Sin(y / (float)h * Mathf.PI);
            if (Mathf.Abs(cx) < hw)
                pixels[y * w + x] = red;
        }
        // Metal bands
        for (int x = 1; x < w - 1; x++)
        {
            if (pixels[3 * w + x] == red)  pixels[3 * w + x]  = band;
            if (pixels[14 * w + x]== red)  pixels[14 * w + x] = band;
        }
        OutlineSprite(pixels, w, h, black);
        return MakeSprite(pixels, w, h);
    }

    // ---- Background ----
    public static Sprite CreateBackgroundSprite(int width = 256, int height = 144)
    {
        Color[] pixels = new Color[width * height];
        // Sky gradient
        for (int y = 0; y < height; y++)
        {
            float t = y / (float)height;
            Color sky = Color.Lerp(new Color(0.4f, 0.6f, 1f), new Color(0.7f, 0.85f, 1f), t);
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = sky;
        }
        return MakeSprite(pixels, width, height);
    }

    // ---- Explosion FX ----
    public static Sprite CreateExplosionSprite()
    {
        int w = 24, h = 24;
        Color[] pixels = new Color[w * h];
        SetAll(pixels, Color.clear);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dx = x - w / 2f;
            float dy = y - h / 2f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist < w / 2f)
            {
                float t = dist / (w / 2f);
                pixels[y * w + x] = Color.Lerp(Color.white, new Color(1f, 0.3f, 0f, 0f), t);
            }
        }
        return MakeSprite(pixels, w, h);
    }

    // ================================================================
    // ---- Internal Helpers ----
    // ================================================================

    private static void FillRect(Color[] pixels, int w, int x, int y, int rw, int rh, Color c)
    {
        for (int dy = 0; dy < rh; dy++)
        for (int dx = 0; dx < rw; dx++)
        {
            int px = x + dx, py = y + dy;
            if (px >= 0 && px < w && py >= 0 && py < pixels.Length / w)
                pixels[py * w + px] = c;
        }
    }

    private static void SetPixel(Color[] pixels, int w, int x, int y, Color c)
    {
        int h = pixels.Length / w;
        if (x >= 0 && x < w && y >= 0 && y < h)
            pixels[y * w + x] = c;
    }

    private static void SetAll(Color[] pixels, Color c)
    {
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
    }

    private static void OutlineSprite(Color[] pixels, int w, int h, Color outline)
    {
        Color[] copy = (Color[])pixels.Clone();
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (copy[y * w + x].a < 0.5f)
            {
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && copy[ny * w + nx].a > 0.5f)
                    {
                        pixels[y * w + x] = outline;
                        break;
                    }
                }
            }
        }
    }

    private static Sprite MakeSprite(Color[] pixels, int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, w, h),
            new Vector2(0.5f, 0.5f),
            16f);          // 16 pixels per unit for chunky pixel art
    }
}
