using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    const int GlyphW = 7;
    const int GlyphH = 8;
    const float PixelsPerUnit = 16f;
    const float HopTime = 0.1f;
    const float Lifetime = 0.42f;
    const float HopHeight = 0.7f;

    static Sprite[] glyphs;
    static Material spriteMaterial;

    Vector3 start;
    Vector3 end;
    float age;

    static readonly string[][] GlyphRows =
    {
        new[] { "..###..", ".#...#.", "#.....#", "#.....#", "#.....#", "#.....#", ".#...#.", "..###.." },
        new[] { ".....##", "...####", "..#####", ".######", "...####", "...####", "...####", "...####" },
        new[] { ".#####.", "#.....#", ".....##", "....##.", "...##..", "..##...", ".#.....", "#######" },
        new[] { ".#####.", "#.....#", ".....##", "..####.", ".....##", ".....##", "#.....#", ".#####." },
        new[] { "....##.", "...###.", "..#.##.", ".#..##.", "#...##.", "#######", "....##.", "....##." },
        new[] { "#######", "#......", "#......", "######.", ".....##", ".....##", "#.....#", ".#####." },
        new[] { "..####.", ".#.....", "#......", "######.", "#.....#", "#.....#", "#.....#", ".#####." },
        new[] { "#######", ".....##", "....##.", "...##..", "..##...", ".##....", ".##....", ".##...." },
        new[] { ".#####.", "#.....#", "#.....#", ".#####.", "#.....#", "#.....#", "#.....#", ".#####." },
        new[] { ".#####.", "#.....#", "#.....#", "#.....#", ".######", ".....##", "....##.", "..####." },
        new[] { ".......", ".......", ".......", ".#####.", ".#####.", ".......", ".......", "......." },
    };

    public static void Show(Vector3 position, int amount)
    {
        if (amount <= 0)
            return;

        EnsureFont();

        GameObject popup = new GameObject("DamagePopup");
        Vector3 origin = position + Vector3.up * 0.2f + Vector3.right * Random.Range(-0.06f, 0.06f);
        popup.transform.position = origin;

        string text = "-" + amount;
        float advance = (GlyphW + 1) / PixelsPerUnit;
        float total = text.Length * advance - (1f / PixelsPerUnit);
        float x = -total * 0.5f;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            int index = c == '-' ? 10 : c - '0';
            if (index < 0 || index >= glyphs.Length)
                continue;

            GameObject digit = new GameObject("Digit");
            digit.transform.SetParent(popup.transform, false);
            digit.transform.localPosition = new Vector3(x, 0f, 0f);

            SpriteRenderer renderer = digit.AddComponent<SpriteRenderer>();
            renderer.sprite = glyphs[index];
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingLayerName = "Entities";
            renderer.sortingOrder = 30;
            x += advance;
        }

        DamagePopup motion = popup.AddComponent<DamagePopup>();
        motion.start = origin;
        motion.end = origin + Vector3.up * HopHeight;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / HopTime);
        float eased = 1f - (1f - t) * (1f - t);
        transform.position = Vector3.Lerp(start, end, eased);

        if (age >= Lifetime)
            Destroy(gameObject);
    }

    static void EnsureFont()
    {
        if (glyphs != null)
            return;

        Texture2D texture = new Texture2D(GlyphW * GlyphRows.Length, GlyphH, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(new Color[texture.width * texture.height]);

        Color ink = new Color32(247, 53, 63, 255);
        for (int g = 0; g < GlyphRows.Length; g++)
        {
            string[] rows = GlyphRows[g];
            for (int row = 0; row < GlyphH; row++)
            {
                int y = GlyphH - 1 - row;
                for (int x = 0; x < GlyphW; x++)
                {
                    if (rows[row][x] == '#')
                        texture.SetPixel(g * GlyphW + x, y, ink);
                }
            }
        }

        texture.Apply();

        glyphs = new Sprite[GlyphRows.Length];
        for (int g = 0; g < glyphs.Length; g++)
        {
            glyphs[g] = Sprite.Create(
                texture,
                new Rect(g * GlyphW, 0, GlyphW, GlyphH),
                new Vector2(0f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
        }

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        spriteMaterial = new Material(shader);
    }
}
